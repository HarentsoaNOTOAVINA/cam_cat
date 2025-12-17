using CamtParser.Service;
using CamtParser.model;
using Xunit;
using System.IO;
using System.Linq;

namespace CamtParser.Tests;

public class XmlParserTests
{
    [Fact]
    public void Test_ExtractTransactions_ShouldParseCorrectly()
    {
        // Arrange
        string xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Document xmlns=""urn:iso:std:iso:20022:tech:xsd:camt.053.001.02"">
  <BkToCstmrStmt>
    <Stmt>
      <Ntry>
        <Amt Ccy=""EUR"">100.00</Amt>
        <CdtDbtInd>CRDT</CdtDbtInd>
        <BookgDt>
          <Dt>2023-12-01</Dt>
        </BookgDt>
        <NtryDtls>
          <TxDtls>
            <Refs>
              <InstrId>WRONG_REF</InstrId>
              <AcctSvcrRef>CORRECT_REF_123</AcctSvcrRef>
            </Refs>
            <RmtInf>
              <Ustrd>Payment for services</Ustrd>
            </RmtInf>
          </TxDtls>
        </NtryDtls>
      </Ntry>
    </Stmt>
  </BkToCstmrStmt>
</Document>";

        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, xmlContent);

        var xmlService = new XmlLoadService();
        var extractor = new TransactionExtractor(xmlService);

        try
        {
            // Act
            var transactions = extractor.ExtractTransactions(tempFile);

            // Assert
            Assert.Single(transactions);
            var t = transactions.First();
            Assert.Equal(100.00m, t.Amount);
            Assert.Equal("Payment for services", t.OriginalLabel);
            Assert.Equal("CORRECT_REF_123", t.Reference); // This validates our fix
            Assert.Equal(new DateTime(2023, 12, 01), t.Date);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
    
    [Fact]
    public void Test_ExtractTransactions_ShouldHandleDebit()
    {
         // Arrange
        string xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Document xmlns=""urn:iso:std:iso:20022:tech:xsd:camt.053.001.02"">
  <BkToCstmrStmt>
    <Stmt>
      <Ntry>
        <Amt Ccy=""EUR"">50.50</Amt>
        <CdtDbtInd>DBIT</CdtDbtInd>
        <BookgDt><Dt>2023-01-01</Dt></BookgDt>
        <NtryDtls><TxDtls><RmtInf><Ustrd>Debit Test</Ustrd></RmtInf></TxDtls></NtryDtls>
      </Ntry>
    </Stmt>
  </BkToCstmrStmt>
</Document>";
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, xmlContent);
        
        var xmlService = new XmlLoadService();
        var extractor = new TransactionExtractor(xmlService);

        try
        {
            // Act
            var transactions = extractor.ExtractTransactions(tempFile);

            // Assert
            Assert.Single(transactions);
            Assert.Equal(-50.50m, transactions.First().Amount);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
