using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizApplication.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceCustomQuizWithUserCustomQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomQuizAssignments");

            migrationBuilder.DropTable(
                name: "CustomQuizQuestions");

            migrationBuilder.DropTable(
                name: "CustomQuizzes");

            migrationBuilder.CreateTable(
                name: "UserCustomQuizzes",
                columns: table => new
                {
                    UserQuizId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    TimeLimit = table.Column<int>(type: "int", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCustomQuizzes", x => x.UserQuizId);
                    table.ForeignKey(
                        name: "FK_UserCustomQuizzes_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCustomQuizAssignments",
                columns: table => new
                {
                    AssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserQuizId = table.Column<int>(type: "int", nullable: false),
                    AssignedToUserId = table.Column<int>(type: "int", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsViewed = table.Column<bool>(type: "bit", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: true),
                    TotalQuestions = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCustomQuizAssignments", x => x.AssignmentId);
                    table.ForeignKey(
                        name: "FK_UserCustomQuizAssignments_UserCustomQuizzes_UserQuizId",
                        column: x => x.UserQuizId,
                        principalTable: "UserCustomQuizzes",
                        principalColumn: "UserQuizId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCustomQuizAssignments_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserCustomQuizQuestions",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserQuizId = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Option1 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Option2 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Option3 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Option4 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    QuestionOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCustomQuizQuestions", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_UserCustomQuizQuestions_UserCustomQuizzes_UserQuizId",
                        column: x => x.UserQuizId,
                        principalTable: "UserCustomQuizzes",
                        principalColumn: "UserQuizId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCustomQuizAnswers",
                columns: table => new
                {
                    AnswerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignmentId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    SelectedAnswer = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    AnsweredDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCustomQuizAnswers", x => x.AnswerId);
                    table.ForeignKey(
                        name: "FK_UserCustomQuizAnswers_UserCustomQuizAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "UserCustomQuizAssignments",
                        principalColumn: "AssignmentId");
                    table.ForeignKey(
                        name: "FK_UserCustomQuizAnswers_UserCustomQuizQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "UserCustomQuizQuestions",
                        principalColumn: "QuestionId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCustomQuizAnswers_AssignmentId",
                table: "UserCustomQuizAnswers",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCustomQuizAnswers_QuestionId",
                table: "UserCustomQuizAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCustomQuizAssignments_AssignedToUserId",
                table: "UserCustomQuizAssignments",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCustomQuizAssignments_UserQuizId",
                table: "UserCustomQuizAssignments",
                column: "UserQuizId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCustomQuizQuestions_UserQuizId",
                table: "UserCustomQuizQuestions",
                column: "UserQuizId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCustomQuizzes_CreatedByUserId",
                table: "UserCustomQuizzes",
                column: "CreatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCustomQuizAnswers");

            migrationBuilder.DropTable(
                name: "UserCustomQuizAssignments");

            migrationBuilder.DropTable(
                name: "UserCustomQuizQuestions");

            migrationBuilder.DropTable(
                name: "UserCustomQuizzes");

            migrationBuilder.CreateTable(
                name: "CustomQuizzes",
                columns: table => new
                {
                    CustomQuizId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DifficultyLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    TimeLimit = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomQuizzes", x => x.CustomQuizId);
                    table.ForeignKey(
                        name: "FK_CustomQuizzes_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId");
                    table.ForeignKey(
                        name: "FK_CustomQuizzes_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomQuizAssignments",
                columns: table => new
                {
                    AssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignedToUserId = table.Column<int>(type: "int", nullable: false),
                    CustomQuizId = table.Column<int>(type: "int", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsViewed = table.Column<bool>(type: "bit", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomQuizAssignments", x => x.AssignmentId);
                    table.ForeignKey(
                        name: "FK_CustomQuizAssignments_CustomQuizzes_CustomQuizId",
                        column: x => x.CustomQuizId,
                        principalTable: "CustomQuizzes",
                        principalColumn: "CustomQuizId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomQuizAssignments_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomQuizQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomQuizId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    QuestionOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomQuizQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomQuizQuestions_CustomQuizzes_CustomQuizId",
                        column: x => x.CustomQuizId,
                        principalTable: "CustomQuizzes",
                        principalColumn: "CustomQuizId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomQuizQuestions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomQuizAssignments_AssignedToUserId",
                table: "CustomQuizAssignments",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomQuizAssignments_CustomQuizId",
                table: "CustomQuizAssignments",
                column: "CustomQuizId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomQuizQuestions_CustomQuizId",
                table: "CustomQuizQuestions",
                column: "CustomQuizId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomQuizQuestions_QuestionId",
                table: "CustomQuizQuestions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomQuizzes_CategoryId",
                table: "CustomQuizzes",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomQuizzes_CreatedByUserId",
                table: "CustomQuizzes",
                column: "CreatedByUserId");
        }
    }
}
