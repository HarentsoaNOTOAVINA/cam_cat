using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using CamtParser.model;
using CamtParser.Service;

namespace CamtParser;

internal abstract class Program
{
    private static readonly LlmService LlmService;
    private static readonly XmlLoadService XmlService;

    static Program()
    {
        ConfigurationReader.LoadConfiguration();
        LlmService = new LlmService();
        XmlService = new XmlLoadService();
    }
    

    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("CAMT parsing...\n");

        try
        {
            var filePath = ConfigurationReader.GetValue("FILE_PATH");
            if (string.IsNullOrEmpty(filePath))
            {
                throw new FileNotFoundException("FILE_PATH not found in configuration.");
            }
            filePath = filePath.Replace("\\\\", "\\");
            Console.WriteLine($"Using file path: {filePath}");
            
            // Extraction des transactions du fichier CAMT via le service dédié
            var extractor = new TransactionExtractor(XmlService);
            var transactions = extractor.ExtractTransactions(filePath);
                
            // Harmonisation des libellés avec un LLM
            await LlmService.HarmonizeLabelsWithLlm(transactions);
                
            // Affichage des résultats
            DisplayResults(transactions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occured : {ex.Message}");
        }
    }

        private static void DisplayResults(List<Transaction> transactions)
        {
            Console.WriteLine("\n--- Résults ---");
            Console.WriteLine("Date\t\tMontant\t\tLibellé Original\t\tLibellé Harmonisé");
            Console.WriteLine(new string('-', 100));
            
            foreach (var transaction in transactions)
            {
                Console.WriteLine($"{transaction.Date:yyyy-MM-dd}\t{transaction.Amount,10:N2}€\t{transaction.OriginalLabel}\t\t{transaction.HarmonizedLabel}");
            }
        }
}
