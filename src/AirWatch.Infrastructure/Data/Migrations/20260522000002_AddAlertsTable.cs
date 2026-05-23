using AirWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirWatch.Infrastructure.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260522000002_AddAlertsTable")]
    public class AddAlertsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE alerts (
                    id           uuid                     NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
                    device_id    uuid                     NOT NULL REFERENCES devices(id) ON DELETE CASCADE,
                    user_id      uuid                     NOT NULL REFERENCES users(id)   ON DELETE CASCADE,
                    triggered_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    email_sent   boolean                  NOT NULL DEFAULT FALSE
                );

                CREATE INDEX ix_alerts_device_id    ON alerts(device_id);
                CREATE INDEX ix_alerts_triggered_at ON alerts(triggered_at);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty — destructive rollback must be handled manually.
        }
    }
}
