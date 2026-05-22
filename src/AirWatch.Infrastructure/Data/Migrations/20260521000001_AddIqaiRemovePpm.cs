using AirWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirWatch.Infrastructure.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260521000001_AddIqaiRemovePpm")]
    public class AddIqaiRemovePpm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE measurements
                    DROP COLUMN IF EXISTS "PpmAlcohol",
                    DROP COLUMN IF EXISTS "PpmLpg",
                    DROP COLUMN IF EXISTS "PpmCo2",
                    DROP COLUMN IF EXISTS "PpmNh3",
                    ADD COLUMN IF NOT EXISTS "RatioMq3"     double precision NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS "RatioMq5"     double precision NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS "RatioMq135"   double precision NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS "DegMq3"       double precision NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS "DegMq5"       double precision NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS "DegMq135"     double precision NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS "Iqai"         double precision NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS "IqaiCategory" varchar(20) NOT NULL DEFAULT 'Boa';

                DROP TABLE IF EXISTS sensor_coefficients;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty — destructive rollback must be handled manually.
        }
    }
}
