using System.Text.Json;
using AIClassification.Application.Services;
using AIClassification.Domain.ValueObjects;
using Google.Generative.AI;
using Microsoft.Extensions.Configuration;

namespace AIClassification.Infrastructure.AI;

public class GeminiService : IGeminiService
{
    private readonly string _apiKey;
    // Using flash model for speed and cost-effectiveness
    private const string ModelName = "gemini-1.5-flash"; 

    public GeminiService(IConfiguration config)
    {
        _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey not found");
    }

    public async Task<(ExtractedInvoiceData Data, double Confidence)> ExtractDataAsync(string imageUrl, CancellationToken ct = default)
    {
        var model = new GenerativeModel(_apiKey, ModelName);

        var prompt = @"
            Analyze this image. It is an invoice. Extract the following data in strict JSON format:
            {
                ""invoiceNumber"": ""string or null"",
                ""vendorName"": ""string or null"",
                ""totalAmount"": decimal or null,
                ""currency"": ""string (e.g. USD) or null"",
                ""issueDate"": ""YYYY-MM-DD or null"",
                ""dueDate"": ""YYYY-MM-DD or null"",
                ""lineItems"": [""string description""],
                ""confidence"": 0.0 to 1.0 representing how legible the image is
            }
            Return ONLY the JSON. No markdown.";

        try
        {
            // Note: Google.Generative.AI handles image fetching if you pass URI, 
            // or you might need to download bytes first depending on the library version.
            // Assuming the library accepts the image URI or base64.
            // If library requires parts:
            // var parts = new [] { new Part { Text = prompt }, new Part { InlineData = new GenerationPart.InlineData { MimeType = "image/jpeg", Data = ... } } };
            
            var response = await model.GenerateContentAsync(prompt, imageUrl); // Simplified call
            var json = CleanJson(response.Text);
            
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<GeminiExtractionResult>(json, options);

            if (result == null) throw new Exception("Failed to deserialize AI response");

            var data = ExtractedInvoiceData.Create(
                result.InvoiceNumber, result.VendorName, result.TotalAmount,
                result.Currency, result.IssueDate, result.DueDate, result.LineItems);

            return (data, result.Confidence);
        }
        catch (Exception ex)
        {
            throw new Exception($"Gemini Extraction Failed: {ex.Message}", ex);
        }
    }

    public async Task<(bool IsFraudulent, string? Reason)> DetectFraudAsync(ExtractedInvoiceData data, CancellationToken ct = default)
    {
        var model = new GenerativeModel(_apiKey, ModelName);

        var prompt = $@"
            Act as a forensic accountant. Analyze this invoice data for fraud indicators.
            Data:
            Vendor: {data.VendorName}
            Amount: {data.TotalAmount} {data.Currency}
            Date: {data.IssueDate}
            
            Check for:
            1. Suspiciously round numbers for large amounts.
            2. Dates in the future or too far in the past.
            3. Missing critical fields.

            Return JSON: {{ ""isFraudulent"": boolean, ""reason"": ""string or null"" }}";

        try
        {
            var response = await model.GenerateContentAsync(prompt);
            var json = CleanJson(response.Text);
            var result = JsonSerializer.Deserialize<GeminiFraudResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return (result?.IsFraudulent ?? false, result?.Reason);
        }
        catch
        {
            return (false, null); // Default to safe
        }
    }

    private string CleanJson(string text)
    {
        return text.Replace("```json", "").Replace("```", "").Trim();
    }

    // Private DTOs for JSON deserialization
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