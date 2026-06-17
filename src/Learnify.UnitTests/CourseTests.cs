using Moq;
using NUnit.Framework;
using Learnify.Courses.API.Application;
using Learnify.Courses.API.Endpoints;
using Learnify.Core.Core;
using Learnify.Core.Domain;
using Microsoft.AspNetCore.Mvc;
using Learnify.Courses.API.Contracts;

namespace Learnify.UnitTests
{
    [TestFixture]
    public class CourseTests
    {
        private Mock<ICourseCatalog> _catalogMock;
        private CourseCatalogController _controller;

        [SetUp]
        public void Setup()
        {
            _catalogMock = new Mock<ICourseCatalog>();
            _controller = new CourseCatalogController(_catalogMock.Object, new Mock<IAuditLogger>().Object);
            // Setup as SuperAdmin to pass IsSuperAdmin check
            ControllerTestHelper.SetupUser(_controller, userId: 10, email: "tiwariojas578@gmail.com", role: "Administrator");
        }

        [Test]
        public async Task BrowseCatalog_ReturnsLiveCourses()
        {
            var courses = new List<CourseOffering> { new CourseOffering { Id = 1, Title = "C# Basics", IsPublished = true, IsApprovedByAdmin = true } };
            _catalogMock.Setup(x => x.BrowseLiveCatalogAsync()).ReturnsAsync(courses);

            var result = await _controller.BrowseCatalog();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var payload = okResult.Value as IEnumerable<CourseView>;
            Assert.That(payload.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetCourse_ExistingCourse_ReturnsOk()
        {
            _catalogMock.Setup(x => x.GetCourseAsync(1))
                        .ReturnsAsync(OperationResult<CourseOffering>.Ok(new CourseOffering { Id = 1, Title = "C#" }));

            var result = await _controller.GetCourse(1);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task SubmitForReview_ValidCourse_ReturnsOk()
        {
            _catalogMock.Setup(x => x.SubmitForReviewAsync(1, It.IsAny<int>()))
                        .ReturnsAsync(OperationResult<CourseOffering>.Ok(new CourseOffering { Id = 1 }));

            var result = await _controller.SubmitForReview(1);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Approve_Administrator_ReturnsOk()
        {
            _catalogMock.Setup(x => x.ApproveForLiveAsync(1))
                        .ReturnsAsync(OperationResult<CourseOffering>.Ok(new CourseOffering { Id = 1 }));

            var result = await _controller.Approve(1);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
}
