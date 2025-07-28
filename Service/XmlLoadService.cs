using System.Xml.Linq;

namespace CamtParser.Service;

public class XmlLoadService
{
    private void DebugXmlFile(string filePath)
    {
        Console.WriteLine($"=== XML File Debug Info ===");
        Console.WriteLine($"File Path: {filePath}");
        Console.WriteLine($"File Exists: {File.Exists(filePath)}");

        if (File.Exists(filePath))
        {
            var fileInfo = new FileInfo(filePath);
            Console.WriteLine($"File Size: {fileInfo.Length} bytes");
            Console.WriteLine($"File Extension: {fileInfo.Extension}");

            // Read first few lines to check content
            try
            {
                var lines = File.ReadAllLines(filePath);
                Console.WriteLine($"Total Lines: {lines.Length}");
                Console.WriteLine("\n=== First 10 lines of file ===");

                for (int i = 0; i < Math.Min(10, lines.Length); i++)
                {
                    Console.WriteLine($"{i + 1}: {lines[i]}");
                }

                // Try to detect encoding issues
                var rawBytes = File.ReadAllBytes(filePath);
                Console.WriteLine($"\n=== First 20 bytes (hex) ===");
                Console.WriteLine(BitConverter.ToString(rawBytes.Take(20).ToArray()));

                // Check for BOM
                if (rawBytes.Length >= 3 && rawBytes[0] == 0xEF && rawBytes[1] == 0xBB && rawBytes[2] == 0xBF)
                {
                    Console.WriteLine("UTF-8 BOM detected");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
        }
    }

// Modified XML loading with better error handling
    public XDocument? SafeLoadXml(string filePath)
    {
        try
        {
            // First, debug the file
            DebugXmlFile(filePath);

            Console.WriteLine("\n=== Attempting XML Load ===");

            // Try different loading approaches
            XDocument? doc = null;
            
            //  Load raw text and parse
            try
            {
                var xmlContent = File.ReadAllText(filePath);
                Console.WriteLine($"Raw content length: {xmlContent.Length}");

                // Check if content is empty or whitespace
                if (string.IsNullOrWhiteSpace(xmlContent))
                {
                    Console.WriteLine("✗ File content is empty or whitespace");
                    return null;
                }

                doc = XDocument.Parse(xmlContent);
                Console.WriteLine("✓ XDocument.Parse() from string succeeded");
                return doc;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ XDocument.Parse() from string failed: {ex.Message}");
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error in SafeLoadXml: {ex.Message}");
            return null;
        }
    }
}