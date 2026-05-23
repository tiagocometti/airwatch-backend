using AirWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirWatch.Infrastructure.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260522000004_RemoveDeviceLocation")]
    public class RemoveDeviceLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE devices DROP COLUMN location;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty — destructive rollback must be handled manually.
        }
    }
}
