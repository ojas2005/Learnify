using Moq;
using NUnit.Framework;
using Learnify.Identity.API.Application;
using Learnify.Identity.API.Endpoints;
using Learnify.Core.Core;
using Learnify.Core.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Learnify.Identity.API.Contracts;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Learnify.UnitTests
{
    public static class ControllerTestHelper
    {
        public static void SetupUser(ControllerBase controller, int userId = 1, string email = "test@test.com", string role = "Learner")
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Email, email),
                new(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }
    }

    [TestFixture]
    public class IdentityTests
    {
        private Mock<IIdentityBroker> _brokerMock;
        private AccountController _controller;

        [SetUp]
        public void Setup()
        {
            _brokerMock = new Mock<IIdentityBroker>();
            _controller = new AccountController(_brokerMock.Object, new Mock<IAuditLogger>().Object, new Mock<ILogger<AccountController>>().Object);
            ControllerTestHelper.SetupUser(_controller);
        }

        [Test]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            var req = new LoginRequest { Email = "test@test.com", Password = "Pass" };
            _brokerMock.Setup(x => x.AuthenticateAsync(req.Email, req.Password))
                       .ReturnsAsync(OperationResult<string>.Ok("fake-jwt-token"));

            var result = await _controller.Login(req);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var req = new LoginRequest { Email = "wrong@test.com", Password = "Pass" };
            _brokerMock.Setup(x => x.AuthenticateAsync(req.Email, req.Password))
                       .ReturnsAsync(OperationResult<string>.AccessDenied("Wrong email or password"));

            var result = await _controller.Login(req);

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task Register_ValidData_ReturnsCreated()
        {
            var req = new RegistrationRequest { DisplayName = "New User", Email = "new@test.com", Password = "Pass", Role = Learnify.Core.Enums.PlatformRole.Learner };
            _brokerMock.Setup(x => x.RegisterNewAccountAsync(req.DisplayName, req.Email, req.Password, req.Role))
                       .ReturnsAsync(OperationResult<LearnerAccount>.Ok(new LearnerAccount { Id = 1, EmailAddress = req.Email }));

            var result = await _controller.Register(req);

            Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        }

        [Test]
        public async Task GetProfile_ExistingUser_ReturnsOk()
        {
            _brokerMock.Setup(x => x.FetchAccountAsync(1))
                       .ReturnsAsync(OperationResult<LearnerAccount>.Ok(new LearnerAccount { Id = 1, DisplayName = "Test" }));

            var result = await _controller.GetProfile(1);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
}
