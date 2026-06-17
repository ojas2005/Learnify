using Moq;
using NUnit.Framework;
using Learnify.Curriculum.API.Application;
using Learnify.Curriculum.API.Endpoints;
using Learnify.Core.Core;
using Learnify.Core.Domain;
using Microsoft.AspNetCore.Mvc;
using Learnify.Reviews.API.Application;
using Learnify.Reviews.API.Endpoints;
using Learnify.Reviews.API.Contracts;

namespace Learnify.UnitTests
{
    [TestFixture]
    public class CurriculumTests
    {
        private Mock<ICurriculumBuilder> _curriculumMock;
        private CurriculumController _controller;

        [SetUp]
        public void Setup()
        {
            _curriculumMock = new Mock<ICurriculumBuilder>();
            _controller = new CurriculumController(_curriculumMock.Object);
            ControllerTestHelper.SetupUser(_controller);
        }

        [Test]
        public async Task GetCurriculum_ReturnsLessons()
        {
            var lessons = new List<CurriculumLesson> { new CurriculumLesson { Id = 1, Title = "Intro" } };
            _curriculumMock.Setup(x => x.GetCourseCurriculumAsync(1)).ReturnsAsync(lessons);

            var result = await _controller.GetCurriculum(1);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task AddLesson_ValidData_ReturnsOk()
        {
            var req = new AddLessonRequest { Title = "New", MediaUrl = "http://link" };
            _curriculumMock.Setup(x => x.AppendLessonAsync(It.IsAny<CurriculumLesson>()))
                           .ReturnsAsync(OperationResult<CurriculumLesson>.Ok(new CurriculumLesson { Id = 2 }));

            var result = await _controller.AddLesson(1, req);

            Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        }
    }

    [TestFixture]
    public class ReviewTests
    {
        private Mock<IReviewModerator> _reviewsMock;
        private LearnerReviewsController _learnerController;
        private ReviewModerationController _adminController;

        [SetUp]
        public void Setup()
        {
            _reviewsMock = new Mock<IReviewModerator>();
            _learnerController = new LearnerReviewsController(_reviewsMock.Object);
            _adminController = new ReviewModerationController(_reviewsMock.Object);
            ControllerTestHelper.SetupUser(_learnerController, userId: 1);
            ControllerTestHelper.SetupUser(_adminController, userId: 10, email: "tiwariojas578@gmail.com", role: "Administrator");
        }

        [Test]
        public async Task PostReview_ValidData_ReturnsOk()
        {
            var req = new ReviewSubmission { CourseId = 1, StarRating = 5, Comment = "Great" };
            _reviewsMock.Setup(x => x.SubmitReviewAsync(1, 1, 5, "Great"))
                        .ReturnsAsync(OperationResult<CourseFeedback>.Ok(new CourseFeedback { Id = 10 }));

            var result = await _learnerController.PostReview(req);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetPending_Administrator_ReturnsOk()
        {
            var pending = new List<CourseFeedback> { new CourseFeedback { Id = 1 } };
            _reviewsMock.Setup(x => x.GetPendingReviewsAsync()).ReturnsAsync(pending);

            var result = await _adminController.GetPendingReviews();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
}
