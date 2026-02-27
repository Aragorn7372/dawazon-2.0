using dawazon2._0.Session;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Text;

namespace dawazonTest.dawazon2._0.Session;

[TestFixture]
public class SessionExtensionsTests
{
    private Mock<ISession> _sessionMock;
    private Dictionary<string, byte[]> _sessionStorage;

    [SetUp]
    public void SetUp()
    {
        _sessionStorage = new Dictionary<string, byte[]>();
        _sessionMock = new Mock<ISession>();

        _sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => _sessionStorage[key] = value);

        _sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) =>
            {
                if (_sessionStorage.TryGetValue(key, out value))
                {
                    return true;
                }
                value = null;
                return false;
            });
    }

    [Test]
    public void SetJson_ShouldStoreObjectAsJsonString()
    {
        // Arrange
        var key = "testKey";
        var obj = new { Id = 1, Name = "Test" };

        // Act
        _sessionMock.Object.SetJson(key, obj);

        // Assert
        Assert.That(_sessionStorage.ContainsKey(key), Is.True);
        var jsonString = Encoding.UTF8.GetString(_sessionStorage[key]);
        Assert.That(jsonString, Contains.Substring("\"Id\":1"));
        Assert.That(jsonString, Contains.Substring("\"Name\":\"Test\""));
    }

    [Test]
    public void GetJson_WhenKeyExists_ShouldReturnDeserializedObject()
    {
        // Arrange
        var key = "testKey";
        var jsonString = "{\"Id\":1,\"Name\":\"Test\"}";
        _sessionStorage[key] = Encoding.UTF8.GetBytes(jsonString);

        // Act
        var result = _sessionMock.Object.GetJson<TestObject>(key);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Name, Is.EqualTo("Test"));
    }

    [Test]
    public void GetJson_WhenKeyDoesNotExist_ShouldReturnDefault()
    {
        // Arrange
        var key = "missingKey";

        // Act
        var result = _sessionMock.Object.GetJson<TestObject>(key);

        // Assert
        Assert.That(result, Is.Null);
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}