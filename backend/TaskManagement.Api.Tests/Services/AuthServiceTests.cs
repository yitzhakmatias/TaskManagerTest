using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using TaskManagement.Api.Core.Application;
using TaskManagement.Api.Core.Ports;

namespace TaskManagement.Api.Tests.Services;

[TestFixture]
public class AuthServiceTests
{
    private static readonly DemoUserOptions DemoUser = new() { Username = "admin", Password = "admin123!" };

    private Mock<IJwtTokenGenerator> _tokenGeneratorMock = null!;
    private AuthService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _tokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _sut = new AuthService(Options.Create(DemoUser), _tokenGeneratorMock.Object);
    }

    [Test]
    public void Authenticate_ReturnsToken_ForValidCredentials()
    {
        var expected = new AuthToken("signed.jwt.token", DateTime.UtcNow.AddHours(1));
        _tokenGeneratorMock.Setup(g => g.GenerateToken("admin")).Returns(expected);

        var result = _sut.Authenticate("admin", "admin123!");

        Assert.That(result, Is.EqualTo(expected));
        _tokenGeneratorMock.Verify(g => g.GenerateToken("admin"), Times.Once);
    }

    [TestCase("admin", "wrong-password")]
    [TestCase("someone-else", "admin123!")]
    [TestCase("", "")]
    public void Authenticate_ReturnsNull_AndNeverGeneratesToken_ForInvalidCredentials(string username, string password)
    {
        var result = _sut.Authenticate(username, password);

        Assert.That(result, Is.Null);
        _tokenGeneratorMock.Verify(g => g.GenerateToken(It.IsAny<string>()), Times.Never);
    }
}
