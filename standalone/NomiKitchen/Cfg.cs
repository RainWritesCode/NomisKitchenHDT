using System.Text;

namespace NomiKitchen;

// Reads and writes BepInEx .cfg values in place, keeping the other lines and CRLF endings.
static class Cfg
{
    public static string PathFor(string hs, string file) => Path.Combine(hs, "BepInEx", "config", file);

    public static string? Get(string path, string section, string key)
    {
        string current = "";
        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (line.StartsWith('[') && line.EndsWith(']')) { current = line[1..^1]; continue; }
            int eq = line.IndexOf('=');
            if (current == section && eq > 0 && line[..eq].Trim() == key) return line[(eq + 1)..].Trim();
        }
        return null;
    }

    public static void Set(string path, string section, string key, string value)
    {
        var lines = File.ReadAllLines(path).ToList();
        string current = "";
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith('[') && line.EndsWith(']')) { current = line[1..^1]; continue; }
            int eq = line.IndexOf('=');
            if (current == section && eq > 0 && line[..eq].Trim() == key)
            {
                lines[i] = $"{key} = {value}";
                File.WriteAllText(path, string.Join("\r\n", lines) + "\r\n", new UTF8Encoding(false));
                return;
            }
        }
        throw new InvalidOperationException($"Setting [{section}] {key} not found in {Path.GetFileName(path)}.");
    }
}
