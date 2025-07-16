namespace Mechanic.Core.Services;

using System.Xml;
using Contracts;

public class XmlSerializer : IXmlSerializer
{
    public Task<string> SerializeAsync<T>(T obj)
    {
        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));

        var settings = new XmlWriterSettings()
        {
            Indent = true, IndentChars = "    ", NewLineOnAttributes = true, OmitXmlDeclaration = false
        };

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);

        serializer.Serialize(xmlWriter, obj);

        return Task.FromResult(stringWriter.ToString());

    }
}
