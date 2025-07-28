using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CamtParser.Service;

public class LlmService
{
    private readonly string? _apiKey;
    private readonly string? _apiUrl;

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
                            text =
                                @$"Tu es un assistant bancaire. Ton rôle est de reformuler des libellés bancaires en français clair pour les rendre compréhensibles pour l’utilisateur.
        
                                Libellé d'entrée : {{originalLabel}}
                                Montant de la transaction : {{amount}}€

                                Règles :

                                Rendre le libellé plus lisible et clair

                                Conserver les informations essentielles de la transaction

                                Standardiser les termes courants (par exemple : 'VIR SEPA' devient 'Virement', 'PRLV SEPA' devient 'Prélèvement', 'CARTE' devient 'Paiement carte', 'CB' devient 'Carte bancaire')

                                Mettre une majuscule à la première lettre, le reste en minuscules

                                Supprimer les codes techniques inutiles

                                Transformer les dates au format français (ex : '12/04' devient '12 avril')

                                Conserver les noms de commerçants ou d'entreprises s’ils sont présents

                                Affiche uniquement le libellé harmonisé, sans aucune explication.
                                Par exemple, si le libellé est: 
                                - « PRLV SEPA MUTUELSANTE 552142259 », la sortie doit être : « Prélèvement mutuelle santé - mensuel ».
                                -  « CB CARREFOUR CITY PARIS 12/04 », la sortie doit être : « Courses supermarché Carrefour City - 12 avril ».
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
            
            return harmonized;
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