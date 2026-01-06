using System.Text.Json;
using AIClassification.Application.Services;
using AIClassification.Domain.ValueObjects;
using Google.Generative.AI;
using Microsoft.Extensions.Configuration;

namespace AIClassification.Infrastructure.AI;

public class GeminiService : IGeminiService
{
    private readonly string _apiKey;
    private const string ModelName = "gemini-1.5-flash"; 

    public GeminiService(IConfiguration config)
    {
        // This reads the key you set in User Secrets or Environment Variables
        _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey not configured");
    }

    public async Task<(ExtractedInvoiceData Data, double Confidence)> ExtractDataAsync(string imageUrl, CancellationToken ct = default)
    {
        // Initialize Gemini
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
            // Send request to Google
            var response = await model.GenerateContentAsync(prompt, imageUrl);
            
            // Clean up response (sometimes AI adds ```json at the start)
            var json = response.Text.Replace("```json", "").Replace("```", "").Trim();
            
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<GeminiExtractionResult>(json, options);

            if (result == null) throw new Exception("Failed to deserialize AI response");

            // Map to Domain Object
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
            var json = response.Text.Replace("```json", "").Replace("```", "").Trim();
            var result = JsonSerializer.Deserialize<GeminiFraudResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return (result?.IsFraudulent ?? false, result?.Reason);
        }
        catch
        {
            return (false, null);
        }
    }

    // Helper classes for JSON parsing
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