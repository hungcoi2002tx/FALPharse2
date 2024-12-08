using AuthenExamCompareFaceExam.ExternalService;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AuthenExam.UnitTests
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private string _authUrl = "https://dev.demorecognition.click/api/Auth/login";
        private string _username = "khanm";
        private string _password = "123456";

        [SetUp]
        public void Setup()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        }

        [Test]
        public async Task GetTokenAsync_ShouldReturnToken_WhenLoginSucceeds()
        {
            var authService = new AuthService(_authUrl, _username, _password);

            // Act
            var token = await authService.GetTokenAsync();

            // Assert
            Assert.IsNotNull(token, "Token should not be null.");
            Assert.IsTrue(token.Contains('.'), "Token should be in JWT format (contain at least two periods).");
        }

        [Test]
        public void GetTokenAsync_ShouldThrowException_WhenLoginFails()
        {
            // Arrange
            var authService = new AuthService(_authUrl, _username, "wrongpassword");

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await authService.GetTokenAsync());
        }
    }
}
