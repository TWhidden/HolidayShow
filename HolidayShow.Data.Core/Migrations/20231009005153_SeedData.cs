using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HolidayShow.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AudioOptions",
                columns: new[] { "AudioId", "AudioDuration", "FileName", "IsNotVisable", "Name" },
                values: new object[] { 1, 0, "", true, "NONE" });

            migrationBuilder.InsertData(
                table: "EffectInstructionsAvailable",
                columns: new[] { "EffectInstructionId", "DisplayName", "InstructionName", "InstructionsForUse", "IsDisabled" },
                values: new object[,]
                {
                    { 1, "GPIO Random", "GPIO_RANDOM", "Set the GPIO ports that participate in this. MetaData to look like DEVPINS=4:1,4:2;DUR=75;  DUR is the amount of time the GPIO is ON", false },
                    { 2, "GPIO Strobe", "GPIO_STROBE", "Strobes at a regular rate for the length of the current set or the effect duration. Set the effect duration to 0 for set duration. DEVPINS=4:1,4:2;DUR=75;  DUR is the lenght of time the GPIO will be ON", false },
                    { 3, "GPIO ON", "GPIO_STAY_ON", "On until duration is done, or set is over. DEVPINS=4:1,4:2,4:3,4:4,4:5,4:6,4:7,4:8;", false },
                    { 4, "GPIO_STROBE_DELAY", "GPIO_STROBE_DELAY", "Strobes the lights, but has a programmed delay after a period of time. MetaData to look like DEVPINS=4:1,4:2;DUR=75;DELAYBETWEEN=10000;EXECUTEFOR=3000;  DUR is the amount of time the GPIO is ON, DELAYBETWEEN is the amount of time everything is off before restarting, EXECUTEFOR is the amount of time the strobe will run for", false },
                    { 5, "GPIO Sequential", "GPIO_SEQUENTIAL", "Sequential processing of the GPIO pins, with the programmed delay, and optional \"Reverase\" for the next pass to go the other direction; DEVPINS=4:1,4:2;DUR=75;REVERSE=1;", false },
                    { 6, "GPIO Random Delay", "GPIO_RANDOM_DELAY", "Randomly execute selected pins for a period of time, then have a delay; DEVPINS=4:1,4:2;DUR=75;DELAYBETWEEN=10000;EXECUTEFOR=2000;", false }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AudioOptions",
                keyColumn: "AudioId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EffectInstructionsAvailable",
                keyColumn: "EffectInstructionId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EffectInstructionsAvailable",
                keyColumn: "EffectInstructionId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "EffectInstructionsAvailable",
                keyColumn: "EffectInstructionId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "EffectInstructionsAvailable",
                keyColumn: "EffectInstructionId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "EffectInstructionsAvailable",
                keyColumn: "EffectInstructionId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "EffectInstructionsAvailable",
                keyColumn: "EffectInstructionId",
                keyValue: 6);
        }
    }
}
