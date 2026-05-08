using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirWatch.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RestructureMeasurementsAndAddSensorConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Recriar tabela measurements ────────────────────────────────────
            migrationBuilder.Sql("DROP TABLE IF EXISTS measurements;");

            migrationBuilder.Sql("""
                CREATE TABLE measurements (
                    "Id"         uuid                     NOT NULL PRIMARY KEY,
                    "DeviceId"   uuid                     NOT NULL REFERENCES devices("Id") ON DELETE CASCADE,
                    "Timestamp"  timestamp with time zone NOT NULL DEFAULT NOW(),
                    "Mq3Adc"     integer                  NOT NULL,
                    "Mq5Adc"     integer                  NOT NULL,
                    "Mq135Adc"   integer                  NOT NULL,
                    "PpmAlcohol" double precision          NOT NULL,
                    "PpmLpg"     double precision          NOT NULL,
                    "PpmCo2"     double precision          NOT NULL,
                    "PpmNh3"     double precision          NOT NULL
                );
                """);

            migrationBuilder.Sql("""
                CREATE INDEX "IX_measurements_DeviceId"  ON measurements ("DeviceId");
                CREATE INDEX "IX_measurements_Timestamp" ON measurements ("Timestamp" DESC);
                """);

            // ── 2. Adicionar colunas RL e R0 em devices ───────────────────────────
            migrationBuilder.Sql("""
                ALTER TABLE devices
                    ADD COLUMN IF NOT EXISTS "RlMq3"   double precision NOT NULL DEFAULT 10000.0,
                    ADD COLUMN IF NOT EXISTS "RlMq5"   double precision NOT NULL DEFAULT 10000.0,
                    ADD COLUMN IF NOT EXISTS "RlMq135" double precision NOT NULL DEFAULT 10000.0,
                    ADD COLUMN IF NOT EXISTS "R0Mq3"   double precision NOT NULL DEFAULT 25000.0,
                    ADD COLUMN IF NOT EXISTS "R0Mq5"   double precision NOT NULL DEFAULT 105000.0,
                    ADD COLUMN IF NOT EXISTS "R0Mq135" double precision NOT NULL DEFAULT 76630.0;
                """);

            // ── 3. Criar sensor_coefficients ──────────────────────────────────────
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS sensor_coefficients (
                    "Id"         uuid                     NOT NULL PRIMARY KEY,
                    "SensorType" character varying(10)    NOT NULL,
                    "GasTarget"  character varying(20)    NOT NULL,
                    "CoefA"      double precision          NOT NULL,
                    "CoefB"      double precision          NOT NULL,
                    "RatioMin"   double precision          NOT NULL,
                    "RatioMax"   double precision          NOT NULL,
                    CONSTRAINT "UQ_sensor_coefficients_SensorType_GasTarget"
                        UNIQUE ("SensorType", "GasTarget")
                );
                """);

            migrationBuilder.Sql("""
                INSERT INTO sensor_coefficients ("Id","SensorType","GasTarget","CoefA","CoefB","RatioMin","RatioMax") VALUES
                    (gen_random_uuid(), 'MQ3',   'Alcohol',   0.3934,  -1.5040, 0.1143, 2.4978),
                    (gen_random_uuid(), 'MQ5',   'LPG',     217.4972,  -2.4221, 0.2059, 1.0352),
                    (gen_random_uuid(), 'MQ135', 'CO2',     110.4700,  -2.8620, 0.7600, 2.4200),
                    (gen_random_uuid(), 'MQ135', 'NH3',     102.2000,  -2.4730, 0.3070, 1.7140)
                ON CONFLICT ("SensorType","GasTarget") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty — destructive rollback must be handled manually.
        }
    }
}
