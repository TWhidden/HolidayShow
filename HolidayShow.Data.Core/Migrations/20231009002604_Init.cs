using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HolidayShow.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AudioOptions",
                columns: table => new
                {
                    AudioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AudioDuration = table.Column<int>(type: "int", nullable: false),
                    IsNotVisable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioOptions", x => x.AudioId);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValueSql: "('NONAME')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.DeviceId);
                });

            migrationBuilder.CreateTable(
                name: "EffectInstructionsAvailable",
                columns: table => new
                {
                    EffectInstructionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InstructionName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InstructionsForUse = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EffectInstructionsAvailable", x => x.EffectInstructionId);
                });

            migrationBuilder.CreateTable(
                name: "Sets",
                columns: table => new
                {
                    SetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SetName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sets", x => x.SetId);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    SettingName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ValueString = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    ValueDouble = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.SettingName);
                });

            migrationBuilder.CreateTable(
                name: "DeviceIoPorts",
                columns: table => new
                {
                    DeviceIoPortId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    CommandPin = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValueSql: "('')"),
                    IsNotVisable = table.Column<bool>(type: "bit", nullable: false),
                    IsDanger = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceIoPorts", x => x.DeviceIoPortId);
                    table.ForeignKey(
                        name: "FK_DeviceIoPorts_Devices",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DevicePatterns",
                columns: table => new
                {
                    DevicePatternId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    PatternName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePatterns", x => x.DevicePatternId);
                    table.ForeignKey(
                        name: "FK_DevicePatterns_Devices",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceEffects",
                columns: table => new
                {
                    EffectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EffectName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InstructionMetaData = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    EffectInstructionId = table.Column<int>(type: "int", nullable: false),
                    TimeOn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeOff = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceEffects", x => x.EffectId);
                    table.ForeignKey(
                        name: "FK_DeviceEffects_EffectInstructionsAvailable",
                        column: x => x.EffectInstructionId,
                        principalTable: "EffectInstructionsAvailable",
                        principalColumn: "EffectInstructionId");
                });

            migrationBuilder.CreateTable(
                name: "DevicePatternSequences",
                columns: table => new
                {
                    DevicePatternSeqenceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DevicePatternId = table.Column<int>(type: "int", nullable: false),
                    OnAt = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    AudioId = table.Column<int>(type: "int", nullable: false),
                    DeviceIoPortId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePatternSequences", x => x.DevicePatternSeqenceId);
                    table.ForeignKey(
                        name: "FK_DevicePatternSequences_AudioOptions1",
                        column: x => x.AudioId,
                        principalTable: "AudioOptions",
                        principalColumn: "AudioId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DevicePatternSequences_DeviceIoPorts",
                        column: x => x.DeviceIoPortId,
                        principalTable: "DeviceIoPorts",
                        principalColumn: "DeviceIoPortId");
                    table.ForeignKey(
                        name: "FK_DevicePatternSequences_DevicePatterns",
                        column: x => x.DevicePatternId,
                        principalTable: "DevicePatterns",
                        principalColumn: "DevicePatternId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SetSequences",
                columns: table => new
                {
                    SetSequenceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SetId = table.Column<int>(type: "int", nullable: false),
                    OnAt = table.Column<int>(type: "int", nullable: false),
                    DevicePatternId = table.Column<int>(type: "int", nullable: true),
                    EffectId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetSequences", x => x.SetSequenceId);
                    table.ForeignKey(
                        name: "FK_SetSequences_DeviceEffects",
                        column: x => x.EffectId,
                        principalTable: "DeviceEffects",
                        principalColumn: "EffectId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SetSequences_DevicePatterns",
                        column: x => x.DevicePatternId,
                        principalTable: "DevicePatterns",
                        principalColumn: "DevicePatternId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SetSequences_Sets",
                        column: x => x.SetId,
                        principalTable: "Sets",
                        principalColumn: "SetId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEffects_EffectInstructionId",
                table: "DeviceEffects",
                column: "EffectInstructionId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceIoPorts_DeviceId",
                table: "DeviceIoPorts",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DevicePatterns_DeviceId",
                table: "DevicePatterns",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DevicePatternSequences_AudioId",
                table: "DevicePatternSequences",
                column: "AudioId");

            migrationBuilder.CreateIndex(
                name: "IX_DevicePatternSequences_DeviceIoPortId",
                table: "DevicePatternSequences",
                column: "DeviceIoPortId");

            migrationBuilder.CreateIndex(
                name: "IX_DevicePatternSequences_DevicePatternId",
                table: "DevicePatternSequences",
                column: "DevicePatternId");

            migrationBuilder.CreateIndex(
                name: "IX_SetSequences_DevicePatternId",
                table: "SetSequences",
                column: "DevicePatternId");

            migrationBuilder.CreateIndex(
                name: "IX_SetSequences_EffectId",
                table: "SetSequences",
                column: "EffectId");

            migrationBuilder.CreateIndex(
                name: "IX_SetSequences_SetId",
                table: "SetSequences",
                column: "SetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DevicePatternSequences");

            migrationBuilder.DropTable(
                name: "SetSequences");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "AudioOptions");

            migrationBuilder.DropTable(
                name: "DeviceIoPorts");

            migrationBuilder.DropTable(
                name: "DeviceEffects");

            migrationBuilder.DropTable(
                name: "DevicePatterns");

            migrationBuilder.DropTable(
                name: "Sets");

            migrationBuilder.DropTable(
                name: "EffectInstructionsAvailable");

            migrationBuilder.DropTable(
                name: "Devices");
        }
    }
}
