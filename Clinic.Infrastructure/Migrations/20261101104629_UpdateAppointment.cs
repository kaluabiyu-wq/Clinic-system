using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentEvents_Appointments_AppointmentId",
                table: "AppointmentEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Doctors_DoctorId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_Date_DoctorId_Status",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_Reference",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "UX_Appointments_QueueNo",
                table: "Appointments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Appointments_QueueNoPositive",
                table: "Appointments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Appointments_SlotMatchesKind",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentEvents_AppointmentId_OccurredAtUtc",
                table: "AppointmentEvents");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentEvents_UserId",
                table: "AppointmentEvents");

            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Reference",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "StartAtUtc",
                table: "Appointments",
                newName: "StartAt");

            migrationBuilder.RenameColumn(
                name: "EndAtUtc",
                table: "Appointments",
                newName: "EndAt");

            migrationBuilder.RenameColumn(
                name: "CheckInAtUtc",
                table: "Appointments",
                newName: "CheckInAt");

            migrationBuilder.RenameIndex(
                name: "UX_Appointments_ActiveSlot",
                table: "Appointments",
                newName: "IX_Appointments_DoctorId_Date_SlotTime");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AppointmentEvents",
                newName: "ChangedByUserId");

            migrationBuilder.RenameColumn(
                name: "OccurredAtUtc",
                table: "AppointmentEvents",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Appointments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "SlotTime",
                table: "Appointments",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "QueueNo",
                table: "Appointments",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsWalkIn",
                table: "Appointments",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<int>(
                name: "DoctorId1",
                table: "Appointments",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ToStatus",
                table: "AppointmentEvents",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "AppointmentEvents",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FromStatus",
                table: "AppointmentEvents",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AppointmentEvents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Date_DoctorId",
                table: "Appointments",
                columns: new[] { "Date", "DoctorId" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId1",
                table: "Appointments",
                column: "DoctorId1");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentEvents_AppointmentId",
                table: "AppointmentEvents",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentEvents_Appointments_AppointmentId",
                table: "AppointmentEvents",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Doctors_DoctorId",
                table: "Appointments",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Doctors_DoctorId1",
                table: "Appointments",
                column: "DoctorId1",
                principalTable: "Doctors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentEvents_Appointments_AppointmentId",
                table: "AppointmentEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Doctors_DoctorId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Doctors_DoctorId1",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_Date_DoctorId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorId1",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentEvents_AppointmentId",
                table: "AppointmentEvents");

            migrationBuilder.DropColumn(
                name: "DoctorId1",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AppointmentEvents");

            migrationBuilder.RenameColumn(
                name: "StartAt",
                table: "Appointments",
                newName: "StartAtUtc");

            migrationBuilder.RenameColumn(
                name: "EndAt",
                table: "Appointments",
                newName: "EndAtUtc");

            migrationBuilder.RenameColumn(
                name: "CheckInAt",
                table: "Appointments",
                newName: "CheckInAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_DoctorId_Date_SlotTime",
                table: "Appointments",
                newName: "UX_Appointments_ActiveSlot");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "AppointmentEvents",
                newName: "OccurredAtUtc");

            migrationBuilder.RenameColumn(
                name: "ChangedByUserId",
                table: "AppointmentEvents",
                newName: "UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Appointments",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "SlotTime",
                table: "Appointments",
                type: "time without time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");

            migrationBuilder.AlterColumn<int>(
                name: "QueueNo",
                table: "Appointments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsWalkIn",
                table: "Appointments",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "Appointments",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "Appointments",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "Appointments",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Appointments",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<string>(
                name: "ToStatus",
                table: "AppointmentEvents",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "AppointmentEvents",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FromStatus",
                table: "AppointmentEvents",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Date_DoctorId_Status",
                table: "Appointments",
                columns: new[] { "Date", "DoctorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Reference",
                table: "Appointments",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Appointments_QueueNo",
                table: "Appointments",
                columns: new[] { "DoctorId", "Date", "QueueNo" },
                unique: true,
                filter: "\"QueueNo\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Appointments_QueueNoPositive",
                table: "Appointments",
                sql: "\"QueueNo\" IS NULL OR \"QueueNo\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Appointments_SlotMatchesKind",
                table: "Appointments",
                sql: "(\"IsWalkIn\" = TRUE AND \"SlotTime\" IS NULL) OR (\"IsWalkIn\" = FALSE AND \"SlotTime\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentEvents_AppointmentId_OccurredAtUtc",
                table: "AppointmentEvents",
                columns: new[] { "AppointmentId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentEvents_UserId",
                table: "AppointmentEvents",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentEvents_Appointments_AppointmentId",
                table: "AppointmentEvents",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Doctors_DoctorId",
                table: "Appointments",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
