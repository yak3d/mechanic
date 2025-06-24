using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using Task = Microsoft.Build.Utilities.Task;

namespace Mechanic.BuildTasks;

public class JsonSchemaCodeGenTask : Task
{
    [Required]
    public string SchemaFile { get; set; }
    
    [Required]
    public string OutputFile { get; set; }

    public string Namespace { get; set; } = "Generated";
    public string ClassStyle { get; set; } = "Poco";
    public bool GenerateDataAnnotations { get; set; } = true;
    public bool GenerateJsonMethods { get; set; } = true;

    public override bool Execute()
    {
        try
        {
            var schema = JsonSchema.FromFileAsync(SchemaFile).Result;

            var settings = new CSharpGeneratorSettings
            {
                Namespace = Namespace,
                ClassStyle = ParseClassStyle(ClassStyle),
                GenerateDataAnnotations = GenerateDataAnnotations,
                GenerateJsonMethods = GenerateJsonMethods,
                GenerateDefaultValues = true
            };

            var generator = new CSharpGenerator(schema, settings);
            var code = generator.GenerateFile();

            var outputDir = Path.GetDirectoryName(OutputFile);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            File.WriteAllText(OutputFile, code);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private CSharpClassStyle ParseClassStyle(string classStyle)
    {
        return classStyle?.ToLowerInvariant() switch
        {
            "poco" => CSharpClassStyle.Poco,
            "inpc" => CSharpClassStyle.Inpc,
            "record" => CSharpClassStyle.Record,
            _ => CSharpClassStyle.Poco
        };
    }
}