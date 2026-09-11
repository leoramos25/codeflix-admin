using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codeflix.Catalog.Infra.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class CreateGenreTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .CreateTable(
                    name: "Genres",
                    columns: table => new
                    {
                        Id = table.Column<Guid>(
                            type: "char(36)",
                            nullable: false,
                            collation: "ascii_general_ci"
                        ),
                        Name = table
                            .Column<string>(type: "longtext", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                        CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Genres", x => x.Id);
                    }
                )
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "GenresCategories",
                    columns: table => new
                    {
                        GenreId = table.Column<Guid>(
                            type: "char(36)",
                            nullable: false,
                            collation: "ascii_general_ci"
                        ),
                        CategoryId = table.Column<Guid>(
                            type: "char(36)",
                            nullable: false,
                            collation: "ascii_general_ci"
                        ),
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey(
                            "PK_GenresCategories",
                            x => new { x.GenreId, x.CategoryId }
                        );
                        table.ForeignKey(
                            name: "FK_GenresCategories_Categories_CategoryId",
                            column: x => x.CategoryId,
                            principalTable: "Categories",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade
                        );
                        table.ForeignKey(
                            name: "FK_GenresCategories_Genres_GenreId",
                            column: x => x.GenreId,
                            principalTable: "Genres",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade
                        );
                    }
                )
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GenresCategories_CategoryId",
                table: "GenresCategories",
                column: "CategoryId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GenresCategories");

            migrationBuilder.DropTable(name: "Genres");
        }
    }
}
