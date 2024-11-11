using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPSLocator.Migrations
{
	/// <inheritdoc />
	public partial class DefaultDB : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "LocationInfo",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Formatted_Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Locality = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Postcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Region = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Cross_Street = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_LocationInfo", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Users",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
					PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
					ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Favourite = table.Column<string>(type: "nvarchar(max)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Users", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Locations",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Fsq_Id = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Categories = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Closed_Bucket = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Distance = table.Column<int>(type: "int", nullable: true),
					Link = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LocationInfoId = table.Column<int>(type: "int", nullable: false),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Timezone = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Request = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Locations", x => x.Id);
					table.ForeignKey(
						name: "FK_Locations_LocationInfo_LocationInfoId",
						column: x => x.LocationInfoId,
						principalTable: "LocationInfo",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Locations_LocationInfoId",
				table: "Locations",
				column: "LocationInfoId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Locations");

			migrationBuilder.DropTable(
				name: "Users");

			migrationBuilder.DropTable(
				name: "LocationInfo");
		}
	}
}
