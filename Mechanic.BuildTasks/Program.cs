using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;

if (args.Length < 2)
{
    Console.WriteLine("Usage: Mechanic.BuildTasks <schema-file> <output-file> [namespace]");
    return 1;
}

var schemaFile = args[0];
var outputFile = args[1];
var namespaceName = args.Length > 2 ? args[2] : "Generated";

try
{
    Console.WriteLine($"🚀 Generating C# classes from {schemaFile}");
    
    if (!File.Exists(schemaFile))
    {
        Console.Error.WriteLine($"❌ Schema file not found: {schemaFile}");
        return 1;
    }

    var schema = await JsonSchema.FromFileAsync(schemaFile);
    Console.WriteLine("✅ Schema loaded successfully");

    var settings = new CSharpGeneratorSettings
    {
        Namespace = namespaceName,
        ClassStyle = CSharpClassStyle.Poco,
        GenerateDataAnnotations = true,
        GenerateJsonMethods = false,
        GenerateDefaultValues = true
    };

    var generator = new CSharpGenerator(schema, settings);
    var code = generator.GenerateFile();

    Console.WriteLine($"✅ Generated {code.Length} characters of code");

    // Ensure output directory exists
    var outputDir = Path.GetDirectoryName(outputFile);
    if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
    {
        Directory.CreateDirectory(outputDir);
        Console.WriteLine($"✅ Created directory: {outputDir}");
    }

    await File.WriteAllTextAsync(outputFile, code);
    Console.WriteLine($"✅ Successfully generated classes to: {outputFile}");

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"❌ Error: {ex.Message}");
    Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
    return 1;
}