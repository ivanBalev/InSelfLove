using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InSelfLove.Data.Migrations
{
    /// <inheritdoc />
    public partial class _2023 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterDatabase()
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "AspNetRoles",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "varchar(85)", maxLength: 85, nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        DeletedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            //        Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        NormalizedName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetRoles", x => x.Id);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "AspNetUsers",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "varchar(85)", maxLength: 85, nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        DeletedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            //        ProfilePhoto = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        IsBanned = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        WindowsTimezoneId = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        NormalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        NormalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        PasswordHash = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        SecurityStamp = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        PhoneNumber = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        PhoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        LockoutEnd = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
            //        LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        AccessFailedCount = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "Courses",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "varchar(85)", maxLength: 85, nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Title = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ThumbnailLink = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        PriceId = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Price = table.Column<long>(type: "bigint", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        DeletedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Courses", x => x.Id);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "AspNetRoleClaims",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", maxLength: 85, nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        RoleId = table.Column<string>(type: "varchar(85)", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ClaimType = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ClaimValue = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
            //            column: x => x.RoleId,
            //            principalTable: "AspNetRoles",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "Appointments",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        Description = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        UtcStart = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            //        UserId = table.Column<string>(type: "varchar(85)", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        IsApproved = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        CanBeOnSite = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        IsOnSite = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        DeletedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Appointments", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Appointments_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "Articles",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        Title = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Slug = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Content = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ImageUrl = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        PreviewImageUrl = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        UserId = table.Column<string>(type: "varchar(85)", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        DeletedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Articles", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Articles_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id");
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserClaims",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", maxLength: 85, nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        UserId = table.Column<string>(type: "varchar(85)", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ClaimType = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ClaimValue = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AspNetUserClaims_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserLogins",
            //    columns: table => new
            //    {
            //        LoginProvider = table.Column<string>(type: "varchar(85)", maxLength: 85, nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ProviderKey = table.Column<string>(type: "varchar(85)", maxLength: 85, nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ProviderDisplayName = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        UserId = table.Column<string>(type: "varchar(85)", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
            //        table.ForeignKey(
            //            name: "FK_AspNetUserLogins_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserRoles",
            //    columns: table => new
            //    {
            //        UserId = table.Column<string>(type: "varchar(85)", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        RoleId = table.Column<string>(type: "varchar(85)", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
            //        table.ForeignKey(
            //            name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
            //            column: x => x.RoleId,
            //            principalTable: "AspNetRoles",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //        table.ForeignKey(
            //            name: "FK_AspNetUserRoles_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserTokens",
            //    columns: table => new
            //    {
            //        UserId = table.Column<string>(type: "varchar(85)", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        LoginProvider = table.Column<string>(type: "varchar(85)", maxLength: 85, nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Name = table.Column<string>(type: "varchar(85)", maxLength: 85, nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Value = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
            //        table.ForeignKey(
            //            name: "FK_AspNetUserTokens_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "Payment",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "varchar(255)", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ApplicationUserId = table.Column<string>(type: "varchar(85)", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        StripeCustomerId = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        AmountTotal = table.Column<long>(type: "bigint", nullable: false),
            //        CourseId = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        AppointmentId = table.Column<int>(type: "int", nullable: true),
            //        CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        DeletedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Payment", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Payment_AspNetUsers_ApplicationUserId",
            //            column: x => x.ApplicationUserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id");
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "Videos",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        Url = table.Column<string>(type: "longtext", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        UserId = table.Column<string>(type: "varchar(85)", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Title = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Slug = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        AssociatedTerms = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        DeletedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Videos", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Videos_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id");
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "ApplicationUserCourse",
            //    columns: table => new
            //    {
            //        ApplicationUsersId = table.Column<string>(type: "varchar(85)", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        CoursesId = table.Column<string>(type: "varchar(85)", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ApplicationUserCourse", x => new { x.ApplicationUsersId, x.CoursesId });
            //        table.ForeignKey(
            //            name: "FK_ApplicationUserCourse_AspNetUsers_ApplicationUsersId",
            //            column: x => x.ApplicationUsersId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //        table.ForeignKey(
            //            name: "FK_ApplicationUserCourse_Courses_CoursesId",
            //            column: x => x.CoursesId,
            //            principalTable: "Courses",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "CourseVideos",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "varchar(255)", nullable: false)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        Title = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ThumbnailLink = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        CourseId = table.Column<string>(type: "varchar(85)", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        IsPreview = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        DeletedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CourseVideos", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_CourseVideos_Courses_CourseId",
            //            column: x => x.CourseId,
            //            principalTable: "Courses",
            //            principalColumn: "Id");
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateTable(
            //    name: "Comments",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //        Content = table.Column<string>(type: "longtext", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        UserId = table.Column<string>(type: "varchar(85)", nullable: true)
            //            .Annotation("MySql:CharSet", "utf8mb4"),
            //        ArticleId = table.Column<int>(type: "int", nullable: true),
            //        VideoId = table.Column<int>(type: "int", nullable: true),
            //        ParentCommentId = table.Column<int>(type: "int", nullable: true),
            //        CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            //        ModifiedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
            //        DeletedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Comments", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Comments_Articles_ArticleId",
            //            column: x => x.ArticleId,
            //            principalTable: "Articles",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //        table.ForeignKey(
            //            name: "FK_Comments_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //        table.ForeignKey(
            //            name: "FK_Comments_Comments_ParentCommentId",
            //            column: x => x.ParentCommentId,
            //            principalTable: "Comments",
            //            principalColumn: "Id");
            //        table.ForeignKey(
            //            name: "FK_Comments_Videos_VideoId",
            //            column: x => x.VideoId,
            //            principalTable: "Videos",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    })
            //    .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ApplicationUserCourse_CoursesId",
            //    table: "ApplicationUserCourse",
            //    column: "CoursesId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Appointments_IsDeleted",
            //    table: "Appointments",
            //    column: "IsDeleted");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Appointments_UserId",
            //    table: "Appointments",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Articles_IsDeleted",
            //    table: "Articles",
            //    column: "IsDeleted");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Articles_UserId",
            //    table: "Articles",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetRoleClaims_RoleId",
            //    table: "AspNetRoleClaims",
            //    column: "RoleId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetRoles_IsDeleted",
            //    table: "AspNetRoles",
            //    column: "IsDeleted");

            //migrationBuilder.CreateIndex(
            //    name: "RoleNameIndex",
            //    table: "AspNetRoles",
            //    column: "NormalizedName",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUserClaims_UserId",
            //    table: "AspNetUserClaims",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUserLogins_UserId",
            //    table: "AspNetUserLogins",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUserRoles_RoleId",
            //    table: "AspNetUserRoles",
            //    column: "RoleId");

            //migrationBuilder.CreateIndex(
            //    name: "EmailIndex",
            //    table: "AspNetUsers",
            //    column: "NormalizedEmail");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUsers_IsDeleted",
            //    table: "AspNetUsers",
            //    column: "IsDeleted");

            //migrationBuilder.CreateIndex(
            //    name: "UserNameIndex",
            //    table: "AspNetUsers",
            //    column: "NormalizedUserName",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_Comments_ArticleId",
            //    table: "Comments",
            //    column: "ArticleId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Comments_IsDeleted",
            //    table: "Comments",
            //    column: "IsDeleted");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Comments_ParentCommentId",
            //    table: "Comments",
            //    column: "ParentCommentId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Comments_UserId",
            //    table: "Comments",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Comments_VideoId",
            //    table: "Comments",
            //    column: "VideoId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Courses_IsDeleted",
            //    table: "Courses",
            //    column: "IsDeleted");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CourseVideos_CourseId",
            //    table: "CourseVideos",
            //    column: "CourseId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CourseVideos_IsDeleted",
            //    table: "CourseVideos",
            //    column: "IsDeleted");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Payment_ApplicationUserId",
            //    table: "Payment",
            //    column: "ApplicationUserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Payment_IsDeleted",
            //    table: "Payment",
            //    column: "IsDeleted");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Videos_IsDeleted",
            //    table: "Videos",
            //    column: "IsDeleted");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Videos_UserId",
            //    table: "Videos",
            //    column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationUserCourse");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "CourseVideos");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "Videos");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
