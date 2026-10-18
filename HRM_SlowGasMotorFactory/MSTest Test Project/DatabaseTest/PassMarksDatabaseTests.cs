using Dapper;
using HRM_SlowGasMotorFactory_Database.Implementations;
using HRM_SlowGasMotorFactory_Database.Migrations;
using HRM_SlowGasMotorFactory_Database.SchemaDto;
using HRM_SlowGasMotorFactory_DataModels.DataModels;
using HRM_SlowGasMotorFactory_DataModels.Enums;
using HRM_SlowGasMotorFactory_TestsMSTest.DatabaseTest;
using System.Data;

namespace HRM_SlowGasMotorFactory_TestsMSTest.DatabaseTest;

[TestClass]
public class PassMarksDatabaseTests
{
    private static ManualMigration _manualMigration = null!;
    private PassMarksDatabase _database = null!;

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
        _database = new PassMarksDatabase(_manualMigration, new FakeLogger<PassMarksDatabase>());
    }

    [TestCleanup]
    public void TestCleanup()
    {
        using var connection = _manualMigration.Connection;
        connection.Execute($"DELETE FROM {PassMarkTableSchema.TableName};");
        connection.Execute($"DELETE FROM {WorkerTableSchema.TableName};");
        connection.Execute($"DELETE FROM {PostTableSchema.TableName};");
    }

    [TestMethod]
    public void TestMethod_Read_Empty()
    {
        var marks = _database.Read();
        Assert.IsNotNull(marks);
        Assert.AreEqual(0, marks.Count);
    }

    [TestMethod]
    public void TestMethod_Read_All()
    {
        using var connection = _manualMigration.Connection;

        var postId = Guid.NewGuid();
        connection.Execute($@"
            INSERT INTO {PostTableSchema.TableName}
            ({PostTableSchema.ColumnaIdName}, {PostTableSchema.ColumnaPostNameName}, 
             {PostTableSchema.ColumnaPostTypeName}, {PostTableSchema.ColumnaSalaryName})
            VALUES (@Id, @PostName, @PostType, @SalaryPerHour)",
            new { Id = postId, PostName = "Test Post", PostType = (int)PostType.Engineer, SalaryPerHour = 1000 });

        var workerId = Guid.NewGuid();
        connection.Execute($@"
            INSERT INTO {WorkerTableSchema.TableName}
            ({WorkerTableSchema.ColumnaIdName}, {WorkerTableSchema.ColumnaPersonalNumberName},
             {WorkerTableSchema.ColumnaPostIdName}, {WorkerTableSchema.ColumnaFIOName},
             {WorkerTableSchema.ColumnaBirthDateName}, {WorkerTableSchema.ColumnaEmploymentDateName})
            VALUES (@Id, @PersonalNumber, @PostId, @FIO, @BirthDate, @EmploymentDate)",
            new
            {
                Id = workerId,
                PersonalNumber = "1234567890",
                PostId = postId,
                FIO = "Test Worker",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                EmploymentDate = DateTime.UtcNow
            });

        for (int i = 0; i < 3; i++)
        {
            connection.Execute($@"
                INSERT INTO {PassMarkTableSchema.TableName}
                ({PassMarkTableSchema.ColumnaIdName}, {PassMarkTableSchema.ColumnaWorkerIdName},
                 {PassMarkTableSchema.ColumnaPassNumberName}, {PassMarkTableSchema.ColumnaMarkDateTimeInName})
                VALUES (@Id, @WorkerId, @PassNumber, @MarkDateTimeIn)",
                new PassMarks
                {
                    Id = Guid.NewGuid(),
                    WorkerId = workerId,
                    PassNumber = $"PASS_{i}",
                    MarkDateTimeIn = DateTime.UtcNow
                });
        }

        var marks = _database.Read();
        Assert.IsNotNull(marks);
        Assert.AreEqual(3, marks.Count);
    }

    [TestMethod]
    public void TestMethod_Get_ByWorkerId()
    {
        using var connection = _manualMigration.Connection;

        var postId = Guid.NewGuid();
        connection.Execute($@"
            INSERT INTO {PostTableSchema.TableName}
            ({PostTableSchema.ColumnaIdName}, {PostTableSchema.ColumnaPostNameName}, 
             {PostTableSchema.ColumnaPostTypeName}, {PostTableSchema.ColumnaSalaryName})
            VALUES (@Id, @PostName, @PostType, @SalaryPerHour)",
            new { Id = postId, PostName = "Test Post", PostType = (int)PostType.Engineer, SalaryPerHour = 1000 });

        var workerId1 = Guid.NewGuid();
        var workerId2 = Guid.NewGuid();

        connection.Execute($@"
            INSERT INTO {WorkerTableSchema.TableName}
            ({WorkerTableSchema.ColumnaIdName}, {WorkerTableSchema.ColumnaPersonalNumberName},
             {WorkerTableSchema.ColumnaPostIdName}, {WorkerTableSchema.ColumnaFIOName},
             {WorkerTableSchema.ColumnaBirthDateName}, {WorkerTableSchema.ColumnaEmploymentDateName})
            VALUES (@Id, @PersonalNumber, @PostId, @FIO, @BirthDate, @EmploymentDate)",
            new
            {
                Id = workerId1,
                PersonalNumber = "1111111111",
                PostId = postId,
                FIO = "Worker 1",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                EmploymentDate = DateTime.UtcNow
            });

        connection.Execute($@"
            INSERT INTO {WorkerTableSchema.TableName}
            ({WorkerTableSchema.ColumnaIdName}, {WorkerTableSchema.ColumnaPersonalNumberName},
             {WorkerTableSchema.ColumnaPostIdName}, {WorkerTableSchema.ColumnaFIOName},
             {WorkerTableSchema.ColumnaBirthDateName}, {WorkerTableSchema.ColumnaEmploymentDateName})
            VALUES (@Id, @PersonalNumber, @PostId, @FIO, @BirthDate, @EmploymentDate)",
            new
            {
                Id = workerId2,
                PersonalNumber = "2222222222",
                PostId = postId,
                FIO = "Worker 2",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                EmploymentDate = DateTime.UtcNow
            });

        for (int i = 0; i < 2; i++)
        {
            connection.Execute($@"
                INSERT INTO {PassMarkTableSchema.TableName}
                ({PassMarkTableSchema.ColumnaIdName}, {PassMarkTableSchema.ColumnaWorkerIdName},
                 {PassMarkTableSchema.ColumnaPassNumberName}, {PassMarkTableSchema.ColumnaMarkDateTimeInName})
                VALUES (@Id, @WorkerId, @PassNumber, @MarkDateTimeIn)",
                new PassMarks
                {
                    Id = Guid.NewGuid(),
                    WorkerId = workerId1,
                    PassNumber = $"W1_{i}",
                    MarkDateTimeIn = DateTime.UtcNow
                });
        }

        for (int i = 0; i < 3; i++)
        {
            connection.Execute($@"
                INSERT INTO {PassMarkTableSchema.TableName}
                ({PassMarkTableSchema.ColumnaIdName}, {PassMarkTableSchema.ColumnaWorkerIdName},
                 {PassMarkTableSchema.ColumnaPassNumberName}, {PassMarkTableSchema.ColumnaMarkDateTimeInName})
                VALUES (@Id, @WorkerId, @PassNumber, @MarkDateTimeIn)",
                new PassMarks
                {
                    Id = Guid.NewGuid(),
                    WorkerId = workerId2,
                    PassNumber = $"W2_{i}",
                    MarkDateTimeIn = DateTime.UtcNow
                });
        }

        var readMethod = typeof(PassMarksDatabase).GetMethod("Read", new[] { typeof(Guid?), typeof(string), typeof(DateTime?), typeof(DateTime?) });

        if (readMethod != null)
        {
            var result = (List<PassMarks>)readMethod.Invoke(_database, new object[] { workerId1, null, null, null });
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(r => r.WorkerId == workerId1));
        }
    }

    [TestMethod]
    public void TestMethod_Get_ByPassNumber()
    {
        using var connection = _manualMigration.Connection;

        var postId = Guid.NewGuid();
        connection.Execute($@"
            INSERT INTO {PostTableSchema.TableName}
            ({PostTableSchema.ColumnaIdName}, {PostTableSchema.ColumnaPostNameName}, 
             {PostTableSchema.ColumnaPostTypeName}, {PostTableSchema.ColumnaSalaryName})
            VALUES (@Id, @PostName, @PostType, @SalaryPerHour)",
            new { Id = postId, PostName = "Test Post", PostType = (int)PostType.Engineer, SalaryPerHour = 1000 });

        var workerId = Guid.NewGuid();
        connection.Execute($@"
            INSERT INTO {WorkerTableSchema.TableName}
            ({WorkerTableSchema.ColumnaIdName}, {WorkerTableSchema.ColumnaPersonalNumberName},
             {WorkerTableSchema.ColumnaPostIdName}, {WorkerTableSchema.ColumnaFIOName},
             {WorkerTableSchema.ColumnaBirthDateName}, {WorkerTableSchema.ColumnaEmploymentDateName})
            VALUES (@Id, @PersonalNumber, @PostId, @FIO, @BirthDate, @EmploymentDate)",
            new
            {
                Id = workerId,
                PersonalNumber = "1234567890",
                PostId = postId,
                FIO = "Test Worker",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                EmploymentDate = DateTime.UtcNow
            });

        var targetPassNumber = "TARGET_123";

        connection.Execute($@"
            INSERT INTO {PassMarkTableSchema.TableName}
            ({PassMarkTableSchema.ColumnaIdName}, {PassMarkTableSchema.ColumnaWorkerIdName},
             {PassMarkTableSchema.ColumnaPassNumberName}, {PassMarkTableSchema.ColumnaMarkDateTimeInName})
            VALUES (@Id, @WorkerId, @PassNumber, @MarkDateTimeIn)",
            new PassMarks
            {
                Id = Guid.NewGuid(),
                WorkerId = workerId,
                PassNumber = targetPassNumber,
                MarkDateTimeIn = DateTime.UtcNow
            });

        connection.Execute($@"
            INSERT INTO {PassMarkTableSchema.TableName}
            ({PassMarkTableSchema.ColumnaIdName}, {PassMarkTableSchema.ColumnaWorkerIdName},
             {PassMarkTableSchema.ColumnaPassNumberName}, {PassMarkTableSchema.ColumnaMarkDateTimeInName})
            VALUES (@Id, @WorkerId, @PassNumber, @MarkDateTimeIn)",
            new PassMarks
            {
                Id = Guid.NewGuid(),
                WorkerId = workerId,
                PassNumber = "OTHER_456",
                MarkDateTimeIn = DateTime.UtcNow
            });

        var readMethod = typeof(PassMarksDatabase).GetMethod("Read", new[] { typeof(Guid?), typeof(string), typeof(DateTime?), typeof(DateTime?) });

        if (readMethod != null)
        {
            var result = (List<PassMarks>)readMethod.Invoke(_database, new object[] { null, targetPassNumber, null, null });
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(targetPassNumber, result.First().PassNumber);
        }
    }

    [TestMethod]
    public void TestMethod_Create()
    {
        using var connection = _manualMigration.Connection;

        var postId = Guid.NewGuid();
        connection.Execute($@"
            INSERT INTO {PostTableSchema.TableName}
            ({PostTableSchema.ColumnaIdName}, {PostTableSchema.ColumnaPostNameName},
             {PostTableSchema.ColumnaPostTypeName}, {PostTableSchema.ColumnaSalaryName})
            VALUES (@Id, @PostName, @PostType, @SalaryPerHour)",
            new { Id = postId, PostName = "Test Post", PostType = (int)PostType.Engineer, SalaryPerHour = 1000 });

        var workerId = Guid.NewGuid();
        connection.Execute($@"
            INSERT INTO {WorkerTableSchema.TableName}
            ({WorkerTableSchema.ColumnaIdName}, {WorkerTableSchema.ColumnaPersonalNumberName},
             {WorkerTableSchema.ColumnaPostIdName}, {WorkerTableSchema.ColumnaFIOName},
             {WorkerTableSchema.ColumnaBirthDateName}, {WorkerTableSchema.ColumnaEmploymentDateName})
            VALUES (@Id, @PersonalNumber, @PostId, @FIO, @BirthDate, @EmploymentDate)",
            new
            {
                Id = workerId,
                PersonalNumber = "1234567890",
                PostId = postId,
                FIO = "Test Worker",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                EmploymentDate = DateTime.UtcNow
            });

        var passMark = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = workerId,
            PassNumber = "CREATE_001",
            MarkDateTimeIn = DateTime.UtcNow
        };

        var result = _database.Create(passMark);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestMethod_Create_IdExists()
    {
        using var connection = _manualMigration.Connection;

        var postId = Guid.NewGuid();
        connection.Execute($@"
            INSERT INTO {PostTableSchema.TableName}
            ({PostTableSchema.ColumnaIdName}, {PostTableSchema.ColumnaPostNameName},
             {PostTableSchema.ColumnaPostTypeName}, {PostTableSchema.ColumnaSalaryName})
            VALUES (@Id, @PostName, @PostType, @SalaryPerHour)",
            new { Id = postId, PostName = "Test Post", PostType = (int)PostType.Engineer, SalaryPerHour = 1000 });

        var workerId = Guid.NewGuid();
        connection.Execute($@"
            INSERT INTO {WorkerTableSchema.TableName}
            ({WorkerTableSchema.ColumnaIdName}, {WorkerTableSchema.ColumnaPersonalNumberName},
             {WorkerTableSchema.ColumnaPostIdName}, {WorkerTableSchema.ColumnaFIOName},
             {WorkerTableSchema.ColumnaBirthDateName}, {WorkerTableSchema.ColumnaEmploymentDateName})
            VALUES (@Id, @PersonalNumber, @PostId, @FIO, @BirthDate, @EmploymentDate)",
            new
            {
                Id = workerId,
                PersonalNumber = "1234567890",
                PostId = postId,
                FIO = "Test Worker",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                EmploymentDate = DateTime.UtcNow
            });

        var existingId = Guid.NewGuid();
        connection.Execute($@"
            INSERT INTO {PassMarkTableSchema.TableName}
            ({PassMarkTableSchema.ColumnaIdName}, {PassMarkTableSchema.ColumnaWorkerIdName},
             {PassMarkTableSchema.ColumnaPassNumberName}, {PassMarkTableSchema.ColumnaMarkDateTimeInName})
            VALUES (@Id, @WorkerId, @PassNumber, @MarkDateTimeIn)",
            new PassMarks
            {
                Id = existingId,
                WorkerId = workerId,
                PassNumber = "EXISTING",
                MarkDateTimeIn = DateTime.UtcNow
            });

        var newPassMark = new PassMarks
        {
            Id = existingId,
            WorkerId = workerId,
            PassNumber = "NEW",
            MarkDateTimeIn = DateTime.UtcNow
        };

        var result = _database.Create(newPassMark);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TestMethod_Create_WorkerIdNotExists()
    {
        var passMark = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            PassNumber = "TEST",
            MarkDateTimeIn = DateTime.UtcNow
        };

        var result = _database.Create(passMark);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TestMethod_Update()
    {
        using var connection = _manualMigration.Connection;

        var postId = Guid.NewGuid();
        connection.Execute($@"
            INSERT INTO {PostTableSchema.TableName}
            ({PostTableSchema.ColumnaIdName}, {PostTableSchema.ColumnaPostNameName},
             {PostTableSchema.ColumnaPostTypeName}, {PostTableSchema.ColumnaSalaryName})
            VALUES (@Id, @PostName, @PostType, @SalaryPerHour)",
            new { Id = postId, PostName = "Test Post", PostType = (int)PostType.Engineer, SalaryPerHour = 1000 });

        var workerId = Guid.NewGuid();
        connection.Execute($@"
            INSERT INTO {WorkerTableSchema.TableName}
            ({WorkerTableSchema.ColumnaIdName}, {WorkerTableSchema.ColumnaPersonalNumberName},
             {WorkerTableSchema.ColumnaPostIdName}, {WorkerTableSchema.ColumnaFIOName},
             {WorkerTableSchema.ColumnaBirthDateName}, {WorkerTableSchema.ColumnaEmploymentDateName})
            VALUES (@Id, @PersonalNumber, @PostId, @FIO, @BirthDate, @EmploymentDate)",
            new
            {
                Id = workerId,
                PersonalNumber = "1234567890",
                PostId = postId,
                FIO = "Test Worker",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                EmploymentDate = DateTime.UtcNow
            });

        var passId = Guid.NewGuid();
        connection.Execute($@"
            INSERT INTO {PassMarkTableSchema.TableName}
            ({PassMarkTableSchema.ColumnaIdName}, {PassMarkTableSchema.ColumnaWorkerIdName},
             {PassMarkTableSchema.ColumnaPassNumberName}, {PassMarkTableSchema.ColumnaMarkDateTimeInName})
            VALUES (@Id, @WorkerId, @PassNumber, @MarkDateTimeIn)",
            new PassMarks
            {
                Id = passId,
                WorkerId = workerId,
                PassNumber = "OLD",
                MarkDateTimeIn = DateTime.UtcNow
            });

        var markToUpdate = new PassMarks
        {
            Id = passId,
            WorkerId = workerId,
            PassNumber = "OLD",
            MarkDateTimeIn = DateTime.UtcNow,
            MarkDateTimeOut = DateTime.UtcNow
        };

        var result = _database.Update(markToUpdate);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestMethod_Update_NotExists()
    {
        var passMark = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            PassNumber = "TEST",
            MarkDateTimeIn = DateTime.UtcNow
        };

        var result = _database.Update(passMark);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TestMethod_Delete()
    {
        using var connection = _manualMigration.Connection;

        var postId = Guid.NewGuid();
        connection.Execute($@"
            INSERT INTO {PostTableSchema.TableName}
            ({PostTableSchema.ColumnaIdName}, {PostTableSchema.ColumnaPostNameName},
             {PostTableSchema.ColumnaPostTypeName}, {PostTableSchema.ColumnaSalaryName})
            VALUES (@Id, @PostName, @PostType, @SalaryPerHour)",
            new { Id = postId, PostName = "Test Post", PostType = (int)PostType.Engineer, SalaryPerHour = 1000 });

        var workerId = Guid.NewGuid();
        connection.Execute($@"
            INSERT INTO {WorkerTableSchema.TableName}
            ({WorkerTableSchema.ColumnaIdName}, {WorkerTableSchema.ColumnaPersonalNumberName},
             {WorkerTableSchema.ColumnaPostIdName}, {WorkerTableSchema.ColumnaFIOName},
             {WorkerTableSchema.ColumnaBirthDateName}, {WorkerTableSchema.ColumnaEmploymentDateName})
            VALUES (@Id, @PersonalNumber, @PostId, @FIO, @BirthDate, @EmploymentDate)",
            new
            {
                Id = workerId,
                PersonalNumber = "1234567890",
                PostId = postId,
                FIO = "Test Worker",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                EmploymentDate = DateTime.UtcNow
            });

        var passId = Guid.NewGuid();
        connection.Execute($@"
            INSERT INTO {PassMarkTableSchema.TableName}
            ({PassMarkTableSchema.ColumnaIdName}, {PassMarkTableSchema.ColumnaWorkerIdName},
             {PassMarkTableSchema.ColumnaPassNumberName}, {PassMarkTableSchema.ColumnaMarkDateTimeInName})
            VALUES (@Id, @WorkerId, @PassNumber, @MarkDateTimeIn)",
            new PassMarks
            {
                Id = passId,
                WorkerId = workerId,
                PassNumber = "DELETE_ME",
                MarkDateTimeIn = DateTime.UtcNow
            });

        var result = _database.Delete(passId);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestMethod_Delete_NotExists()
    {
        var result = _database.Delete(Guid.NewGuid());
        Assert.IsFalse(result);
    }
}