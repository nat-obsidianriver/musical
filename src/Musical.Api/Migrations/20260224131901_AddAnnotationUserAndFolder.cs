using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Musical.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnotationUserAndFolder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "Annotations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Annotations",
                type: "TEXT",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_FolderId",
                table: "Annotations",
                column: "FolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Annotations_Folders_FolderId",
                table: "Annotations",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Annotations_Folders_FolderId",
                table: "Annotations");

            migrationBuilder.DropIndex(
                name: "IX_Annotations_FolderId",
                table: "Annotations");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "Annotations");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Annotations");
        }
    }
}
