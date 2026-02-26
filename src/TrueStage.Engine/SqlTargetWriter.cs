using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using TrueStage.Core.Interfaces;
using TrueStage.Core.Models;

namespace TrueStage.Engine;

public class SqlTargetWriter : ITargetWriter
{
    private readonly string _connectionString;

    public SqlTargetWriter(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task CreateIngestionJobAsync(IngestionContext context)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.ExecuteAsync(
            "INSERT INTO Ingestion_Log (job_id, cu_id, source_file, status, started_at) VALUES (@job_id, @cu_id, @source_file, 'IN_PROGRESS', @started_at)",
            new { job_id = context.JobId, cu_id = context.CuId, source_file = context.SourceFilePath, started_at = context.StartedAt });
    }

    public async Task CloseIngestionJobAsync(IngestionContext context, string status)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.ExecuteAsync(
            @"UPDATE Ingestion_Log SET 
                status = @status, 
                total_rows = @total, 
                success_rows = @success, 
                failed_rows = @failed,
                rows_new = @rows_new,
                rows_updated = @rows_updated,
                rows_unchanged = @rows_unchanged,
                completed_at = @completed
              WHERE job_id = @job_id",
            new
            {
                status,
                total = context.TotalRows,
                success = context.SuccessRows,
                failed = context.FailedRows,
                rows_new = context.RowsNew,
                rows_updated = context.RowsUpdated,
                rows_unchanged = context.RowsUnchanged,
                completed = DateTime.UtcNow,
                job_id = context.JobId
            });
    }

    public async Task WriteRowsAsync(IEnumerable<MappedRow> rows, IngestionContext context)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        foreach (var row in rows)
        {
            if (row.HasError) continue;

            row.Data.TryGetValue("source_member_id", out var memberId);

            // Check if record exists
            var existing = await conn.QueryFirstOrDefaultAsync<(string? hash, long? id)>(
                "SELECT record_hash, member_id FROM Members WHERE cu_id = @cu_id AND source_member_id = @source_member_id",
                new { cu_id = context.CuId, source_member_id = memberId?.ToString() });

            if (existing.id == null)
            {
                // INSERT new record
                await InsertMemberAsync(conn, row, context);
                context.RowsNew++;
            }
            else if (existing.hash != row.RecordHash)
            {
                // UPDATE changed record
                await UpdateMemberAsync(conn, row, context);
                context.RowsUpdated++;
            }
            else
            {
                // UNCHANGED — skip
                context.RowsUnchanged++;
            }
        }
    }

    private static async Task InsertMemberAsync(SqlConnection conn, MappedRow row, IngestionContext context)
    {
        row.Data.TryGetValue("source_member_id", out var srcId);
        row.Data.TryGetValue("first_name", out var firstName);
        row.Data.TryGetValue("last_name", out var lastName);
        row.Data.TryGetValue("date_of_birth", out var dob);
        row.Data.TryGetValue("email", out var email);
        row.Data.TryGetValue("phone", out var phone);
        row.Data.TryGetValue("member_status", out var status);
        row.Data.TryGetValue("account_balance", out var balance);
        row.Data.TryGetValue("branch_code", out var branch);
        row.Data.TryGetValue("as_of_date", out var asOfDate);

        await conn.ExecuteAsync(
            @"INSERT INTO Members 
                (cu_id, source_member_id, first_name, last_name, date_of_birth, email, phone, member_status, account_balance, branch_code, as_of_date, record_hash, adapter_version, source_file)
              VALUES
                (@cu_id, @src_id, @first, @last, @dob, @email, @phone, @status, @balance, @branch, @as_of_date, @hash, @version, @file)",
            new
            {
                cu_id = context.CuId,
                src_id = srcId?.ToString(),
                first = firstName?.ToString(),
                last = lastName?.ToString(),
                dob = dob is DateTime dt ? (object)dt : DBNull.Value,
                email = email?.ToString(),
                phone = phone?.ToString(),
                status = status?.ToString(),
                balance = balance is decimal d ? (object)d : (balance != null ? (object)Convert.ToDecimal(balance) : DBNull.Value),
                branch = branch?.ToString(),
                as_of_date = asOfDate is DateTime aod ? (object)aod : DBNull.Value,
                hash = row.RecordHash,
                version = context.Config.AdapterVersion,
                file = context.SourceFileName
            });
    }

    private static async Task UpdateMemberAsync(SqlConnection conn, MappedRow row, IngestionContext context)
    {
        row.Data.TryGetValue("source_member_id", out var srcId);
        row.Data.TryGetValue("first_name", out var firstName);
        row.Data.TryGetValue("last_name", out var lastName);
        row.Data.TryGetValue("date_of_birth", out var dob);
        row.Data.TryGetValue("email", out var email);
        row.Data.TryGetValue("phone", out var phone);
        row.Data.TryGetValue("member_status", out var status);
        row.Data.TryGetValue("account_balance", out var balance);
        row.Data.TryGetValue("branch_code", out var branch);
        row.Data.TryGetValue("as_of_date", out var asOfDate);

        await conn.ExecuteAsync(
            @"UPDATE Members SET
                first_name = @first, last_name = @last, date_of_birth = @dob, email = @email,
                phone = @phone, member_status = @status, account_balance = @balance,
                branch_code = @branch, as_of_date = @as_of_date,
                record_hash = @hash, ingested_at = GETUTCDATE(), source_file = @file
              WHERE cu_id = @cu_id AND source_member_id = @src_id",
            new
            {
                cu_id = context.CuId,
                src_id = srcId?.ToString(),
                first = firstName?.ToString(),
                last = lastName?.ToString(),
                dob = dob is DateTime dt ? (object)dt : DBNull.Value,
                email = email?.ToString(),
                phone = phone?.ToString(),
                status = status?.ToString(),
                balance = balance is decimal d ? (object)d : (balance != null ? (object)Convert.ToDecimal(balance) : DBNull.Value),
                branch = branch?.ToString(),
                as_of_date = asOfDate is DateTime aod ? (object)aod : DBNull.Value,
                hash = row.RecordHash,
                file = context.SourceFileName
            });
    }

    public async Task WriteErrorAsync(MappedRow row, IngestionContext context, string errorMessage)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.ExecuteAsync(
            "INSERT INTO Row_Error_Log (job_id, cu_id, row_number, raw_data, error_message) VALUES (@job_id, @cu_id, @row, @raw, @err)",
            new { job_id = context.JobId, cu_id = context.CuId, row = row.RowNumber, raw = row.RawData, err = errorMessage });
    }
}
