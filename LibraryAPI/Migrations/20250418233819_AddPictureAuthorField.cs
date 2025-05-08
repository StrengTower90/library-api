﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPictureAuthorField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Picture",
                table: "Authors",
                type: "varchar(max)",
                unicode: false,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Picture",
                table: "Authors");
        }
    }
}
