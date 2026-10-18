using Dapper;
using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces.Database;
using HRM_SlowGasMotorFactory_Database.Migrations;
using HRM_SlowGasMotorFactory_Database.SchemaDto;
using HRM_SlowGasMotorFactory_DataModels.DataModels;
using Microsoft.Extensions.Logging;

namespace HRM_SlowGasMotorFactory_Database.Implementations;

public class WorkerSalaryDatabase : IWorkerSalaryDatabase
{
    private readonly ManualMigration _manualMigration;
    private readonly ILogger<WorkerSalaryDatabase> _logger;

    public WorkerSalaryDatabase(ManualMigration manualMigration, ILogger<WorkerSalaryDatabase> logger)
    {
        _manualMigration = manualMigration;
        _logger = logger;
    }

    public List<WorkerSalary> Read()
    {
        try
        {
            using var connection = _manualMigration.Connection;
            var query = $@"
                SELECT ws.*, w.*
                FROM {WorkerSalaryTableSchema.TableName} ws
                INNER JOIN {WorkerTableSchema.TableName} w ON ws.{WorkerSalaryTableSchema.ColumnaWorkerIdName} = w.{WorkerTableSchema.ColumnaIdName};";

            var result = connection.Query<dynamic>(query).ToList();

            return result.Select(row => new WorkerSalary
            {
                Id = row.id,
                WorkerId = row.workerid,
                Salary = row.salary,
                CalculateDate = DateTime.Parse(row.calculatedate.ToString()),
                Worker = new Worker
                {
                    Id = row.workerid,
                    PersonalNumber = row.personalnumber,
                    FIO = row.fio
                }
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Read");
            return new List<WorkerSalary>();
        }
    }

    public bool Create(WorkerSalary workerSalary)
    {
        try
        {
            using var connection = _manualMigration.Connection;
            var query = $@"
                INSERT INTO {WorkerSalaryTableSchema.TableName}
                ({WorkerSalaryTableSchema.ColumnaIdName},
                 {WorkerSalaryTableSchema.ColumnaWorkerIdName},
                 {WorkerSalaryTableSchema.ColumnaSalaryName},
                 {WorkerSalaryTableSchema.ColumnaCalculateDateName})
                VALUES
                (@Id, @WorkerId, @Salary, @CalculateDate);";

            return connection.Execute(query, workerSalary) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create");
            return false;
        }
    }
}