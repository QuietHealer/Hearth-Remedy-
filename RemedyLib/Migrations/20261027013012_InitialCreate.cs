using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RemedyLib.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ingredients = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Steps = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Category = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    UsageMethod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuthorDeviceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsFlagged = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Votes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votes", x => x.Id);
                    table.CheckConstraint("CK_Votes_Value", "\"Value\" IN (-1, 1)");
                    table.ForeignKey(
                        name: "FK_Votes_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "AuthorDeviceId", "Category", "CreatedUtc", "Ingredients", "IsFlagged", "IsHidden", "Steps", "Title", "UsageMethod" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "seed", "Sore Throat", new DateTime(2026, 10, 1, 12, 0, 0, 0, DateTimeKind.Utc), "1 tbsp honey\nJuice of half a lemon\n1 cup warm (not boiling) water", false, false, "Stir the honey and lemon juice into the warm water.\nSip slowly.\nDo not give honey to children under 1 year old.", "Honey and lemon for a sore throat", "Eat" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "seed", "Cold & Flu", new DateTime(2026, 10, 1, 12, 1, 0, 0, DateTimeKind.Utc), "A bowl of hot water\nA towel", false, false, "Pour hot (not boiling) water into a bowl.\nLean over it at a safe distance with a towel over your head.\nBreathe slowly for 5 to 10 minutes.\nKeep children away from hot water.", "Steam for a stuffy nose", "Breathe" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "seed", "Skin", new DateTime(2026, 10, 1, 12, 2, 0, 0, DateTimeKind.Utc), "1 cup plain oats, ground into a fine powder\nA tub of lukewarm water", false, false, "Stir the oat powder into lukewarm bathwater.\nSoak for 15 to 20 minutes.\nPat skin dry and moisturize.", "Oatmeal bath for itchy skin", "Skin" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_Category",
                table: "Posts",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CreatedUtc",
                table: "Posts",
                column: "CreatedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_PostId_DeviceId",
                table: "Reports",
                columns: new[] { "PostId", "DeviceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votes_PostId_DeviceId",
                table: "Votes",
                columns: new[] { "PostId", "DeviceId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "Votes");

            migrationBuilder.DropTable(
                name: "Posts");
        }
    }
}
