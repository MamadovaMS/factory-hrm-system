using Dapper;
using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;
using HRM_SlowGasMotorFactory_Database.Implementations;
using HRM_SlowGasMotorFactory_Database.Migrations;
using HRM_SlowGasMotorFactory_Database.SchemaDto;
using HRM_SlowGasMotorFactory_DataModels.DataModels;
using HRM_SlowGasMotorFactory_DataModels.Enums;
using HRM_SlowGasMotorFactory_TestsMSTest.DatabaseTest;
using Microsoft.Extensions.Logging.Testing;
using System.Data;

namespace HRM_SlowGasMotorFactory_TestsMSTest.DatabaseTests;

[TestClass]
public sealed class WorkerSalaryDatabaseTests
{
    private static ManualMigration _manualMigration = null!;
    private WorkerSalaryDatabase _database = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
        _manualMigration = ManualMigrationCreator.Create();
        _manualMigration.ApplyMigrations();
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _manualMigration?.DropDatabase();
    }

    [TestInitialize]
    public void TestInit()
    {
        _database = new WorkerSalaryDatabase(_manualMigration, new DatabaseTest.FakeLogger<WorkerSalaryDatabase>());
    }

    [TestCleanup]
    public void TestCleanup()
    {
        using var connection = _manualMigration.Connection;
        connection.Execute($@"DELETE FROM {WorkerSalaryTableSchema.TableName};");
        connection.Execute($@"DELETE FROM {PassMarkTableSchema.TableName};");
        connection.Execute($@"DELETE FROM {WorkerTableSchema.TableName};");
        connection.Execute($@"DELETE FROM {PostTableSchema.TableName};");
    }

    [TestMethod]
    public void TestMethod_Create()
    {
        using var connection = _manualMigration.Connection;
        var workerId = InsertWorker(connection);
        var workerSalary = new WorkerSalary
        {
            Id = Guid.NewGuid(),
            WorkerId = workerId,
            Salary = 50000.50,
            CalculateDate = DateTime.UtcNow.AddDays(-30)
        };

        var result = _database.Create(workerSalary);
        var salaryFromDb = GetWorkerSalaryById(connection, workerSalary.Id);

        Assert.IsTrue(result);
        Assert.IsNotNull(salaryFromDb);
        Assert.AreEqual(workerSalary.Id, salaryFromDb.Id);
        Assert.AreEqual(workerSalary.WorkerId, salaryFromDb.WorkerId);
        Assert.AreEqual(workerSalary.Salary, salaryFromDb.Salary);
        Assert.AreEqual(workerSalary.CalculateDate.Date, salaryFromDb.CalculateDate.Date);
    }

    [TestMethod]
    public void TestMethod_Create_InvalidWorkerId()
    {
        var workerSalary = new WorkerSalary
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            Salary = 50000.50,
            CalculateDate = DateTime.UtcNow.AddDays(-30)
        };

        var result = _database.Create(workerSalary);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TestMethod_Read_All()
    {
        using var connection = _manualMigration.Connection;
        var workerId = InsertWorker(connection);

        InsertWorkerSalary(connection, workerId, 50000.50);
        InsertWorkerSalary(connection, workerId, 55000.75);
        InsertWorkerSalary(connection, workerId, 60000.00);

        var salaries = _database.Read();

        Assert.IsNotNull(salaries);
        Assert.AreEqual(3, salaries.Count);
    }

    [TestMethod]
    public void TestMethod_Read_Empty()
    {
        var salaries = _database.Read();
        Assert.IsNotNull(salaries);
        Assert.AreEqual(0, salaries.Count);
    }

    [TestMethod]
    public void TestMethod_Read_WithWorker()
    {
        using var connection = _manualMigration.Connection;
        var workerId = InsertWorker(connection);

        InsertWorkerSalary(connection, workerId, 50000.50);

        var salaries = _database.Read();

        Assert.IsNotNull(salaries);
        Assert.AreEqual(1, salaries.Count);

        var salary = salaries.First();
        Assert.IsNotNull(salary.Worker);
        Assert.AreEqual(workerId, salary.Worker.Id);
    }

    [TestMethod]
    public void TestMethod_Create_DuplicateId()
    {
        using var connection = _manualMigration.Connection;
        var workerId = InsertWorker(connection);
        var existingId = InsertWorkerSalary(connection, workerId, 50000.50);

        var duplicateSalary = new WorkerSalary
        {
            Id = existingId,
            WorkerId = workerId,
            Salary = 60000.00,
            CalculateDate = DateTime.UtcNow.AddDays(-30)
        };

        var result = _database.Create(duplicateSalary);

        Assert.IsFalse(result);
    }

    // ==================== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ====================

    private static Guid InsertWorker(IDbConnection connection)
    {
        var postId = InsertPost(connection);
        var workerId = Guid.NewGuid();

        var query = $@"
            INSERT INTO {WorkerTableSchema.TableName}
            ({WorkerTableSchema.ColumnaIdName},
             {WorkerTableSchema.ColumnaPersonalNumberName},
             {WorkerTableSchema.ColumnaPostIdName},
             {WorkerTableSchema.ColumnaFIOName},
             {WorkerTableSchema.ColumnaBirthDateName},
             {WorkerTableSchema.ColumnaEmploymentDateName})
            VALUES
            (@Id, @PersonalNumber, @PostId, @FIO, @BirthDate, @EmploymentDate);";

        connection.Execute(query, new Worker
        {
            Id = workerId,
            PersonalNumber = "1234567890",
            PostId = postId,
            FIO = "Тестовый сотрудник",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            EmploymentDate = DateTime.UtcNow.AddDays(-100),
            IsDeleted = false
        });

        return workerId;
    }

    private static Guid InsertPost(IDbConnection connection)
    {
        var postId = Guid.NewGuid();
        var query = $@"
            INSERT INTO {PostTableSchema.TableName}
            ({PostTableSchema.ColumnaIdName},
             {PostTableSchema.ColumnaPostNameName},
             {PostTableSchema.ColumnaPostTypeName},
             {PostTableSchema.ColumnaSalaryName})
            VALUES
            (@Id, @PostName, @PostType, @SalaryPerHour);";

        connection.Execute(query, new Post
        {
            Id = postId,
            PostName = "Инженер завода",
            PostType = PostType.Engineer,
            SalaryPerHour = 500
        });

        return postId;
    }

    private static Guid InsertWorkerSalary(IDbConnection connection, Guid workerId, double salary)
    {
        var id = Guid.NewGuid();
        var query = $@"
            INSERT INTO {WorkerSalaryTableSchema.TableName}
            ({WorkerSalaryTableSchema.ColumnaIdName},
             {WorkerSalaryTableSchema.ColumnaWorkerIdName},
             {WorkerSalaryTableSchema.ColumnaSalaryName},
             {WorkerSalaryTableSchema.ColumnaCalculateDateName})
            VALUES
            (@Id, @WorkerId, @Salary, @CalculateDate);";

        connection.Execute(query, new WorkerSalary
        {
            Id = id,
            WorkerId = workerId,
            Salary = salary,
            CalculateDate = DateTime.UtcNow.AddDays(-30)
        });

        return id;
    }

    private static WorkerSalary? GetWorkerSalaryById(IDbConnection connection, Guid id)
    {
        var query = $@"SELECT * FROM {WorkerSalaryTableSchema.TableName} WHERE Id = @Id;";
        var row = connection.QueryFirstOrDefault<dynamic>(query, new { Id = id });

        if (row == null) return null;

        return new WorkerSalary
        {
            Id = row.id,
            WorkerId = row.workerid,
            Salary = row.salary,
            CalculateDate = DateTime.Parse(row.calculatedate.ToString())
        };
    }
}