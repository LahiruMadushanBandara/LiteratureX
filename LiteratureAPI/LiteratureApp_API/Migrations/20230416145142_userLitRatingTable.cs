using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiteratureApp_API.Migrations
{
    public partial class userLitRatingTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfileLiteratureRating",
                columns: table => new
                {
                    ProfileLiteratureRatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfileId = table.Column<int>(type: "int", nullable: false),
                    ProfileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfileImageName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LiteratureId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileLiteratureRating", x => x.ProfileLiteratureRatingId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileLiteratureRating");
        }
    }
}
