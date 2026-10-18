using Dapper;
using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;
using HRM_SlowGasMotorFactory_Database.Migrations;
using HRM_SlowGasMotorFactory_Database.SchemaDto;
using HRM_SlowGasMotorFactory_DataModels.DataModels;
using Microsoft.Extensions.Logging;

namespace HRM_SlowGasMotorFactory_Database.Implementations;

public class WorkerDatabase : IWorkerDatabase
{
    private readonly ManualMigration _manualMigration;
    private readonly ILogger<IWorkerDatabase> _logger;

    public WorkerDatabase(ManualMigration manualMigration, ILogger<IWorkerDatabase> logger)
    {
        _manualMigration = manualMigration;
        _logger = logger;
    }

    public bool Create(Worker worker)
    {
        try
        {
            using var connection = _manualMigration.Connection;
            var query = $@"
                INSERT INTO {WorkerTableSchema.TableName}
                ({WorkerTableSchema.ColumnaIdName}, {WorkerTableSchema.ColumnaPersonalNumberName},
                 {WorkerTableSchema.ColumnaPostIdName}, {WorkerTableSchema.ColumnaFIOName},
                 {WorkerTableSchema.ColumnaBirthDateName}, {WorkerTableSchema.ColumnaEmploymentDateName})
                VALUES (@Id, @PersonalNumber, @PostId, @FIO, @BirthDate, @EmploymentDate)";
            return connection.Execute(query, worker) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create failed");
            return false;
        }
    }

    public bool Delete(Guid id)
    {
        try
        {
            using var connection = _manualMigration.Connection;
            var query = $@"
                UPDATE {WorkerTableSchema.TableName}
                SET {WorkerTableSchema.ColumnaIsDeletedName} = TRUE
                WHERE {WorkerTableSchema.ColumnaIdName} = @Id";
            return connection.Execute(query, new { Id = id }) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete failed");
            return false;
        }
    }

    public List<Worker> Read()
    {
        try
        {
            using var connection = _manualMigration.Connection;
            var query = $@"
                SELECT w.*, p.*
                FROM {WorkerTableSchema.TableName} w
                JOIN {PostTableSchema.TableName} p ON w.{WorkerTableSchema.ColumnaPostIdName} = p.{PostTableSchema.ColumnaIdName}
                WHERE w.{WorkerTableSchema.ColumnaIsDeletedName} = FALSE";

            var result = connection.Query<Worker, Post, Worker>(
                query,
                (worker, post) =>
                {
                    worker.Post = post;
                    return worker;
                },
                splitOn: $"{PostTableSchema.ColumnaIdName}"
            ).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Read failed");
            return new List<Worker>();
        }
    }

    public Worker? Read(Guid id)
    {
        try
        {
            using var connection = _manualMigration.Connection;
            var query = $@"
                SELECT w.*, p.*
                FROM {WorkerTableSchema.TableName} w
                JOIN {PostTableSchema.TableName} p ON w.{WorkerTableSchema.ColumnaPostIdName} = p.{PostTableSchema.ColumnaIdName}
                WHERE w.{WorkerTableSchema.ColumnaIdName} = @Id
                  AND w.{WorkerTableSchema.ColumnaIsDeletedName} = FALSE";

            var result = connection.Query<Worker, Post, Worker>(
                query,
                (worker, post) =>
                {
                    worker.Post = post;
                    return worker;
                },
                new { Id = id },
                splitOn: $"{PostTableSchema.ColumnaIdName}"
            ).FirstOrDefault();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Read single failed");
            return null;
        }
    }

    public List<Post> ReadWorkerPreviousPosts(Guid workerId)
    {
        try
        {
            using var connection = _manualMigration.Connection;
            var query = $@"
                SELECT DISTINCT p.*
                FROM {WorkerTableSchema.TableName} w
                JOIN {PostTableSchema.TableName} p ON 
                    w.{WorkerTableSchema.ColumnaPostIdName} = p.{PostTableSchema.ColumnaIdName} OR
                    w.{WorkerTableSchema.ColumnaPostBeforeIdName} = p.{PostTableSchema.ColumnaIdName} OR
                    w.{WorkerTableSchema.ColumnaPostBeforeBeforeIdName} = p.{PostTableSchema.ColumnaIdName}
                WHERE w.{WorkerTableSchema.ColumnaIdName} = @WorkerId
                  AND w.{WorkerTableSchema.ColumnaIsDeletedName} = FALSE";

            return connection.Query<Post>(query, new { WorkerId = workerId }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ReadWorkerPreviousPosts failed");
            return new List<Post>();
        }
    }

    public bool Update(Worker worker)
    {
        try
        {
            using var connection = _manualMigration.Connection;
            using var transaction = connection.BeginTransaction();

            // Получаем текущего сотрудника
            var oldWorker = connection.QueryFirstOrDefault<Worker>(
                $"SELECT * FROM {WorkerTableSchema.TableName} WHERE {WorkerTableSchema.ColumnaIdName} = @Id",
                new { worker.Id }, transaction);

            if (oldWorker == null)
            {
                _logger.LogWarning("Update failed: Worker {Id} not found", worker.Id);
                return false;
            }

            if (oldWorker.IsDeleted)
            {
                _logger.LogWarning("Update failed: Worker {Id} is deleted", worker.Id);
                return false;
            }

            // Если должность изменилась, обновляем историю
            if (oldWorker.PostId != worker.PostId)
            {
                // Сохраняем старую должность в PostBeforeId, а предыдущую в PostBeforeBeforeId
                var updateHistoryQuery = $@"
                    UPDATE {WorkerTableSchema.TableName}
                    SET 
                        {WorkerTableSchema.ColumnaPostBeforeIdName} = @OldPostId,
                        {WorkerTableSchema.ColumnaPostBeforeBeforeIdName} = (SELECT {WorkerTableSchema.ColumnaPostBeforeIdName} FROM {WorkerTableSchema.TableName} WHERE {WorkerTableSchema.ColumnaIdName} = @Id)
                    WHERE {WorkerTableSchema.ColumnaIdName} = @Id";

                connection.Execute(updateHistoryQuery,
                    new { worker.Id, OldPostId = oldWorker.PostId }, transaction);
            }

            // Обновляем основные поля
            var query = $@"
                UPDATE {WorkerTableSchema.TableName}
                SET 
                    {WorkerTableSchema.ColumnaPersonalNumberName} = @PersonalNumber,
                    {WorkerTableSchema.ColumnaPostIdName} = @PostId,
                    {WorkerTableSchema.ColumnaFIOName} = @FIO,
                    {WorkerTableSchema.ColumnaBirthDateName} = @BirthDate,
                    {WorkerTableSchema.ColumnaEmploymentDateName} = @EmploymentDate
                WHERE {WorkerTableSchema.ColumnaIdName} = @Id
                  AND {WorkerTableSchema.ColumnaIsDeletedName} = FALSE";

            var result = connection.Execute(query, worker, transaction) > 0;
            transaction.Commit();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update failed");
            return false;
        }
    }
}