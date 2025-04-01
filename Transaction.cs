namespace CamtParser;

public class Transaction
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string OriginalLabel { get; set; }
    public string HarmonizedLabel { get; set; }
    public string Reference { get; set; }
}