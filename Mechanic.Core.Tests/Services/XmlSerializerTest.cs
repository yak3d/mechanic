using JetBrains.Annotations;
using Mechanic.Core.Services;
using Shouldly;

namespace Mechanic.Core.Tests.Services;

[TestSubject(typeof(XmlSerializer))]
public class XmlSerializerTest
{


    [Fact]
    public async Task SerializeAsync_WithComplexNestedObject_ShouldSerializeAllProperties()
    {
        var nestedObject = new ComplexTestData
        {
            Header = new TestData { Id = 1, Name = "HEADER_DATA" },
            Items = new List<TestData>
            {
                new() { Id = 2, Name = "ITEM_ONE" },
                new() { Id = 3, Name = "ITEM_TWO" }
            }
        };
        var service = new XmlSerializer();

        var result = await service.SerializeAsync(nestedObject);

        result.ShouldContain("<ComplexTestData");
        result.ShouldContain("<Header>");
        result.ShouldContain("<Items>");
        result.ShouldContain("HEADER_DATA");
        result.ShouldContain("ITEM_ONE");
        result.ShouldContain("ITEM_TWO");
    }
}


public class TestData
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsActive { get; set; }
}

public class ComplexTestData
{
    public TestData? Header { get; set; }
    public List<TestData>? Items { get; set; }
}

[System.Xml.Serialization.XmlRoot("DataWithAttributes")]
public class TestDataWithAttributes
{
    [System.Xml.Serialization.XmlAttribute("identifier")]
    public int Id { get; set; }
    
    [System.Xml.Serialization.XmlElement("designation")]
    public string? Name { get; set; }
}