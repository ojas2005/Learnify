using Moq;
using NUnit.Framework;
using Learnify.Analytics.API.Application;
using Learnify.Analytics.API.Endpoints;
using Learnify.Analytics.API.Contracts;
using Learnify.Tracking.API.Application;
using Learnify.Exams.API.Application;
using Learnify.Exams.API.Endpoints;
using Learnify.Core.Core;
using Learnify.Core.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Learnify.UnitTests
{
    [TestFixture]
    public class AnalyticsTests
    {
        private Mock<IAnalyticsService> _aggregatorMock;
        private AnalyticsController _controller;

        [SetUp]
        public void Setup()
        {
            _aggregatorMock = new Mock<IAnalyticsService>();
            _controller = new AnalyticsController(_aggregatorMock.Object);
            // Setup as SuperAdmin
            ControllerTestHelper.SetupUser(_controller, userId: 10, email: "tiwariojas578@gmail.com", role: "Administrator");
        }

        [Test]
        public async Task GetDashboardAnalytics_Administrator_ReturnsOk()
        {
            _aggregatorMock.Setup(x => x.GetPlatformAnalyticsAsync()).ReturnsAsync(new PlatformAnalytics());
            var result = await _controller.GetDashboardAnalytics();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }

    [TestFixture]
    public class ExamTests
    {
        private Mock<IExamEngine> _examinerMock;
        private ExamController _controller;

        [SetUp]
        public void Setup()
        {
            _examinerMock = new Mock<IExamEngine>();
            _controller = new ExamController(_examinerMock.Object);
            ControllerTestHelper.SetupUser(_controller);
        }

        [Test]
        public async Task GetCourseExams_ReturnsExams()
        {
            _examinerMock.Setup(x => x.GetExamsForCourseAsync(1)).ReturnsAsync(new List<CourseExam>());
            var result = await _controller.GetCourseExams(1);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
}
