using XmlSchemaClassGenerator;

namespace Mechanic.BuildTasks;

public class PyroSchemaCodeGenTask(
    string outputFolder
    )
{
    public async Task GenerateSchemaCode()
    {
        Console.WriteLine("Generating Pyro Schema Code...");
        Console.WriteLine(outputFolder);
        var generator = new Generator
        {
            OutputFolder = outputFolder,
            GenerateNullables = true,
            Log = s => Console.Out.WriteLine(s),
            NamespaceProvider = new NamespaceProvider
            {
                GenerateNamespace = _ => "Pyro.Models.Generated",
            },
            DataAnnotationMode = DataAnnotationMode.None
            
        };
        
        generator.Generate([await DownloadPyroSchema()]);
    }

    private static async Task<TextReader> DownloadPyroSchema()
    {
        try
        {
            Console.WriteLine("Downloading Pyro Schema XSD...");
            const string PyroSchemaUrl = "https://raw.githubusercontent.com/fireundubh/pyro/refs/heads/master/pyro/PapyrusProject.xsd";
    
            using var httpClient = new HttpClient();
    
            var response = await httpClient.GetAsync(PyroSchemaUrl);
            Console.Out.WriteLine(response.StatusCode);
            response.EnsureSuccessStatusCode();

            var responseText = await httpClient.GetStreamAsync(PyroSchemaUrl);
            return new StreamReader(responseText);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[CRITICAL ERROR] Unknown machine spirit disruption: {ex.Message}");
            Console.Error.WriteLine($"[STACK TRACE] Error sanctification data: {ex.StackTrace}");
            throw;
        }
    }
}