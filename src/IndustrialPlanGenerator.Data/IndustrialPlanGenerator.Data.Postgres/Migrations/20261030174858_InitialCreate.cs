using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndustrialPlanGenerator.Data.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "production_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    productive_process = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_lines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "shifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    schedule = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shifts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    credentials = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.CheckConstraint("ck_user_role", "role IN ('Administrator', 'Technician', 'Operator', 'PlantManager')");
                });

            migrationBuilder.CreateTable(
                name: "equipment_systems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    operative_state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    line_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment_systems", x => x.Id);
                    table.ForeignKey(
                        name: "fk_equipment_system_line",
                        column: x => x.line_id,
                        principalTable: "production_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "administrators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_administrators", x => x.Id);
                    table.ForeignKey(
                        name: "fk_administrator_user",
                        column: x => x.Id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "operators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    age = table.Column<int>(type: "integer", nullable: false),
                    shift_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operators", x => x.Id);
                    table.CheckConstraint("ck_operator_age", "age >= 18");
                    table.ForeignKey(
                        name: "fk_operator_shift",
                        column: x => x.shift_id,
                        principalTable: "shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_operator_user",
                        column: x => x.Id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plant_managers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plant_managers", x => x.Id);
                    table.ForeignKey(
                        name: "fk_plant_manager_user",
                        column: x => x.Id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "technicians",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    specialty = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_technicians", x => x.Id);
                    table.ForeignKey(
                        name: "fk_technician_user",
                        column: x => x.Id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sensors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sensors", x => x.Id);
                    table.CheckConstraint("ck_sensor_type", "type IN ('Temperature', 'Vibration', 'Pressure')");
                    table.ForeignKey(
                        name: "fk_sensor_equipment",
                        column: x => x.equipment_id,
                        principalTable: "equipment_systems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "operator_lines",
                columns: table => new
                {
                    operator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operator_lines", x => new { x.operator_id, x.line_id });
                    table.ForeignKey(
                        name: "fk_operator_line_line",
                        column: x => x.line_id,
                        principalTable: "production_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_operator_line_operator",
                        column: x => x.operator_id,
                        principalTable: "operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    technician_creator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creation_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    all_criteria_met = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintenance_plans", x => x.Id);
                    table.CheckConstraint("ck_maintenance_plan_status", "status IN ('Generated', 'Validated', 'Rejected')");
                    table.ForeignKey(
                        name: "fk_maintenance_plan_line",
                        column: x => x.line_id,
                        principalTable: "production_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_maintenance_plan_technician",
                        column: x => x.technician_creator_id,
                        principalTable: "technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    system_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criticality = table.Column<string>(type: "text", nullable: false),
                    technician_creator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creation_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintenance_tasks", x => x.Id);
                    table.CheckConstraint("ck_maintenance_task_criticality", "criticality IN ('Low', 'Medium', 'High')");
                    table.CheckConstraint("ck_maintenance_task_status", "status IN ('Active', 'Duplicated')");
                    table.CheckConstraint("ck_maintenance_task_type", "type IN ('Preventive', 'Corrective', 'Predictive')");
                    table.ForeignKey(
                        name: "fk_maintenance_task_equipment",
                        column: x => x.system_id,
                        principalTable: "equipment_systems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_maintenance_task_technician",
                        column: x => x.technician_creator_id,
                        principalTable: "technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "technician_lines",
                columns: table => new
                {
                    technician_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_technician_lines", x => new { x.technician_id, x.line_id });
                    table.CheckConstraint("ck_technician_line_permission", "permission IN ('Generate', 'Validate')");
                    table.ForeignKey(
                        name: "fk_technician_line_line",
                        column: x => x.line_id,
                        principalTable: "production_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_technician_line_technician",
                        column: x => x.technician_id,
                        principalTable: "technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plan_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "fk_plan_assignment_operator",
                        column: x => x.operator_id,
                        principalTable: "operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_plan_assignment_plan",
                        column: x => x.plan_id,
                        principalTable: "maintenance_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plan_parameters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    preventive_proportion = table.Column<double>(type: "numeric(5,2)", nullable: false),
                    corrective_proportion = table.Column<double>(type: "numeric(5,2)", nullable: false),
                    predictive_proportion = table.Column<double>(type: "numeric(5,2)", nullable: false),
                    total_tasks = table.Column<int>(type: "integer", nullable: false),
                    system_coverage = table.Column<double>(type: "numeric(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_parameters", x => x.Id);
                    table.CheckConstraint("ck_plan_parameters_corrective_proportion", "corrective_proportion >= 0 AND corrective_proportion <= 100");
                    table.CheckConstraint("ck_plan_parameters_predictive_proportion", "predictive_proportion >= 0 AND predictive_proportion <= 100");
                    table.CheckConstraint("ck_plan_parameters_preventive_proportion", "preventive_proportion >= 0 AND preventive_proportion <= 100");
                    table.CheckConstraint("ck_plan_parameters_system_coverage", "system_coverage >= 0 AND system_coverage <= 100");
                    table.ForeignKey(
                        name: "fk_plan_parameters_plan",
                        column: x => x.plan_id,
                        principalTable: "maintenance_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plan_validations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plant_manager_id = table.Column<Guid>(type: "uuid", nullable: false),
                    validation_date = table.Column<DateOnly>(type: "date", nullable: false),
                    observations = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    result = table.Column<string>(type: "text", nullable: false),
                    new_plan_required = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_validations", x => x.Id);
                    table.CheckConstraint("ck_plan_validation_result", "result IN ('Approved', 'Rejected')");
                    table.ForeignKey(
                        name: "fk_plan_validation_plan",
                        column: x => x.plan_id,
                        principalTable: "maintenance_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_plan_validation_plant_manager",
                        column: x => x.plant_manager_id,
                        principalTable: "plant_managers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "plan_tasks",
                columns: table => new
                {
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_tasks", x => new { x.plan_id, x.task_id });
                    table.ForeignKey(
                        name: "fk_plan_task_plan",
                        column: x => x.plan_id,
                        principalTable: "maintenance_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_plan_task_task",
                        column: x => x.task_id,
                        principalTable: "maintenance_tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "task_executions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    observations = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_executions", x => x.Id);
                    table.ForeignKey(
                        name: "fk_task_execution_assignment",
                        column: x => x.assignment_id,
                        principalTable: "plan_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_task_execution_task",
                        column: x => x.task_id,
                        principalTable: "maintenance_tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reinspection_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    execution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    support_technician_id = table.Column<Guid>(type: "uuid", nullable: true),
                    request_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    manual_result = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    result_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reinspection_requests", x => x.Id);
                    table.CheckConstraint("ck_reinspection_request_status", "status IN ('Pending', 'InProgress', 'Completed')");
                    table.ForeignKey(
                        name: "fk_reinspection_request_execution",
                        column: x => x.execution_id,
                        principalTable: "task_executions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_reinspection_request_operator",
                        column: x => x.operator_id,
                        principalTable: "operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reinspection_request_support_technician",
                        column: x => x.support_technician_id,
                        principalTable: "technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_equipment_system_line_id",
                table: "equipment_systems",
                column: "line_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_plan_line_id",
                table: "maintenance_plans",
                column: "line_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_plan_technician_creator_id",
                table: "maintenance_plans",
                column: "technician_creator_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_task_system_id",
                table: "maintenance_tasks",
                column: "system_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_task_technician_creator_id",
                table: "maintenance_tasks",
                column: "technician_creator_id");

            migrationBuilder.CreateIndex(
                name: "ux_maintenance_task_active_unique",
                table: "maintenance_tasks",
                columns: new[] { "description", "system_id", "type", "criticality" },
                unique: true,
                filter: "status = 'Active'");

            migrationBuilder.CreateIndex(
                name: "ix_operator_line_line_id",
                table: "operator_lines",
                column: "line_id");

            migrationBuilder.CreateIndex(
                name: "IX_operators_shift_id",
                table: "operators",
                column: "shift_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_assignment_operator_id",
                table: "plan_assignments",
                column: "operator_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_assignment_plan_id",
                table: "plan_assignments",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ux_plan_assignment_operator",
                table: "plan_assignments",
                columns: new[] { "plan_id", "operator_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_plan_parameters_plan_id",
                table: "plan_parameters",
                column: "plan_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plan_task_task_id",
                table: "plan_tasks",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_validation_plan_id",
                table: "plan_validations",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_validation_plant_manager_id",
                table: "plan_validations",
                column: "plant_manager_id");

            migrationBuilder.CreateIndex(
                name: "ix_reinspection_request_execution_id",
                table: "reinspection_requests",
                column: "execution_id");

            migrationBuilder.CreateIndex(
                name: "ix_reinspection_request_operator_id",
                table: "reinspection_requests",
                column: "operator_id");

            migrationBuilder.CreateIndex(
                name: "ix_reinspection_request_support_technician_id",
                table: "reinspection_requests",
                column: "support_technician_id");

            migrationBuilder.CreateIndex(
                name: "ix_sensor_equipment_id",
                table: "sensors",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "ux_shift_name",
                table: "shifts",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_shift_schedule",
                table: "shifts",
                column: "schedule",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_task_execution_assignment_id",
                table: "task_executions",
                column: "assignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_execution_task_id",
                table: "task_executions",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "ix_technician_line_line_id",
                table: "technician_lines",
                column: "line_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "administrators");

            migrationBuilder.DropTable(
                name: "operator_lines");

            migrationBuilder.DropTable(
                name: "plan_parameters");

            migrationBuilder.DropTable(
                name: "plan_tasks");

            migrationBuilder.DropTable(
                name: "plan_validations");

            migrationBuilder.DropTable(
                name: "reinspection_requests");

            migrationBuilder.DropTable(
                name: "sensors");

            migrationBuilder.DropTable(
                name: "technician_lines");

            migrationBuilder.DropTable(
                name: "plant_managers");

            migrationBuilder.DropTable(
                name: "task_executions");

            migrationBuilder.DropTable(
                name: "plan_assignments");

            migrationBuilder.DropTable(
                name: "maintenance_tasks");

            migrationBuilder.DropTable(
                name: "operators");

            migrationBuilder.DropTable(
                name: "maintenance_plans");

            migrationBuilder.DropTable(
                name: "equipment_systems");

            migrationBuilder.DropTable(
                name: "shifts");

            migrationBuilder.DropTable(
                name: "technicians");

            migrationBuilder.DropTable(
                name: "production_lines");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
