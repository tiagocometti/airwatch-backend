using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirWatch.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RestructureForArduinoPayload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Schema already applied manually via psql. Migration recorded for snapshot consistency.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty - manual schema changes are not reversible via EF.
        }
    }
}
