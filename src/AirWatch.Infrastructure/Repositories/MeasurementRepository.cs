using AirWatch.Application.DTOs.Measurements;
using AirWatch.Application.Interfaces.Repositories;
using AirWatch.Domain.Entities;
using AirWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace AirWatch.Infrastructure.Repositories;

public class MeasurementRepository(AppDbContext context) : IMeasurementRepository
{
    public async Task AddAsync(Measurement measurement)
    {
        await context.Measurements.AddAsync(measurement);
        await context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Measurement> Items, int TotalCount)> GetByDeviceIdAsync(Guid deviceId, int page, int pageSize)
    {
        var query = context.Measurements
            .Where(m => m.DeviceId == deviceId)
            .OrderByDescending(m => m.Timestamp);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Device)
            .AsNoTracking()
            .ToListAsync();

        return (items, total);
    }

    public async Task<(IEnumerable<Measurement> Items, int TotalCount)> GetByPeriodAsync(
        Guid userId, DateTime from, DateTime to, int page, int pageSize)
    {
        var query = context.Measurements
            .Where(m => m.Device.UserId == userId && m.Timestamp >= from && m.Timestamp <= to)
            .OrderByDescending(m => m.Timestamp);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Device)
            .AsNoTracking()
            .ToListAsync();

        return (items, total);
    }

    public async Task<(IEnumerable<Measurement> Items, int TotalCount)> GetLatestAsync(Guid userId, int page, int pageSize)
    {
        var query = context.Measurements
            .Where(m => m.Device.UserId == userId)
            .OrderByDescending(m => m.Timestamp);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Device)
            .AsNoTracking()
            .ToListAsync();

        return (items, total);
    }

    public async Task<Dictionary<Guid, DateTime>> GetLatestTimestampsByDeviceIdsAsync(IEnumerable<Guid> deviceIds)
    {
        var ids = deviceIds.ToList();
        return await context.Measurements
            .Where(m => ids.Contains(m.DeviceId))
            .GroupBy(m => m.DeviceId)
            .Select(g => new { DeviceId = g.Key, Latest = g.Max(m => m.Timestamp) })
            .ToDictionaryAsync(x => x.DeviceId, x => x.Latest);
    }

    public async Task<IEnumerable<MeasurementHistoryDto>> GetHistoryAsync(
        Guid deviceId, DateTime from, DateTime to, string granularity)
    {
        if (granularity == "raw")
        {
            var items = await context.Measurements
                .Where(m => m.DeviceId == deviceId && m.Timestamp >= from && m.Timestamp <= to)
                .OrderBy(m => m.Timestamp)
                .AsNoTracking()
                .ToListAsync();

            return items.Select(m => new MeasurementHistoryDto(
                DateTime.SpecifyKind(m.Timestamp, DateTimeKind.Utc),
                m.Iqai,
                m.IqaiCategory));
        }

        var intervalSeconds = granularity switch
        {
            "1min"  => 60,
            "5min"  => 300,
            "1hour" => 3600,
            "6hour" => 21600,
            _       => throw new ArgumentException($"Granularidade inválida: {granularity}", nameof(granularity))
        };

        const string sql = """
            SELECT
                to_timestamp(floor(extract(epoch from timestamp) / $1) * $1) AS bucket,
                AVG(iqai)         AS iqai,
                (array_agg(iqai_category ORDER BY timestamp DESC))[1] AS category
            FROM measurements
            WHERE device_id = $2
              AND timestamp >= $3
              AND timestamp <= $4
            GROUP BY 1
            ORDER BY 1
            """;

        var results = new List<MeasurementHistoryDto>();
        await context.Database.OpenConnectionAsync();
        try
        {
            await using var cmd = (Npgsql.NpgsqlCommand)context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new Npgsql.NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer,     Value = intervalSeconds });
            cmd.Parameters.Add(new Npgsql.NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Uuid,        Value = deviceId });
            cmd.Parameters.Add(new Npgsql.NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.TimestampTz, Value = DateTime.SpecifyKind(from, DateTimeKind.Utc) });
            cmd.Parameters.Add(new Npgsql.NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.TimestampTz, Value = DateTime.SpecifyKind(to,   DateTimeKind.Utc) });

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new MeasurementHistoryDto(
                    DateTime.SpecifyKind(reader.GetDateTime(0), DateTimeKind.Utc),
                    reader.GetDouble(1),
                    reader.GetString(2)));
            }
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }

        return results;
    }
}
