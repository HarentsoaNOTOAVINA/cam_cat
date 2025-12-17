using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CamtParser.model;

namespace CamtParser.Service;

public class LlmService
{
    private readonly string? _apiKey;
    private readonly string? _apiUrl;
    private const int BatchSize = 10;

    public LlmService()
    {
        _apiKey = ConfigurationReader.GetValue("LLM_API_KEY");
        // Ensure we are getting the correct base URL key as previously identified
        _apiUrl = ConfigurationReader.GetValue("LLM_API_BASE_URL");
    }

    private class TransactionInput
    {
        public int Id { get; set; }
        public string Label { get; set; } = "";
        public decimal Amount { get; set; }
    }

    private class HarmonizedOutput
    {
        public int Id { get; set; }
        public string HarmonizedLabel { get; set; } = "";
    }

    private async Task<List<HarmonizedOutput>> GetHarmonizedBatch(HttpClient client, List<TransactionInput> batch)
    {
        string jsonInput = JsonSerializer.Serialize(batch);
        
        string prompt = @$"
Tu es un assistant expert bancaire. Ta tâche est de reformuler une liste de libellés bancaires pour les rendre lisibles.

Voici la liste des transactions à traiter (format JSON) :
{jsonInput}

Règles de transformation :
1. Rendre le libellé clair et concis.
2. Standardiser les termes (ex: 'VIR' -> 'Virement', 'PRLV' -> 'Prélèvement').
3. Mettre une majuscule au début, le reste en minuscules (sauf noms propres).
4. Supprimer les références techniques inutiles.
5. Formater les dates (ex: '12/04' -> '12 avril').
6. Conserver les noms des tiers.

IMPORTANT : Tu dois répondre UNIQUEMENT par un tableau JSON valide contenant les libellés harmonisés, avec la structure suivante :
[{{""Id"": 1, ""HarmonizedLabel"": ""Libellé reformulé""}}, ...]

Ne mets PAS de markdown, PAS de code block, juste le JSON brut. Assure-toi de renvoyer une entrée pour CHAQUE élément de la liste d'entrée, avec le même ID.
";

        var requestData = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var fullUrl = $"{_apiUrl}?key={_apiKey}";
        var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

        try 
        {
            var response = await client.PostAsync(fullUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {response.StatusCode} - {errorBody}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseContent);
            
            // Extract the text from Gemini response structure
            var candidates = doc.RootElement.GetProperty("candidates");
            if (candidates.GetArrayLength() == 0) return new List<HarmonizedOutput>();

            var text = candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrEmpty(text)) return new List<HarmonizedOutput>();

            // Clean up potentially wrapped markdown (```json ... ```)
            text = text.Replace("```json", "").Replace("```", "").Trim();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<HarmonizedOutput>>(text, options) ?? new List<HarmonizedOutput>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nBatch API Warning: {ex.Message}");
            return new List<HarmonizedOutput>(); // Fail gracefully for this batch
        }
    }

    public async Task HarmonizeLabelsWithLlm(List<Transaction> transactions)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        Console.WriteLine($"Harmonizing {transactions.Count} transactions in batches of {BatchSize}...");

        for (int i = 0; i < transactions.Count; i += BatchSize)
        {
            var batch = transactions.Skip(i).Take(BatchSize).ToList();
            var inputs = batch.Select((t, index) => new TransactionInput 
            { 
                Id = index, 
                Label = t.OriginalLabel,
                Amount = t.Amount
            }).ToList();

            Console.Write($"\rProcessing batch {(i / BatchSize) + 1}/{(int)Math.Ceiling(transactions.Count / (double)BatchSize)}...");

            try
            {
                var outputs = await GetHarmonizedBatch(client, inputs);

                foreach (var output in outputs)
                {
                    if (output.Id >= 0 && output.Id < batch.Count)
                    {
                        batch[output.Id].HarmonizedLabel = output.HarmonizedLabel;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError in loop: {ex.Message}");
            }
            
            // Fill in any missing or failed ones with original label
            foreach (var t in batch)
            {
                if (string.IsNullOrEmpty(t.HarmonizedLabel))
                {
                    t.HarmonizedLabel = t.OriginalLabel;
                }
            }

            // Simple rate limiting avoidance
            if (i + BatchSize < transactions.Count) 
                await Task.Delay(1000); 
        }

        Console.WriteLine("\nHarmonization done.");
    }
}