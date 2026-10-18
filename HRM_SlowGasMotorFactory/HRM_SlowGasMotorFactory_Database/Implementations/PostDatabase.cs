using Dapper;
using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;
using HRM_SlowGasMotorFactory_Database.Migrations;
using HRM_SlowGasMotorFactory_Database.SchemaDto;
using HRM_SlowGasMotorFactory_DataModels.DataModels;
using Microsoft.Extensions.Logging;

namespace HRM_SlowGasMotorFactory_Database.Implementations;

public class PostDatabase : IPostDatabase
{
    private readonly ManualMigration _manualMigration;
    private readonly ILogger<IPostDatabase> _logger;

    public PostDatabase(ManualMigration manualMigration, ILogger<IPostDatabase> logger)
    {
        _manualMigration = manualMigration;
        _logger = logger;
    }

    public bool Create(Post post)
    {
        try
        {
            using var connection = _manualMigration.Connection;
            var query = $@"
                INSERT INTO {PostTableSchema.TableName}
                ({PostTableSchema.ColumnaIdName}, {PostTableSchema.ColumnaPostNameName}, 
                 {PostTableSchema.ColumnaPostTypeName}, {PostTableSchema.ColumnaSalaryName})
                VALUES (@Id, @PostName, @PostType, @SalaryPerHour)";
            return connection.Execute(query, post) > 0;
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
            var query = $"DELETE FROM {PostTableSchema.TableName} WHERE {PostTableSchema.ColumnaIdName} = @Id";
            return connection.Execute(query, new { Id = id }) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete failed");
            return false;
        }
    }

    public List<Post> Read()
    {
        try
        {
            using var connection = _manualMigration.Connection;
            var query = $"SELECT * FROM {PostTableSchema.TableName}";
            return connection.Query<Post>(query).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Read failed");
            return new List<Post>();
        }
    }

    public Post? Read(Guid id)
    {
        try
        {
            using var connection = _manualMigration.Connection;
            var query = $"SELECT * FROM {PostTableSchema.TableName} WHERE {PostTableSchema.ColumnaIdName} = @Id";
            return connection.QueryFirstOrDefault<Post>(query, new { Id = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Read single failed");
            return null;
        }
    }

    public bool Update(Post post)
    {
        try
        {
            using var connection = _manualMigration.Connection;
            var query = $@"
                UPDATE {PostTableSchema.TableName}
                SET {PostTableSchema.ColumnaPostNameName} = @PostName,
                    {PostTableSchema.ColumnaPostTypeName} = @PostType,
                    {PostTableSchema.ColumnaSalaryName} = @SalaryPerHour
                WHERE {PostTableSchema.ColumnaIdName} = @Id";
            return connection.Execute(query, post) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update failed");
            return false;
        }
    }
}