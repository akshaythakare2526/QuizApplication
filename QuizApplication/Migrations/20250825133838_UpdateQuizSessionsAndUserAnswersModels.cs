using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizApplication.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuizSessionsAndUserAnswersModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SelectedOptionI",
                table: "UserAnswers",
                newName: "SelectedOption");

            migrationBuilder.AddColumn<DateTime>(
                name: "AnsweredAt",
                table: "UserAnswers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeTaken",
                table: "UserAnswers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DifficultyLevel",
                table: "QuizSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxPossibleScore",
                table: "QuizSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfQuestions",
                table: "QuizSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "QuizTitle",
                table: "QuizSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedCategories",
                table: "QuizSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalScore",
                table: "QuizSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_QuestionId",
                table: "UserAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_SessionId",
                table: "UserAnswers",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessions_UserId",
                table: "QuizSessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizSessions_Users_UserId",
                table: "QuizSessions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_Questions_QuestionId",
                table: "UserAnswers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "QuestionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_QuizSessions_SessionId",
                table: "UserAnswers",
                column: "SessionId",
                principalTable: "QuizSessions",
                principalColumn: "SessionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizSessions_Users_UserId",
                table: "QuizSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_Questions_QuestionId",
                table: "UserAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_QuizSessions_SessionId",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_QuestionId",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_SessionId",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_QuizSessions_UserId",
                table: "QuizSessions");

            migrationBuilder.DropColumn(
                name: "AnsweredAt",
                table: "UserAnswers");

            migrationBuilder.DropColumn(
                name: "TimeTaken",
                table: "UserAnswers");

            migrationBuilder.DropColumn(
                name: "DifficultyLevel",
                table: "QuizSessions");

            migrationBuilder.DropColumn(
                name: "MaxPossibleScore",
                table: "QuizSessions");

            migrationBuilder.DropColumn(
                name: "NumberOfQuestions",
                table: "QuizSessions");

            migrationBuilder.DropColumn(
                name: "QuizTitle",
                table: "QuizSessions");

            migrationBuilder.DropColumn(
                name: "SelectedCategories",
                table: "QuizSessions");

            migrationBuilder.DropColumn(
                name: "TotalScore",
                table: "QuizSessions");

            migrationBuilder.RenameColumn(
                name: "SelectedOption",
                table: "UserAnswers",
                newName: "SelectedOptionI");
        }
    }
}
