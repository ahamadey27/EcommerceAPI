using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixCartItemProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_ShoppingCarts_ShoppingCartId",
                table: "CartItems");

            migrationBuilder.RenameColumn(
                name: "ShoppingCartId",
                table: "CartItems",
                newName: "CartId");

            migrationBuilder.RenameColumn(
                name: "AddedAt",
                table: "CartItems",
                newName: "UpdatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_ShoppingCartId_ProductId",
                table: "CartItems",
                newName: "IX_CartItems_CartId_ProductId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CartItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_ShoppingCarts_CartId",
                table: "CartItems",
                column: "CartId",
                principalTable: "ShoppingCarts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_ShoppingCarts_CartId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CartItems");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "CartItems",
                newName: "AddedAt");

            migrationBuilder.RenameColumn(
                name: "CartId",
                table: "CartItems",
                newName: "ShoppingCartId");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_CartId_ProductId",
                table: "CartItems",
                newName: "IX_CartItems_ShoppingCartId_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_ShoppingCarts_ShoppingCartId",
                table: "CartItems",
                column: "ShoppingCartId",
                principalTable: "ShoppingCarts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
