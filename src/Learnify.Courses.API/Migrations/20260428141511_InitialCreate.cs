using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learnify.Courses.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearnerAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    HashedPassword = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    ProfilePictureUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RegisteredOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseOfferings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Synopsis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ListPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsApprovedByAdmin = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalRuntimeMinutes = table.Column<int>(type: "int", nullable: false),
                    TotalRegistrations = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseOfferings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseOfferings_LearnerAccounts_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "LearnerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompletionCredential",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CredentialCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    LearnerId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    AwardedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DocumentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VerificationPin = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletionCredential", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompletionCredential_CourseOfferings_CourseId",
                        column: x => x.CourseId,
                        principalTable: "CourseOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompletionCredential_LearnerAccounts_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "LearnerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseExams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    LessonId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    QuestionsPayload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PassThreshold = table.Column<int>(type: "int", nullable: false),
                    AttemptsAllowed = table.Column<int>(type: "int", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseExams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseExams_CourseOfferings_CourseId",
                        column: x => x.CourseId,
                        principalTable: "CourseOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseFeedback",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LearnerId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    StarRating = table.Column<int>(type: "int", nullable: false),
                    ReviewText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    SubmittedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastEditedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseFeedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseFeedback_CourseOfferings_CourseId",
                        column: x => x.CourseId,
                        principalTable: "CourseOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseFeedback_LearnerAccounts_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "LearnerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LearnerId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    RegisteredOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CompletionPercent = table.Column<int>(type: "int", nullable: false),
                    LastOpenedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CredentialIssued = table.Column<bool>(type: "bit", nullable: false),
                    PaymentReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseRegistrations_CourseOfferings_CourseId",
                        column: x => x.CourseId,
                        principalTable: "CourseOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseRegistrations_LearnerAccounts_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "LearnerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumLessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Format = table.Column<int>(type: "int", nullable: false),
                    MediaUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SequencePosition = table.Column<int>(type: "int", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsPreviewable = table.Column<bool>(type: "bit", nullable: false),
                    AddedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumLessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumLessons_CourseOfferings_CourseId",
                        column: x => x.CourseId,
                        principalTable: "CourseOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamAttempt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    LearnerId = table.Column<int>(type: "int", nullable: false),
                    BeganAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: false),
                    HasPassed = table.Column<bool>(type: "bit", nullable: false),
                    AnswersPayload = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamAttempt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamAttempt_CourseExams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "CourseExams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamAttempt_LearnerAccounts_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "LearnerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonWatchRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LearnerId = table.Column<int>(type: "int", nullable: false),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    IsFinished = table.Column<bool>(type: "bit", nullable: false),
                    FinishedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SecondsWatched = table.Column<int>(type: "int", nullable: false),
                    LastWatchedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CourseRegistrationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonWatchRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonWatchRecord_CourseRegistrations_CourseRegistrationId",
                        column: x => x.CourseRegistrationId,
                        principalTable: "CourseRegistrations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LessonWatchRecord_CurriculumLessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "CurriculumLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonWatchRecord_LearnerAccounts_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "LearnerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompletionCredential_CourseId",
                table: "CompletionCredential",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletionCredential_LearnerId",
                table: "CompletionCredential",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseExams_CourseId",
                table: "CourseExams",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseFeedback_CourseId",
                table: "CourseFeedback",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseFeedback_LearnerId",
                table: "CourseFeedback",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferings_AuthorId",
                table: "CourseOfferings",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseRegistrations_CourseId",
                table: "CourseRegistrations",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseRegistrations_LearnerId",
                table: "CourseRegistrations",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumLessons_CourseId",
                table: "CurriculumLessons",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamAttempt_ExamId",
                table: "ExamAttempt",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamAttempt_LearnerId",
                table: "ExamAttempt",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonWatchRecord_CourseRegistrationId",
                table: "LessonWatchRecord",
                column: "CourseRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonWatchRecord_LearnerId",
                table: "LessonWatchRecord",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonWatchRecord_LessonId",
                table: "LessonWatchRecord",
                column: "LessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompletionCredential");

            migrationBuilder.DropTable(
                name: "CourseFeedback");

            migrationBuilder.DropTable(
                name: "ExamAttempt");

            migrationBuilder.DropTable(
                name: "LessonWatchRecord");

            migrationBuilder.DropTable(
                name: "CourseExams");

            migrationBuilder.DropTable(
                name: "CourseRegistrations");

            migrationBuilder.DropTable(
                name: "CurriculumLessons");

            migrationBuilder.DropTable(
                name: "CourseOfferings");

            migrationBuilder.DropTable(
                name: "LearnerAccounts");
        }
    }
}
