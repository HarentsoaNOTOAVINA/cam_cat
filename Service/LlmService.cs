using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CamtParser.Service;

public class LlmService
{
    private readonly string _apiKey;
    private readonly string _apiUrl;

    public LlmService()
    {
        _apiKey = ConfigurationReader.GetValue("LLM_API_KEY");
        
        // Fix: Use LLM_API_BASE_URL instead of LLM_API_URL
        _apiUrl = ConfigurationReader.GetValue("LLM_API_BASE_URL");
    }

    private async Task<string> GetHarmonizedLabelFromLlm(HttpClient client, string originalLabel, decimal amount)
    {
        var requestData = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = @$"Act as a banking expert. Your task is to standardize bank transaction labels.
        
                                Input label: {originalLabel}
                                Transaction amount: {amount}€
        
                                Rules:
                                1. Make the label more readable and clear
                                2. Keep essential transaction information
                                3. Standardize common terms (e.g., 'VIR SEPA' to 'Virement', 'PRLV SEPA' to 'Prélèvement', 'CARTE' to 'Paiement carte', 'CB' to 'Carte bancaire')
                                4. Capitalize first letter, rest in lowercase
                                5. Remove unnecessary technical codes
                                6. Change the date format to french (e.g., '12/04' to '12 avril')
                                7. Keep merchant/company names if present
        
                                Output only the harmonized label without any explanation.
                                For example, if i have 'PRLV SEPA MUTUELSANTE 552142259', the output willi be 'Prélèvement mutuelle santé - mensuel'.
                                "
                        }
                    }
                }
            }
        };

        // Fix: Construct the full URL with API key as a query parameter
        var fullUrl = $"{_apiUrl}?key={_apiKey}";
        
        var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
        var response = await client.PostAsync(fullUrl, content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var harmonized = responseObject
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? originalLabel;

            return harmonized.Replace("PRLV SEPA", "Prélèvement ")
                .Replace("VIR SEPA", "Virement ")
                .Replace("CARTE ", "Paiement carte ")
                .Replace("CB ", "Carte bancaire ");
        }

        throw new Exception($"LLM API error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
    }

    public async Task HarmonizeLabelsWithLlm(List<Transaction> transactions)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            Console.WriteLine("Harmonization...");

            for (int i = 0; i < transactions.Count; i++)
            {
                var transaction = transactions[i];

                try
                {
                    string harmonizedLabel =
                        await GetHarmonizedLabelFromLlm(client, transaction.OriginalLabel, transaction.Amount);
                    transaction.HarmonizedLabel = harmonizedLabel;

                    if (i % 5 == 0 || i == transactions.Count - 1)
                    {
                        Console.Write($"\rProgression: {i + 1}/{transactions.Count}");
                    }
                }
                catch (Exception ex)
                {
                    transaction.HarmonizedLabel = transaction.OriginalLabel;
                    Console.WriteLine($"\nHarmonization error for the transaction {i}: {ex.Message}");
                }
            }

            Console.WriteLine("\nHarmonization done.");
        }
    }
}