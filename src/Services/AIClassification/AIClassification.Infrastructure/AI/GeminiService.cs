using System.Text.Json;
using AIClassification.Application.Services;
using AIClassification.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Mscc.GenerativeAI;

namespace AIClassification.Infrastructure.AI
{
    public class GeminiService : IGeminiService
    {
        private readonly string _apiKey;

        // Use the model enum or string constant as needed
        private const string ModelName = "Gemini06"; // use your preferred model

        public GeminiService(IConfiguration config)
        {
            _apiKey = config["Gemini:ApiKey"]
                      ?? throw new ArgumentNullException("Gemini:ApiKey not configured");
        }

        public async Task<(ExtractedInvoiceData data, double confidence)> ExtractInvoiceDataAsync(string imageUrl, CancellationToken ct = default)
        {
            // Create the client
            var ai = new GoogleAI(apiKey: _apiKey);

            // Create the model
            var model = ai.GenerativeModel(model: ModelName);

            // Prompt for invoice extraction
            var prompt = @"
Analyze this image. It is an invoice. Extract the following data in strict JSON:
{
""invoiceNumber"": ""string or null"",
""vendorName"": ""string or null"",
""currency"": ""string or null"",
""totalAmount"": number or null,
""issueDate"": ""YYYY-MM-DD or null"",
""dueDate"": ""YYYY-MM-DD or null"",
""lineItems"": [""string description""],
""confidence"": number 0.0-1.0
}
Return strict JSON.";

            // Call the API
            var response = await model.GenerateContent(prompt);

            var json = response?.Text?.Trim() ?? throw new Exception("No response text");

            // Clean up if Markdown ``` present
            json = json.Replace("```json", "").Replace("```", "").Trim();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<GeminiExtractionResult>(json, options)
                         ?? throw new Exception("AI response invalid");

            var data = ExtractedInvoiceData.Create(
                result.InvoiceNumber,
                result.VendorName,
                result.TotalAmount,
                result.Currency,
                result.IssueDate,
                result.DueDate,
                result.LineItems);

            return (data, result.Confidence);
        }

        public async Task<(bool isFraudulent, string? reason)> DetectFraudAsync(ExtractedInvoiceData data, CancellationToken ct = default)
        {
            var ai = new GoogleAI(apiKey: _apiKey);
            var model = ai.GenerativeModel(model: ModelName);

            var prompt = $@"
Act as a forensic accountant. Given:
Vendor: {data.VendorName}
Amount: {data.TotalAmount}
Currency: {data.Currency}
IssueDate: {data.IssueDate}

Return JSON: {{""isFraudulent"": bool,""reason"": ""string or null""}}";

            var response = await model.GenerateContent(prompt);

            var json = response?.Text?.Trim() ?? string.Empty;
            json = json.Replace("```json", "").Replace("```", "").Trim();

            var result = JsonSerializer.Deserialize<GeminiFraudResult>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return (result?.IsFraudulent ?? false, result?.Reason);
        }

        private class GeminiExtractionResult
        {
            public string? InvoiceNumber { get; set; }
            public string? VendorName { get; set; }
            public decimal? TotalAmount { get; set; }
            public string? Currency { get; set; }
            public DateTime? IssueDate { get; set; }
            public DateTime? DueDate { get; set; }
            public List<string>? LineItems { get; set; }
            public double Confidence { get; set; }
        }

        private class GeminiFraudResult
        {
            public bool IsFraudulent { get; set; }
            public string? Reason { get; set; }
        }
    }
}
