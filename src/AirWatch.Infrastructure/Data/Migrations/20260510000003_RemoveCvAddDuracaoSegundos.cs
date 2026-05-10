using AirWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirWatch.Infrastructure.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260510000003_RemoveCvAddDuracaoSegundos")]
    public class RemoveCvAddDuracaoSegundos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE calibrations
                    DROP COLUMN IF EXISTS "CvMq3",
                    DROP COLUMN IF EXISTS "CvMq5",
                    DROP COLUMN IF EXISTS "CvMq135",
                    ADD COLUMN IF NOT EXISTS "DuracaoSegundos" integer NOT NULL DEFAULT 300;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty — destructive rollback must be handled manually.
        }
    }
}
