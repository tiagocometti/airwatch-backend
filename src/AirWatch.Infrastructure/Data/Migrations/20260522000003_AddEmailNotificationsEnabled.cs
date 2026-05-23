using AirWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirWatch.Infrastructure.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260522000003_AddEmailNotificationsEnabled")]
    public class AddEmailNotificationsEnabled : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE users
                    ADD COLUMN email_notifications_enabled boolean NOT NULL DEFAULT TRUE;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty — destructive rollback must be handled manually.
        }
    }
}
