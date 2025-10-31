using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomQuizzesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPracticeMode",
                table: "QuizSessions");

            migrationBuilder.CreateTable(
                name: "CustomQuizzes",
                columns: table => new
                {
                    CustomQuizId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    TimeLimit = table.Column<int>(type: "int", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    DifficultyLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
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
                    CustomQuizId = table.Column<int>(type: "int", nullable: false),
                    AssignedToUserId = table.Column<int>(type: "int", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomQuizAssignments");

            migrationBuilder.DropTable(
                name: "CustomQuizQuestions");

            migrationBuilder.DropTable(
                name: "CustomQuizzes");

            migrationBuilder.AddColumn<bool>(
                name: "IsPracticeMode",
                table: "QuizSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
