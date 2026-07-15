using System.Text.Json;
using DatParser.Parsers;

if (args.Length < 2)
{
    Console.Error.WriteLine("Usage: DatParser <type> <input.dat> [output.json]");
    Console.Error.WriteLine("Types: avatar, navigator, title, item, skin, se");
    Console.Error.WriteLine();
    Console.Error.WriteLine("  DatParser avatar avatar.dat Avatars.json");
    Console.Error.WriteLine("  DatParser title title.dat   (outputs to stdout)");
    return 1;
}

var type = args[0].ToLowerInvariant();
var inputPath = args[1];
var outputPath = args.Length > 2 ? args[2] : null;

if (!File.Exists(inputPath))
{
    Console.Error.WriteLine($"File not found: {inputPath}");
    return 1;
}

var options = new JsonSerializerOptions { WriteIndented = true };

object result = type switch
{
    "avatar" => AvatarParser.Parse(inputPath),
    "navigator" => NavigatorParser.Parse(inputPath),
    "title" => TitleParser.Parse(inputPath),
    "item" => ItemParser.Parse(inputPath),
    "skin" => SkinParser.Parse(inputPath),
    "se" or "soundeffect" => SoundEffectParser.Parse(inputPath),
    _ => throw new ArgumentException($"Unknown type: {type}. Use: avatar, navigator, title, item, skin, se")
};

var json = JsonSerializer.Serialize(result, options);

if (outputPath is not null)
{
    File.WriteAllText(outputPath, json);
    Console.WriteLine($"Wrote {outputPath}");
}
else
{
    Console.WriteLine(json);
}

return 0;
