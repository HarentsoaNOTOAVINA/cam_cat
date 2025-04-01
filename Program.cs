using System.Globalization;
using System.Text;
using System.Xml.Linq;
using CamtParser.Service;

namespace CamtParser;

internal class Program
{
    private static readonly LlmService _service;

    static Program()
    {
        ConfigurationReader.LoadConfiguration();
        _service = new LlmService();
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
            // Extraction des transactions du fichier CAMT
            var transactions = ExtractTransactionsFromCamt(filePath);
                
            // Harmonisation des libellés avec un LLM
            await _service.HarmonizeLabelsWithLlm(transactions);
                
            // Affichage des résultats
            DisplayResults(transactions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occured : {ex.Message}");
        }
    }
    
    private static List<Transaction> ExtractTransactionsFromCamt(string filePath)
        {
            var transactions = new List<Transaction>();
            
            var fileName = Path.GetFileName(filePath);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"CAMT file {fileName} not found.");
            }
            
            try
            {
                // Chargement du fichier XML
                XDocument doc = XDocument.Load(filePath);
                
                // Définition des namespaces CAMT
                XNamespace ns = doc.Root.GetDefaultNamespace();
                
                // Recherche des transactions dans le document
                var stmts = doc.Descendants(ns + "Stmt");
                
                foreach (var stmt in stmts)
                {
                    var entries = stmt.Elements(ns + "Ntry");
                    
                    foreach (var entry in entries)
                    {
                        // Extraction des détails de la transaction
                        var amountElement = entry.Element(ns + "Amt");
                        var dateElement = entry.Element(ns + "BookgDt")?.Element(ns + "Dt");
                        var creditDebitIndicator = entry.Element(ns + "CdtDbtInd")?.Value;
                        
                        // Extraction des détails spécifiques dans NtryDtls
                        var txDetails = entry.Elements(ns + "NtryDtls")
                            .Elements(ns + "TxDtls").FirstOrDefault();
                        
                        string? reference = txDetails?.Element(ns + "Refs")?.Element(ns + "InstrId")?.Value;
                        
                        // Récupération du libellé (plusieurs possibilités selon la structure CAMT)
                        string? label = txDetails?.Element(ns + "RmtInf")?.Element(ns + "Ustrd")?.Value;
                        
                        if (string.IsNullOrEmpty(label))
                        {
                            label = txDetails?.Element(ns + "AddtlTxInf")?.Value;
                        }
                        
                        if (string.IsNullOrEmpty(label))
                        {
                            // Recherche dans d'autres endroits possibles du document
                            label = entry.Element(ns + "AddtlNtryInf")?.Value;
                        }

                        // Si on a bien un montant
                        if (amountElement != null)
                        {
                            decimal amount = decimal.Parse(amountElement.Value, CultureInfo.InvariantCulture);
                            
                            // Ajustement du signe selon l'indicateur crédit/débit
                            if (creditDebitIndicator == "DBIT")
                            {
                                amount = -amount;
                            }
                            
                            var transaction = new Transaction
                            {
                                Date = dateElement != null ? DateTime.Parse(dateElement.Value) : DateTime.MinValue,
                                Amount = amount,
                                OriginalLabel = label ?? "without label",
                                HarmonizedLabel = "", 
                                Reference = reference ?? ""
                            };
                            
                            transactions.Add(transaction);
                        }
                    }
                }
                
                Console.WriteLine($"{transactions.Count} transactions from CAMT file.");
                return transactions;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occured on parsing CAMT file : {ex.Message}", ex);
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
