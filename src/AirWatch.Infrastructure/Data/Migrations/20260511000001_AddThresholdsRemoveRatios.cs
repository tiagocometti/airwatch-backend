using AirWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirWatch.Infrastructure.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260511000001_AddThresholdsRemoveRatios")]
    public class AddThresholdsRemoveRatios : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE sensor_coefficients
                    DROP COLUMN IF EXISTS "RatioMin",
                    DROP COLUMN IF EXISTS "RatioMax",
                    ADD COLUMN IF NOT EXISTS "SafeMax"  double precision NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS "GoodMax"  double precision NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS "AlertMax" double precision NOT NULL DEFAULT 0;

                UPDATE sensor_coefficients SET "SafeMax" = 100,  "GoodMax" = 500,  "AlertMax" = 1000 WHERE "GasTarget" = 'Alcohol';
                UPDATE sensor_coefficients SET "SafeMax" = 300,  "GoodMax" = 1000, "AlertMax" = 2100 WHERE "GasTarget" = 'LPG';
                UPDATE sensor_coefficients SET "SafeMax" = 700,  "GoodMax" = 1000, "AlertMax" = 2000 WHERE "GasTarget" = 'CO2';
                UPDATE sensor_coefficients SET "SafeMax" = 5,    "GoodMax" = 25,   "AlertMax" = 50   WHERE "GasTarget" = 'NH3';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty — destructive rollback must be handled manually.
        }
    }
}
