using Azure;
using Azure.AI.TextAnalytics;

namespace PostIn.Services;

public class SentimentService
{
    private readonly TextAnalyticsClient? _client;

    public SentimentService(IConfiguration configuration)
    {
        var endpoint = configuration["AzureLanguage:Endpoint"];
        var apiKey = configuration["AzureLanguage:ApiKey"];

        if (!string.IsNullOrWhiteSpace(endpoint) && !string.IsNullOrWhiteSpace(apiKey))
        {
            _client = new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        }
    }

    public async Task<(string Sentiment, double Pos, double Neu, double Neg)> AnalyzeSentimentAsync(string text)
    {
        if (_client == null || string.IsNullOrWhiteSpace(text))
        {
            return ("Neutral", 0.0, 1.0, 0.0);
        }

        try
        {
            var response = await _client.AnalyzeSentimentAsync(text);
            var doc = response.Value;

            return (
                doc.Sentiment.ToString(),
                Math.Round(doc.ConfidenceScores.Positive, 2),
                Math.Round(doc.ConfidenceScores.Neutral, 2),
                Math.Round(doc.ConfidenceScores.Negative, 2)
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Azure AI Error] {ex.Message}");
            return ("Unknown", 0.0, 0.0, 0.0);
        }
    }
}