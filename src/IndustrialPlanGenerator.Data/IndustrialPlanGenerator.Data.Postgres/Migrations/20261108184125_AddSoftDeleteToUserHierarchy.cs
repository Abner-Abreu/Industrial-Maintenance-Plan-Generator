using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndustrialPlanGenerator.Data.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToUserHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_administrator_user",
                table: "administrators");

            migrationBuilder.DropForeignKey(
                name: "fk_operator_user",
                table: "operators");

            migrationBuilder.DropForeignKey(
                name: "fk_plant_manager_user",
                table: "plant_managers");

            migrationBuilder.DropForeignKey(
                name: "fk_technician_user",
                table: "technicians");

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "technicians",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "plant_managers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "operators",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "administrators",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "fk_administrator_user",
                table: "administrators",
                column: "Id",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_operator_user",
                table: "operators",
                column: "Id",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_plant_manager_user",
                table: "plant_managers",
                column: "Id",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_technician_user",
                table: "technicians",
                column: "Id",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_administrator_user",
                table: "administrators");

            migrationBuilder.DropForeignKey(
                name: "fk_operator_user",
                table: "operators");

            migrationBuilder.DropForeignKey(
                name: "fk_plant_manager_user",
                table: "plant_managers");

            migrationBuilder.DropForeignKey(
                name: "fk_technician_user",
                table: "technicians");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "technicians");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "plant_managers");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "operators");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "administrators");

            migrationBuilder.AddForeignKey(
                name: "fk_administrator_user",
                table: "administrators",
                column: "Id",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_operator_user",
                table: "operators",
                column: "Id",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_plant_manager_user",
                table: "plant_managers",
                column: "Id",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_technician_user",
                table: "technicians",
                column: "Id",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
