using Moq;
using NUnit.Framework;
using Learnify.Registration.API.Application;
using Learnify.Registration.API.Endpoints;
using Learnify.Core.Core;
using Learnify.Core.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Learnify.UnitTests
{
    [TestFixture]
    public class RegistrationTests
    {
        private Mock<ISeatReservation> _seatsMock;
        private RegistrationController _controller;

        [SetUp]
        public void Setup()
        {
            _seatsMock = new Mock<ISeatReservation>();
            _controller = new RegistrationController(_seatsMock.Object, new Mock<IAuditLogger>().Object);
            ControllerTestHelper.SetupUser(_controller, userId: 1);
        }

        [Test]
        public async Task Enroll_NewStudent_ReturnsOk()
        {
            var req = new EnrollRequest { CourseId = 1 };
            _seatsMock.Setup(x => x.ClaimSeatAsync(1, 1))
                      .ReturnsAsync(OperationResult<CourseRegistration>.Ok(new CourseRegistration { Id = 100 }));

            var result = await _controller.Enroll(req);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UpdateProgress_ValidPercent_ReturnsNoContent()
        {
            var update = new ProgressUpdate { CompletionPercent = 50 };
            _seatsMock.Setup(x => x.GetRegistrationAsync(1))
                      .ReturnsAsync(OperationResult<CourseRegistration>.Ok(new CourseRegistration { Id = 1, LearnerId = 1 })); 
            _seatsMock.Setup(x => x.RecordProgressAsync(1, 50))
                      .ReturnsAsync(OperationResult.Done);

            var result = await _controller.UpdateProgress(1, update);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task MarkComplete_ValidRegistration_ReturnsNoContent()
        {
             _seatsMock.Setup(x => x.GetRegistrationAsync(1))
                      .ReturnsAsync(OperationResult<CourseRegistration>.Ok(new CourseRegistration { Id = 1, LearnerId = 1 }));
            _seatsMock.Setup(x => x.MarkFinishedAsync(1))
                      .ReturnsAsync(OperationResult.Done);

            var result = await _controller.MarkComplete(1);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }
    }
}
