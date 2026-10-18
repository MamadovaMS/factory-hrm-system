using Dapper;
using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;
using HRM_SlowGasMotorFactory_Database.Migrations;
using HRM_SlowGasMotorFactory_Database.SchemaDto;
using HRM_SlowGasMotorFactory_DataModels.DataModels;
using Microsoft.Extensions.Logging;

namespace HRM_SlowGasMotorFactory_Database.Implementations;

public class PassMarksDatabase : IPassMarksDatabase
{
    private readonly ManualMigration _manualMigration;
    private readonly ILogger<IPassMarksDatabase> _logger;

    public PassMarksDatabase(ManualMigration manualMigration, ILogger<IPassMarksDatabase> logger)
    {
        _manualMigration = manualMigration;
        _logger = logger;
    }

    public bool Create(PassMarks passMark)
    {
        try
        {
            using var connection = _manualMigration.Connection;

            var workerExists = connection.QueryFirstOrDefault<int?>($@"
                SELECT 1 FROM {WorkerTableSchema.TableName} 
                WHERE {WorkerTableSchema.ColumnaIdName} = @WorkerId",
                new { passMark.WorkerId });

            if (workerExists == null)
            {
                _logger.LogWarning("Create failed: WorkerId {WorkerId} not found", passMark.WorkerId);
                return false;
            }

            var passExists = connection.QueryFirstOrDefault<int?>($@"
                SELECT 1 FROM {PassMarkTableSchema.TableName} 
                WHERE {PassMarkTableSchema.ColumnaPassNumberName} = @PassNumber",
                new { passMark.PassNumber });

            if (passExists != null)
            {
                _logger.LogWarning("Create failed: PassNumber {PassNumber} already exists", passMark.PassNumber);
                return false;
            }

            var query = $@"
                INSERT INTO {PassMarkTableSchema.TableName}
                ({PassMarkTableSchema.ColumnaIdName},
                 {PassMarkTableSchema.ColumnaWorkerIdName},
                 {PassMarkTableSchema.ColumnaPassNumberName},
                 {PassMarkTableSchema.ColumnaMarkDateTimeInName},
                 {PassMarkTableSchema.ColumnaMarkDateTimeOutName})
                VALUES
                (@Id, @WorkerId, @PassNumber, @MarkDateTimeIn, @MarkDateTimeOut)";

            return connection.Execute(query, passMark) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create failed");
            return false;
        }
    }

    public List<PassMarks> Read()
    {
        try
        {
            using var connection = _manualMigration.Connection;

            var query = $@"
                SELECT pm.*, w.*, p.*
                FROM {PassMarkTableSchema.TableName} pm
                JOIN {WorkerTableSchema.TableName} w ON pm.{PassMarkTableSchema.ColumnaWorkerIdName} = w.{WorkerTableSchema.ColumnaIdName}
                JOIN {PostTableSchema.TableName} p ON w.{WorkerTableSchema.ColumnaPostIdName} = p.{PostTableSchema.ColumnaIdName}";

            var result = connection.Query<PassMarks, Worker, Post, PassMarks>(
                query,
                (passMark, worker, post) =>
                {
                    passMark.Worker = worker;
                    passMark.Worker.Post = post;
                    return passMark;
                },
                splitOn: $"{WorkerTableSchema.ColumnaIdName},{PostTableSchema.ColumnaIdName}"
            ).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Read failed");
            return new List<PassMarks>();
        }
    }

    public bool Update(PassMarks passMark)
    {
        try
        {
            using var connection = _manualMigration.Connection;

            var exists = connection.QueryFirstOrDefault<int?>($@"
                SELECT 1 FROM {PassMarkTableSchema.TableName} 
                WHERE {PassMarkTableSchema.ColumnaIdName} = @Id",
                new { passMark.Id });

            if (exists == null)
            {
                _logger.LogWarning("Update failed: Id {Id} not found", passMark.Id);
                return false;
            }

            var query = $@"
                UPDATE {PassMarkTableSchema.TableName}
                SET {PassMarkTableSchema.ColumnaMarkDateTimeOutName} = @MarkDateTimeOut
                WHERE {PassMarkTableSchema.ColumnaIdName} = @Id";

            return connection.Execute(query, passMark) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update failed");
            return false;
        }
    }

    public bool Delete(Guid id)
    {
        try
        {
            using var connection = _manualMigration.Connection;
            var query = $"DELETE FROM {PassMarkTableSchema.TableName} WHERE {PassMarkTableSchema.ColumnaIdName} = @Id";
            return connection.Execute(query, new { Id = id }) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete failed");
            return false;
        }
    }
}