using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirWatch.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Applied manually via psql.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty.
        }
    }
}
