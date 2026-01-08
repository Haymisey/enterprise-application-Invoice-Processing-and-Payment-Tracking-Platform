using System.Text.Json;
using System.Net.Http;
using AIClassification.Application.Services;
using AIClassification.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Mscc.GenerativeAI;

namespace AIClassification.Infrastructure.AI
{
    public class GeminiService : IGeminiService
    {
        private readonly string _apiKey;

        // Use a valid model name for Mscc.GenerativeAI
        private const string ModelName = "gemini-2.5-flash";

        public GeminiService(IConfiguration config)
        {
            var key = config["Gemini:ApiKey"];
            
            if (string.IsNullOrEmpty(key) || key.Contains("YOUR_GOOGLE_GEMINI_API_KEY_HERE"))
            {
                throw new ArgumentException("Gemini:ApiKey is not configured! Please set it as an environment variable: export Gemini__ApiKey=\"YOUR_KEY\"");
            }

            _apiKey = key;
        }

        public async Task<(ExtractedInvoiceData data, double confidence)> ExtractInvoiceDataAsync(string imageUrl, CancellationToken ct = default)
        {
            var ai = new GoogleAI(apiKey: _apiKey);
            var model = ai.GenerativeModel(model: ModelName);

            // Prompt for invoice extraction - include the URL so the model can fetch/see it
            var prompt = $@"
Analyze this invoice image: {imageUrl}
Extract the following data in strict JSON format:
{{
  ""invoiceNumber"": ""string"",
  ""vendorName"": ""string"",
  ""totalAmount"": number,
  ""currency"": ""3-letter code (e.g. USD)"",
  ""issueDate"": ""YYYY-MM-DD"",
  ""dueDate"": ""YYYY-MM-DD"",
  ""lineItems"": [""string description""],
  ""confidence"": 0.95
}}
Return ONLY the JSON block.";

            // Call the API with simple string prompt
            var response = await model.GenerateContent(prompt);
            var json = response?.Text?.Trim() ?? throw new Exception("AI returned no text");

            // Clean up Markdown if model returned it
            if (json.Contains("```json"))
            {
                json = json.Split("```json")[1].Split("```")[0].Trim();
            }
            else if (json.Contains("```"))
            {
                json = json.Split("```")[1].Split("```")[0].Trim();
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<GeminiExtractionResult>(json, options)
                         ?? throw new Exception("Failed to parse AI JSON response");

            var data = ExtractedInvoiceData.Create(
                result.InvoiceNumber,
                result.VendorName,
                result.TotalAmount,
                result.Currency,
                result.IssueDate,
                result.DueDate,
                result.LineItems ?? new List<string>());

            return (data, result.Confidence > 0 ? result.Confidence : 0.9);
        }

        public async Task<(bool isFraudulent, string? reason)> DetectFraudAsync(ExtractedInvoiceData data, CancellationToken ct = default)
        {
            var ai = new GoogleAI(apiKey: _apiKey);
            var model = ai.GenerativeModel(model: ModelName);

            var prompt = $@"
Act as a fraud detection expert. Analyze this invoice data for red flags (unusually high amount, suspicious vendor, impossible dates):
Vendor: {data.VendorName}
Amount: {data.TotalAmount} {data.Currency}
Issue Date: {data.IssueDate:yyyy-MM-dd}
Due Date: {data.DueDate:yyyy-MM-dd}

Return JSON: {{ ""isFraudulent"": bool, ""reason"": ""detailed explanation or null"" }}";

            var response = await model.GenerateContent(prompt);
            var json = response?.Text?.Trim() ?? string.Empty;
            
            if (json.Contains("```json")) json = json.Split("```json")[1].Split("```")[0].Trim();
            else if (json.Contains("```")) json = json.Split("```")[1].Split("```")[0].Trim();

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
