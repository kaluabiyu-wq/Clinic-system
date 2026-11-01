using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Updateschedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Schedules_DoctorId",
                table: "Schedules");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Schedules_EndAfterStart",
                table: "Schedules");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Schedules_SlotMinutes",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleExceptions_DoctorId_Date",
                table: "ScheduleExceptions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ScheduleExceptions_Window",
                table: "ScheduleExceptions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "ScheduleExceptions");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "ScheduleExceptions");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Schedules",
                newName: "OpenTime");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "Schedules",
                newName: "CloseTime");

            migrationBuilder.AddColumn<int>(
                name: "DoctorId1",
                table: "Schedules",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DoctorId1",
                table: "ScheduleExceptions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_DoctorId1",
                table: "Schedules",
                column: "DoctorId1",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Schedules_CloseAfterOpen",
                table: "Schedules",
                sql: "\"CloseTime\" > \"OpenTime\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Schedules_SlotMinutesPositive",
                table: "Schedules",
                sql: "\"SlotMinutes\" > 0");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleExceptions_DoctorId_Date",
                table: "ScheduleExceptions",
                columns: new[] { "DoctorId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleExceptions_DoctorId1",
                table: "ScheduleExceptions",
                column: "DoctorId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleExceptions_Doctors_DoctorId1",
                table: "ScheduleExceptions",
                column: "DoctorId1",
                principalTable: "Doctors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Doctors_DoctorId1",
                table: "Schedules",
                column: "DoctorId1",
                principalTable: "Doctors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleExceptions_Doctors_DoctorId1",
                table: "ScheduleExceptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Doctors_DoctorId1",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_DoctorId1",
                table: "Schedules");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Schedules_CloseAfterOpen",
                table: "Schedules");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Schedules_SlotMinutesPositive",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleExceptions_DoctorId_Date",
                table: "ScheduleExceptions");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleExceptions_DoctorId1",
                table: "ScheduleExceptions");

            migrationBuilder.DropColumn(
                name: "DoctorId1",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "DoctorId1",
                table: "ScheduleExceptions");

            migrationBuilder.RenameColumn(
                name: "OpenTime",
                table: "Schedules",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "CloseTime",
                table: "Schedules",
                newName: "EndTime");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Schedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "ScheduleExceptions",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "ScheduleExceptions",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_DoctorId",
                table: "Schedules",
                column: "DoctorId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Schedules_EndAfterStart",
                table: "Schedules",
                sql: "\"EndTime\" > \"StartTime\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Schedules_SlotMinutes",
                table: "Schedules",
                sql: "\"SlotMinutes\" > 0");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleExceptions_DoctorId_Date",
                table: "ScheduleExceptions",
                columns: new[] { "DoctorId", "Date" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ScheduleExceptions_Window",
                table: "ScheduleExceptions",
                sql: "(\"StartTime\" IS NULL AND \"EndTime\" IS NULL) OR (\"StartTime\" IS NOT NULL AND \"EndTime\" IS NOT NULL AND \"EndTime\" > \"StartTime\")");
        }
    }
}
