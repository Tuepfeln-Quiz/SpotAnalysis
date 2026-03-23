using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotAnalysis.Data.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChemicalTypes",
                columns: table => new
                {
                    ChemicalTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChemicalTypes", x => x.ChemicalTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    GroupID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.GroupID);
                });

            migrationBuilder.CreateTable(
                name: "Methods",
                columns: table => new
                {
                    MethodID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Methods", x => x.MethodID);
                });

            migrationBuilder.CreateTable(
                name: "Observations",
                columns: table => new
                {
                    ObservationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Observations", x => x.ObservationID);
                });

            migrationBuilder.CreateTable(
                name: "QuizStatus",
                columns: table => new
                {
                    QuizStatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizStatus", x => x.QuizStatusID);
                });

            migrationBuilder.CreateTable(
                name: "QuizTypes",
                columns: table => new
                {
                    QuizTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizTypes", x => x.QuizTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Chemicals",
                columns: table => new
                {
                    ChemicalID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChemicalTypeID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Formula = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chemicals", x => x.ChemicalID);
                    table.ForeignKey(
                        name: "FK_Chemicals_ChemicalTypes_ChemicalTypeID",
                        column: x => x.ChemicalTypeID,
                        principalTable: "ChemicalTypes",
                        principalColumn: "ChemicalTypeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    QuizID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuizTypeID = table.Column<int>(type: "int", nullable: false),
                    QuizStatusID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.QuizID);
                    table.ForeignKey(
                        name: "FK_Quizzes_QuizStatus_QuizStatusID",
                        column: x => x.QuizStatusID,
                        principalTable: "QuizStatus",
                        principalColumn: "QuizStatusID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quizzes_QuizTypes_QuizTypeID",
                        column: x => x.QuizTypeID,
                        principalTable: "QuizTypes",
                        principalColumn: "QuizTypeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserGroups",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false),
                    GroupID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroups", x => new { x.UserID, x.GroupID });
                    table.ForeignKey(
                        name: "FK_UserGroups_Groups_GroupID",
                        column: x => x.GroupID,
                        principalTable: "Groups",
                        principalColumn: "GroupID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserGroups_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserID, x.RoleID });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MethodOutputs",
                columns: table => new
                {
                    ChemicalID = table.Column<int>(type: "int", nullable: false),
                    MethodID = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MethodOutputs", x => new { x.ChemicalID, x.MethodID });
                    table.ForeignKey(
                        name: "FK_MethodOutputs_Chemicals_ChemicalID",
                        column: x => x.ChemicalID,
                        principalTable: "Chemicals",
                        principalColumn: "ChemicalID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MethodOutputs_Methods_MethodID",
                        column: x => x.MethodID,
                        principalTable: "Methods",
                        principalColumn: "MethodID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reactions",
                columns: table => new
                {
                    ReactionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Chemical1ID = table.Column<int>(type: "int", nullable: false),
                    Chemical2ID = table.Column<int>(type: "int", nullable: false),
                    RelevantProduct = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Formula = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ObservationID = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reactions", x => x.ReactionID);
                    table.ForeignKey(
                        name: "FK_Reactions_Chemicals_Chemical1ID",
                        column: x => x.Chemical1ID,
                        principalTable: "Chemicals",
                        principalColumn: "ChemicalID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reactions_Chemicals_Chemical2ID",
                        column: x => x.Chemical2ID,
                        principalTable: "Chemicals",
                        principalColumn: "ChemicalID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reactions_Observations_ObservationID",
                        column: x => x.ObservationID,
                        principalTable: "Observations",
                        principalColumn: "ObservationID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupQuizzes",
                columns: table => new
                {
                    GroupID = table.Column<int>(type: "int", nullable: false),
                    QuizID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupQuizzes", x => new { x.GroupID, x.QuizID });
                    table.ForeignKey(
                        name: "FK_GroupQuizzes_Groups_GroupID",
                        column: x => x.GroupID,
                        principalTable: "Groups",
                        principalColumn: "GroupID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupQuizzes_Quizzes_QuizID",
                        column: x => x.QuizID,
                        principalTable: "Quizzes",
                        principalColumn: "QuizID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuizAttempts",
                columns: table => new
                {
                    AttemptID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    QuizID = table.Column<int>(type: "int", nullable: false),
                    Started = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Completed = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttempts", x => x.AttemptID);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_Quizzes_QuizID",
                        column: x => x.QuizID,
                        principalTable: "Quizzes",
                        principalColumn: "QuizID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "STLQuestions",
                columns: table => new
                {
                    QuestionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizID = table.Column<int>(type: "int", nullable: false),
                    ChemicalID = table.Column<int>(type: "int", nullable: false),
                    ObservationID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STLQuestions", x => x.QuestionID);
                    table.ForeignKey(
                        name: "FK_STLQuestions_Chemicals_ChemicalID",
                        column: x => x.ChemicalID,
                        principalTable: "Chemicals",
                        principalColumn: "ChemicalID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_STLQuestions_Observations_ObservationID",
                        column: x => x.ObservationID,
                        principalTable: "Observations",
                        principalColumn: "ObservationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_STLQuestions_Quizzes_QuizID",
                        column: x => x.QuizID,
                        principalTable: "Quizzes",
                        principalColumn: "QuizID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "STQuestion",
                columns: table => new
                {
                    QuestionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChemicalID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STQuestion", x => x.QuestionID);
                    table.ForeignKey(
                        name: "FK_STQuestion_Chemicals_ChemicalID",
                        column: x => x.ChemicalID,
                        principalTable: "Chemicals",
                        principalColumn: "ChemicalID");
                    table.ForeignKey(
                        name: "FK_STQuestion_Quizzes_QuizID",
                        column: x => x.QuizID,
                        principalTable: "Quizzes",
                        principalColumn: "QuizID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "STLAvailableReactions",
                columns: table => new
                {
                    QuestionID = table.Column<int>(type: "int", nullable: false),
                    ReactionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STLAvailableReactions", x => new { x.QuestionID, x.ReactionID });
                    table.ForeignKey(
                        name: "FK_STLAvailableReactions_Reactions_ReactionID",
                        column: x => x.ReactionID,
                        principalTable: "Reactions",
                        principalColumn: "ReactionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_STLAvailableReactions_STLQuestions_QuestionID",
                        column: x => x.QuestionID,
                        principalTable: "STLQuestions",
                        principalColumn: "QuestionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "STLResults",
                columns: table => new
                {
                    ResultID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttemptID = table.Column<int>(type: "int", nullable: false),
                    QuestionID = table.Column<int>(type: "int", nullable: false),
                    ChosenChemicalID = table.Column<int>(type: "int", nullable: false),
                    ChosenReactionID = table.Column<int>(type: "int", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STLResults", x => x.ResultID);
                    table.ForeignKey(
                        name: "FK_STLResults_Chemicals_ChosenChemicalID",
                        column: x => x.ChosenChemicalID,
                        principalTable: "Chemicals",
                        principalColumn: "ChemicalID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_STLResults_QuizAttempts_AttemptID",
                        column: x => x.AttemptID,
                        principalTable: "QuizAttempts",
                        principalColumn: "AttemptID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_STLResults_Reactions_ChosenReactionID",
                        column: x => x.ChosenReactionID,
                        principalTable: "Reactions",
                        principalColumn: "ReactionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_STLResults_STLQuestions_QuestionID",
                        column: x => x.QuestionID,
                        principalTable: "STLQuestions",
                        principalColumn: "QuestionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "STAvailableChemicals",
                columns: table => new
                {
                    QuestionID = table.Column<int>(type: "int", nullable: false),
                    ChemicalID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STAvailableChemicals", x => new { x.QuestionID, x.ChemicalID });
                    table.ForeignKey(
                        name: "FK_STAvailableChemicals_Chemicals_ChemicalID",
                        column: x => x.ChemicalID,
                        principalTable: "Chemicals",
                        principalColumn: "ChemicalID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_STAvailableChemicals_STQuestion_QuestionID",
                        column: x => x.QuestionID,
                        principalTable: "STQuestion",
                        principalColumn: "QuestionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "STAvailableMethods",
                columns: table => new
                {
                    QuestionID = table.Column<int>(type: "int", nullable: false),
                    MethodID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STAvailableMethods", x => new { x.QuestionID, x.MethodID });
                    table.ForeignKey(
                        name: "FK_STAvailableMethods_Methods_MethodID",
                        column: x => x.MethodID,
                        principalTable: "Methods",
                        principalColumn: "MethodID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_STAvailableMethods_STQuestion_QuestionID",
                        column: x => x.QuestionID,
                        principalTable: "STQuestion",
                        principalColumn: "QuestionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "STResults",
                columns: table => new
                {
                    ResultID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttemptID = table.Column<int>(type: "int", nullable: false),
                    QuestionID = table.Column<int>(type: "int", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    ChemicalID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STResults", x => x.ResultID);
                    table.ForeignKey(
                        name: "FK_STResults_Chemicals_ChemicalID",
                        column: x => x.ChemicalID,
                        principalTable: "Chemicals",
                        principalColumn: "ChemicalID");
                    table.ForeignKey(
                        name: "FK_STResults_QuizAttempts_AttemptID",
                        column: x => x.AttemptID,
                        principalTable: "QuizAttempts",
                        principalColumn: "AttemptID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_STResults_STQuestion_QuestionID",
                        column: x => x.QuestionID,
                        principalTable: "STQuestion",
                        principalColumn: "QuestionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "STChemicalResults",
                columns: table => new
                {
                    ChemicalResultID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResultID = table.Column<int>(type: "int", nullable: false),
                    SelectedChemicalID = table.Column<int>(type: "int", nullable: false),
                    ChosenChemicalID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STChemicalResults", x => x.ChemicalResultID);
                    table.ForeignKey(
                        name: "FK_STChemicalResults_Chemicals_ChosenChemicalID",
                        column: x => x.ChosenChemicalID,
                        principalTable: "Chemicals",
                        principalColumn: "ChemicalID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_STChemicalResults_Chemicals_SelectedChemicalID",
                        column: x => x.SelectedChemicalID,
                        principalTable: "Chemicals",
                        principalColumn: "ChemicalID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_STChemicalResults_STResults_ResultID",
                        column: x => x.ResultID,
                        principalTable: "STResults",
                        principalColumn: "ResultID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "STLogs",
                columns: table => new
                {
                    LogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResultID = table.Column<int>(type: "int", nullable: false),
                    Chemical1ID = table.Column<int>(type: "int", nullable: false),
                    Chemical2ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STLogs", x => x.LogID);
                    table.ForeignKey(
                        name: "FK_STLogs_Chemicals_Chemical1ID",
                        column: x => x.Chemical1ID,
                        principalTable: "Chemicals",
                        principalColumn: "ChemicalID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_STLogs_Chemicals_Chemical2ID",
                        column: x => x.Chemical2ID,
                        principalTable: "Chemicals",
                        principalColumn: "ChemicalID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_STLogs_STResults_ResultID",
                        column: x => x.ResultID,
                        principalTable: "STResults",
                        principalColumn: "ResultID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chemicals_ChemicalTypeID",
                table: "Chemicals",
                column: "ChemicalTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_GroupQuizzes_QuizID",
                table: "GroupQuizzes",
                column: "QuizID");

            migrationBuilder.CreateIndex(
                name: "IX_MethodOutputs_MethodID",
                table: "MethodOutputs",
                column: "MethodID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_QuizID",
                table: "QuizAttempts",
                column: "QuizID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_UserID",
                table: "QuizAttempts",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_QuizStatusID",
                table: "Quizzes",
                column: "QuizStatusID");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_QuizTypeID",
                table: "Quizzes",
                column: "QuizTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_Chemical1ID",
                table: "Reactions",
                column: "Chemical1ID");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_Chemical2ID",
                table: "Reactions",
                column: "Chemical2ID");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_ObservationID",
                table: "Reactions",
                column: "ObservationID");

            migrationBuilder.CreateIndex(
                name: "IX_STAvailableChemicals_ChemicalID",
                table: "STAvailableChemicals",
                column: "ChemicalID");

            migrationBuilder.CreateIndex(
                name: "IX_STAvailableMethods_MethodID",
                table: "STAvailableMethods",
                column: "MethodID");

            migrationBuilder.CreateIndex(
                name: "IX_STChemicalResults_ChosenChemicalID",
                table: "STChemicalResults",
                column: "ChosenChemicalID");

            migrationBuilder.CreateIndex(
                name: "IX_STChemicalResults_ResultID",
                table: "STChemicalResults",
                column: "ResultID");

            migrationBuilder.CreateIndex(
                name: "IX_STChemicalResults_SelectedChemicalID",
                table: "STChemicalResults",
                column: "SelectedChemicalID");

            migrationBuilder.CreateIndex(
                name: "IX_STLAvailableReactions_ReactionID",
                table: "STLAvailableReactions",
                column: "ReactionID");

            migrationBuilder.CreateIndex(
                name: "IX_STLogs_Chemical1ID",
                table: "STLogs",
                column: "Chemical1ID");

            migrationBuilder.CreateIndex(
                name: "IX_STLogs_Chemical2ID",
                table: "STLogs",
                column: "Chemical2ID");

            migrationBuilder.CreateIndex(
                name: "IX_STLogs_ResultID",
                table: "STLogs",
                column: "ResultID");

            migrationBuilder.CreateIndex(
                name: "IX_STLQuestions_ChemicalID",
                table: "STLQuestions",
                column: "ChemicalID");

            migrationBuilder.CreateIndex(
                name: "IX_STLQuestions_ObservationID",
                table: "STLQuestions",
                column: "ObservationID");

            migrationBuilder.CreateIndex(
                name: "IX_STLQuestions_QuizID",
                table: "STLQuestions",
                column: "QuizID");

            migrationBuilder.CreateIndex(
                name: "IX_STLResults_AttemptID",
                table: "STLResults",
                column: "AttemptID");

            migrationBuilder.CreateIndex(
                name: "IX_STLResults_ChosenChemicalID",
                table: "STLResults",
                column: "ChosenChemicalID");

            migrationBuilder.CreateIndex(
                name: "IX_STLResults_ChosenReactionID",
                table: "STLResults",
                column: "ChosenReactionID");

            migrationBuilder.CreateIndex(
                name: "IX_STLResults_QuestionID",
                table: "STLResults",
                column: "QuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_STQuestion_ChemicalID",
                table: "STQuestion",
                column: "ChemicalID");

            migrationBuilder.CreateIndex(
                name: "IX_STQuestion_QuizID",
                table: "STQuestion",
                column: "QuizID");

            migrationBuilder.CreateIndex(
                name: "IX_STResults_AttemptID",
                table: "STResults",
                column: "AttemptID");

            migrationBuilder.CreateIndex(
                name: "IX_STResults_ChemicalID",
                table: "STResults",
                column: "ChemicalID");

            migrationBuilder.CreateIndex(
                name: "IX_STResults_QuestionID",
                table: "STResults",
                column: "QuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_GroupID",
                table: "UserGroups",
                column: "GroupID");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleID",
                table: "UserRoles",
                column: "RoleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupQuizzes");

            migrationBuilder.DropTable(
                name: "MethodOutputs");

            migrationBuilder.DropTable(
                name: "STAvailableChemicals");

            migrationBuilder.DropTable(
                name: "STAvailableMethods");

            migrationBuilder.DropTable(
                name: "STChemicalResults");

            migrationBuilder.DropTable(
                name: "STLAvailableReactions");

            migrationBuilder.DropTable(
                name: "STLogs");

            migrationBuilder.DropTable(
                name: "STLResults");

            migrationBuilder.DropTable(
                name: "UserGroups");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Methods");

            migrationBuilder.DropTable(
                name: "STResults");

            migrationBuilder.DropTable(
                name: "Reactions");

            migrationBuilder.DropTable(
                name: "STLQuestions");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "QuizAttempts");

            migrationBuilder.DropTable(
                name: "STQuestion");

            migrationBuilder.DropTable(
                name: "Observations");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Chemicals");

            migrationBuilder.DropTable(
                name: "Quizzes");

            migrationBuilder.DropTable(
                name: "ChemicalTypes");

            migrationBuilder.DropTable(
                name: "QuizStatus");

            migrationBuilder.DropTable(
                name: "QuizTypes");
        }
    }
}
