namespace CamtParser.Service;

public static class ConfigurationReader
{
    private static readonly Dictionary<string, string> EnvVariables = new Dictionary<string, string>();

    public static void LoadConfiguration(string filePath = ".env")
    {
        var fullPath = Path.GetFullPath(filePath);
        Console.WriteLine($"Looking for .env file at: {fullPath}");

        if (!File.Exists(fullPath))
        {
            Console.WriteLine(".env file not found!");
            return;
        }

        foreach (var line in File.ReadAllLines(fullPath))
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#")) continue;

            var parts = trimmedLine.Split('=', 2);
            if (parts.Length != 2) continue;

            EnvVariables[parts[0].Trim()] = parts[1].Trim();
            Console.WriteLine($"Loaded config: {parts[0]} = {parts[1]}");
        }
    }

    public static string? GetValue(string key)
    {
        if (EnvVariables.TryGetValue(key, out var value))
        {
            return value;
        }
        Console.WriteLine($"Configuration key not found: {key}");
        return null;
    }
}