using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Musical.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnotationRangeAndAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentFileName",
                table: "Annotations",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PositionXEnd",
                table: "Annotations",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PositionYEnd",
                table: "Annotations",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentFileName",
                table: "Annotations");

            migrationBuilder.DropColumn(
                name: "PositionXEnd",
                table: "Annotations");

            migrationBuilder.DropColumn(
                name: "PositionYEnd",
                table: "Annotations");
        }
    }
}
