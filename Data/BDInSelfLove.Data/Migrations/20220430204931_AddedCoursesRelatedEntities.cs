using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BDInSelfLove.Data.Migrations
{
    public partial class AddedCoursesRelatedEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(85)", maxLength: 85, nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    ThumbnailLink = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserCourses",
                columns: table => new
                {
                    CourseId = table.Column<string>(type: "varchar(85)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(85)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserCourses", x => new { x.CourseId, x.UserId });
                    table.ForeignKey(
                        name: "FK_AspNetUserCourses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUserCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseVideos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(767)", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    ThumbnailLink = table.Column<string>(type: "text", nullable: true),
                    CourseId = table.Column<string>(type: "varchar(85)", nullable: true),
                    IsPreview = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseVideos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseVideos_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserCourses_UserId",
                table: "AspNetUserCourses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_IsDeleted",
                table: "Courses",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CourseVideos_CourseId",
                table: "CourseVideos",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseVideos_IsDeleted",
                table: "CourseVideos",
                column: "IsDeleted");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetUserCourses");

            migrationBuilder.DropTable(
                name: "CourseVideos");

            migrationBuilder.DropTable(
                name: "Courses");
        }
    }
}
