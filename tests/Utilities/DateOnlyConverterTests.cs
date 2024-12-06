using System.Text.Json;
using Xunit;

namespace Tests.Utilities;

public class DateOnlyConverterTests
{
    [Fact]
    public void DateOnlyConverter_CanConvertDateOnly()
    {
        // Arrange
        var options = new JsonSerializerOptions
        {
            Converters = { new DateOnlyConverter() }
        };
        var testDate = new DateOnly(2024, 3, 14);
        var expectedJson = "\"2024-03-14\"";

        // Act
        var jsonString = JsonSerializer.Serialize(testDate, options);
        var deserializedDate = JsonSerializer.Deserialize<DateOnly>(jsonString, options);

        // Assert
        Assert.Equal(expectedJson, jsonString);
        Assert.Equal(testDate, deserializedDate);
    }
} 