using AirWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirWatch.Infrastructure.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260522000001_RenameColumnsToSnakeCase")]
    public class RenameColumnsToSnakeCase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                -- users
                ALTER TABLE users RENAME COLUMN "Id"           TO id;
                ALTER TABLE users RENAME COLUMN "Name"         TO name;
                ALTER TABLE users RENAME COLUMN "Email"        TO email;
                ALTER TABLE users RENAME COLUMN "PasswordHash" TO password_hash;
                ALTER TABLE users RENAME COLUMN "CreatedAt"    TO created_at;

                -- devices
                ALTER TABLE devices RENAME COLUMN "Id"           TO id;
                ALTER TABLE devices RENAME COLUMN "UserId"       TO user_id;
                ALTER TABLE devices RENAME COLUMN "ExternalId"   TO external_id;
                ALTER TABLE devices RENAME COLUMN "Name"         TO name;
                ALTER TABLE devices RENAME COLUMN "Location"     TO location;
                ALTER TABLE devices RENAME COLUMN "IsActive"     TO is_active;
                ALTER TABLE devices RENAME COLUMN "IsOnline"     TO is_online;
                ALTER TABLE devices RENAME COLUMN "LastSeen"     TO last_seen;
                ALTER TABLE devices RENAME COLUMN "RegisteredAt" TO registered_at;
                ALTER TABLE devices RENAME COLUMN "RlMq3"        TO rl_mq3;
                ALTER TABLE devices RENAME COLUMN "RlMq5"        TO rl_mq5;
                ALTER TABLE devices RENAME COLUMN "RlMq135"      TO rl_mq135;

                -- measurements
                ALTER TABLE measurements RENAME COLUMN "Id"          TO id;
                ALTER TABLE measurements RENAME COLUMN "DeviceId"    TO device_id;
                ALTER TABLE measurements RENAME COLUMN "Timestamp"   TO timestamp;
                ALTER TABLE measurements RENAME COLUMN "Mq3Adc"      TO mq3_adc;
                ALTER TABLE measurements RENAME COLUMN "Mq5Adc"      TO mq5_adc;
                ALTER TABLE measurements RENAME COLUMN "Mq135Adc"    TO mq135_adc;
                ALTER TABLE measurements RENAME COLUMN "RatioMq3"    TO ratio_mq3;
                ALTER TABLE measurements RENAME COLUMN "RatioMq5"    TO ratio_mq5;
                ALTER TABLE measurements RENAME COLUMN "RatioMq135"  TO ratio_mq135;
                ALTER TABLE measurements RENAME COLUMN "DegMq3"      TO deg_mq3;
                ALTER TABLE measurements RENAME COLUMN "DegMq5"      TO deg_mq5;
                ALTER TABLE measurements RENAME COLUMN "DegMq135"    TO deg_mq135;
                ALTER TABLE measurements RENAME COLUMN "Iqai"        TO iqai;
                ALTER TABLE measurements RENAME COLUMN "IqaiCategory" TO iqai_category;

                -- calibrations
                ALTER TABLE calibrations RENAME COLUMN "Id"              TO id;
                ALTER TABLE calibrations RENAME COLUMN "DeviceId"        TO device_id;
                ALTER TABLE calibrations RENAME COLUMN "StartedAt"       TO started_at;
                ALTER TABLE calibrations RENAME COLUMN "CompletedAt"     TO completed_at;
                ALTER TABLE calibrations RENAME COLUMN "Status"          TO status;
                ALTER TABLE calibrations RENAME COLUMN "Location"        TO location;
                ALTER TABLE calibrations RENAME COLUMN "R0Mq3"          TO r0_mq3;
                ALTER TABLE calibrations RENAME COLUMN "R0Mq5"          TO r0_mq5;
                ALTER TABLE calibrations RENAME COLUMN "R0Mq135"        TO r0_mq135;
                ALTER TABLE calibrations RENAME COLUMN "SampleCount"     TO sample_count;
                ALTER TABLE calibrations RENAME COLUMN "DuracaoSegundos" TO duracao_segundos;
                ALTER TABLE calibrations RENAME COLUMN "IsActive"        TO is_active;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty — destructive rollback must be handled manually.
        }
    }
}
