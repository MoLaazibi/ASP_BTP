using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AP.BTP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class finalMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StreetName = table.Column<string>(type: "varchar(100)", nullable: false),
                    HouseNumber = table.Column<string>(type: "varchar(10)", nullable: false),
                    PostalCode = table.Column<string>(type: "varchar(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(30)", nullable: false),
                    Color = table.Column<string>(type: "varchar(20)", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroundPlan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileUrl = table.Column<string>(type: "varchar(255)", nullable: false),
                    UploadTime = table.Column<DateTime>(type: "datetime2(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroundPlan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TreeType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    Description = table.Column<string>(type: "varchar(255)", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    QrCodeUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreeType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Instruction",
                columns: table => new
                {
                    instructieId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    boomSoortId = table.Column<int>(type: "int", nullable: false),
                    seizoen = table.Column<string>(type: "varchar(20)", nullable: false),
                    bestandUrl = table.Column<string>(type: "varchar(255)", nullable: false),
                    uploadTijd = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instruction", x => x.instructieId);
                    table.ForeignKey(
                        name: "FK_Instruction_TreeType_boomSoortId",
                        column: x => x.boomSoortId,
                        principalTable: "TreeType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TreeImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TreeTypeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreeImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreeImages_TreeType_TreeTypeId",
                        column: x => x.TreeTypeId,
                        principalTable: "TreeType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeTask",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    PlannedDuration = table.Column<int>(type: "int", nullable: false),
                    PlannedStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StopTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    TaskListId = table.Column<int>(type: "int", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeTask", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NurserySite",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    AddressId = table.Column<int>(type: "int", nullable: false),
                    GroundPlanId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NurserySite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NurserySite_Address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Address",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NurserySite_GroundPlan_GroundPlanId",
                        column: x => x.GroundPlanId,
                        principalTable: "GroundPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "varchar(100)", nullable: false),
                    LastName = table.Column<string>(type: "varchar(100)", nullable: false),
                    PreferredNurserySiteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserList_NurserySite_PreferredNurserySiteId",
                        column: x => x.PreferredNurserySiteId,
                        principalTable: "NurserySite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Zone",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(10)", nullable: false),
                    Size = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    NurserySiteId = table.Column<int>(type: "int", nullable: false),
                    TreeTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zone", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zone_NurserySite_NurserySiteId",
                        column: x => x.NurserySiteId,
                        principalTable: "NurserySite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Zone_TreeType_TreeTypeId",
                        column: x => x.TreeTypeId,
                        principalTable: "TreeType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_UserList_UserId",
                        column: x => x.UserId,
                        principalTable: "UserList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    ZoneId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskList_UserList_UserId",
                        column: x => x.UserId,
                        principalTable: "UserList",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskList_Zone_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zone",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Address",
                columns: new[] { "Id", "HouseNumber", "PostalCode", "StreetName" },
                values: new object[] { 1, "A22", "2140", "teststraat" });

            migrationBuilder.InsertData(
                table: "Category",
                columns: new[] { "Id", "Color", "Name" },
                values: new object[] { 1, "#111999", "Watergeven" });

            migrationBuilder.InsertData(
                table: "GroundPlan",
                columns: new[] { "Id", "FileUrl", "UploadTime" },
                values: new object[] { 1, "/test/file", new DateTime(2025, 12, 20, 0, 0, 0, 0, DateTimeKind.Local) });

            migrationBuilder.InsertData(
                table: "TaskList",
                columns: new[] { "Id", "Date", "UserId", "ZoneId" },
                values: new object[] { 1, new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null });

            migrationBuilder.InsertData(
                table: "TreeType",
                columns: new[] { "Id", "Description", "Name", "QrCodeUrl" },
                values: new object[] { 1, "Testbeschrijving", "Haagbeuk", null });

            migrationBuilder.InsertData(
                table: "UserList",
                columns: new[] { "Id", "AuthId", "Email", "FirstName", "LastName", "PreferredNurserySiteId", "Username" },
                values: new object[,]
                {
                    { 1, "6910ac6f6749b5a63269405d", "testadmin@test.com", "FirstAdmin", "LastAdmin", null, "TestAdmin" },
                    { 2, "6910b5346749b5a6326942b8", "testmedewerker@test.com", "FirstMedewerker", "LastMedewerker", null, "TestMedewerker" }
                });

            migrationBuilder.InsertData(
                table: "EmployeeTask",
                columns: new[] { "Id", "CategoryId", "Description", "Order", "PlannedDuration", "PlannedStartTime", "StartTime", "StopTime", "TaskListId" },
                values: new object[,]
                {
                    { 1, 1, "Dit is taak 1", 1, 2, new DateTime(2025, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 1, 11, 30, 0, 0, DateTimeKind.Utc), 1 },
                    { 2, 1, "Dit is taak 2", 2, 1, new DateTime(2025, 10, 1, 13, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 1, 13, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 1, 14, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 3, 1, "Dit is taak 3", 3, 3, new DateTime(2025, 10, 1, 14, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 1, 14, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 1, 18, 0, 0, 0, DateTimeKind.Utc), 1 }
                });

            migrationBuilder.InsertData(
                table: "NurserySite",
                columns: new[] { "Id", "AddressId", "GroundPlanId", "Name", "UserId" },
                values: new object[] { 1, 1, 1, "Site 1", 2 });

            migrationBuilder.InsertData(
                table: "TaskList",
                columns: new[] { "Id", "Date", "UserId", "ZoneId" },
                values: new object[] { 3, new DateTime(2025, 11, 12, 0, 0, 0, 0, DateTimeKind.Utc), 2, null });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "Role", "UserId" },
                values: new object[,]
                {
                    { 1, 3, 1 },
                    { 2, 1, 2 }
                });

            migrationBuilder.InsertData(
                table: "EmployeeTask",
                columns: new[] { "Id", "CategoryId", "Description", "Order", "PlannedDuration", "PlannedStartTime", "StartTime", "StopTime", "TaskListId" },
                values: new object[] { 7, 1, "Dit is taak 7", 4, 1, new DateTime(2025, 11, 12, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 11, 12, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 11, 12, 9, 30, 0, 0, DateTimeKind.Utc), 3 });

            migrationBuilder.InsertData(
                table: "Zone",
                columns: new[] { "Id", "Code", "IsDeleted", "NurserySiteId", "Size", "TreeTypeId" },
                values: new object[] { 1, "A1", false, 1, 20m, 1 });

            migrationBuilder.InsertData(
                table: "TaskList",
                columns: new[] { "Id", "Date", "UserId", "ZoneId" },
                values: new object[] { 2, new DateTime(2025, 11, 11, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1 });

            migrationBuilder.InsertData(
                table: "EmployeeTask",
                columns: new[] { "Id", "CategoryId", "Description", "Order", "PlannedDuration", "PlannedStartTime", "StartTime", "StopTime", "TaskListId" },
                values: new object[,]
                {
                    { 4, 1, "Dit is taak 4", 1, 1, new DateTime(2025, 11, 11, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 11, 11, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 11, 11, 10, 30, 0, 0, DateTimeKind.Utc), 2 },
                    { 5, 1, "Dit is taak 5", 2, 1, new DateTime(2025, 11, 11, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 11, 11, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 11, 11, 12, 30, 0, 0, DateTimeKind.Utc), 2 },
                    { 6, 1, "Dit is taak 6", 3, 1, new DateTime(2025, 11, 11, 13, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 11, 11, 13, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 11, 11, 14, 0, 0, 0, DateTimeKind.Utc), 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Address_Id",
                table: "Address",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Category_Id",
                table: "Category",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTask_StartTime",
                table: "EmployeeTask",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTask_StopTime",
                table: "EmployeeTask",
                column: "StopTime");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTask_TaskListId",
                table: "EmployeeTask",
                column: "TaskListId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTask_TaskListId_Order",
                table: "EmployeeTask",
                columns: new[] { "TaskListId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_GroundPlan_Id",
                table: "GroundPlan",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instruction_boomSoortId",
                table: "Instruction",
                column: "boomSoortId");

            migrationBuilder.CreateIndex(
                name: "IX_NurserySite_AddressId",
                table: "NurserySite",
                column: "AddressId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NurserySite_GroundPlanId",
                table: "NurserySite",
                column: "GroundPlanId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NurserySite_Id",
                table: "NurserySite",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NurserySite_UserId",
                table: "NurserySite",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskList_Date",
                table: "TaskList",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_TaskList_UserId",
                table: "TaskList",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskList_ZoneId",
                table: "TaskList",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_TreeImages_TreeTypeId",
                table: "TreeImages",
                column: "TreeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TreeType_Id",
                table: "TreeType",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserList_PreferredNurserySiteId",
                table: "UserList",
                column: "PreferredNurserySiteId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Zone_Id",
                table: "Zone",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Zone_NurserySiteId",
                table: "Zone",
                column: "NurserySiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Zone_TreeTypeId",
                table: "Zone",
                column: "TreeTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTask_TaskList_TaskListId",
                table: "EmployeeTask",
                column: "TaskListId",
                principalTable: "TaskList",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_NurserySite_UserList_UserId",
                table: "NurserySite",
                column: "UserId",
                principalTable: "UserList",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NurserySite_Address_AddressId",
                table: "NurserySite");

            migrationBuilder.DropForeignKey(
                name: "FK_NurserySite_GroundPlan_GroundPlanId",
                table: "NurserySite");

            migrationBuilder.DropForeignKey(
                name: "FK_NurserySite_UserList_UserId",
                table: "NurserySite");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "EmployeeTask");

            migrationBuilder.DropTable(
                name: "Instruction");

            migrationBuilder.DropTable(
                name: "TreeImages");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "TaskList");

            migrationBuilder.DropTable(
                name: "Zone");

            migrationBuilder.DropTable(
                name: "TreeType");

            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "GroundPlan");

            migrationBuilder.DropTable(
                name: "UserList");

            migrationBuilder.DropTable(
                name: "NurserySite");
        }
    }
}
