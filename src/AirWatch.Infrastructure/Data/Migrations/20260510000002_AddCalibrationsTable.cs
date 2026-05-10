using AirWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirWatch.Infrastructure.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260510000002_AddCalibrationsTable")]
    public class AddCalibrationsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE calibrations (
                    "Id"          uuid                     NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
                    "DeviceId"    uuid                     NOT NULL REFERENCES devices("Id") ON DELETE CASCADE,
                    "StartedAt"   timestamp with time zone NOT NULL DEFAULT NOW(),
                    "CompletedAt" timestamp with time zone,
                    "Status"      character varying(20)    NOT NULL DEFAULT 'InProgress',
                    "Location"    character varying(255)   NOT NULL,
                    "R0Mq3"       double precision,
                    "R0Mq5"       double precision,
                    "R0Mq135"     double precision,
                    "SampleCount" integer                  NOT NULL DEFAULT 0,
                    "CvMq3"       double precision,
                    "CvMq5"       double precision,
                    "CvMq135"     double precision,
                    "IsActive"    boolean                  NOT NULL DEFAULT FALSE
                );
                """);

            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX "uq_one_active_per_device"
                    ON calibrations("DeviceId")
                    WHERE "IsActive" = TRUE;

                CREATE INDEX "ix_calibrations_device_id" ON calibrations("DeviceId");
                CREATE INDEX "ix_calibrations_status"    ON calibrations("Status");
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty — destructive rollback must be handled manually.
        }
    }
}
