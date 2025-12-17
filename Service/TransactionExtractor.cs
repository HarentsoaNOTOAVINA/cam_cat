using System.Globalization;
using System.Xml.Linq;
using CamtParser.model;

namespace CamtParser.Service;

public class TransactionExtractor
{
    private readonly XmlLoadService _xmlService;

    public TransactionExtractor(XmlLoadService xmlService)
    {
        _xmlService = xmlService;
    }

    public List<Transaction> ExtractTransactions(string filePath)
    {
        var transactions = new List<Transaction>();
        
        var fileName = Path.GetFileName(filePath);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"CAMT file {fileName} not found.");
        }
        
        try
        {
            // Use the safer Xml loading method
            XDocument? doc = _xmlService.SafeLoadXml(filePath);
            if (doc == null) return transactions; // Or throw? Returning empty list for now as per logic
            
            // Définition des namespaces CAMT
            XNamespace ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;
            if (ns == XNamespace.None) 
            {
                 // Handle case where root might be null or no namespace
                 // For now, if doc.Root is null, we already returned or crashed in GetDefaultNamespace if not careful.
                 // safeLoadXml returns XDocument or null.
                 if (doc.Root == null) return transactions;
                 ns = doc.Root.GetDefaultNamespace();
            }
            
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
                    
                    // Corrected reference extraction based on CAMT guide (AcctSvcrRef)
                    string? reference = txDetails?.Element(ns + "Refs")?.Element(ns + "AcctSvcrRef")?.Value;
                    
                    // Fallback to InstrId if AcctSvcrRef is missing (defensive coding)
                    if (string.IsNullOrEmpty(reference))
                    {
                        reference = txDetails?.Element(ns + "Refs")?.Element(ns + "InstrId")?.Value;
                    }
                    
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
}
