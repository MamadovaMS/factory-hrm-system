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
public sealed class WorkerDatabaseTests
{
    private static ManualMigration _manualMigration = null!;
    private WorkerDatabase _database = null!;

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
        _database = new WorkerDatabase(_manualMigration, new FakeLogger<WorkerDatabase>());
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
    public void TestMethod_Read_All()
    {
        using var connection = _manualMigration.Connection;
        InsertWorker(connection, 3);
        var workers = _database.Read();
        Assert.IsNotNull(workers);
        Assert.AreEqual(3, workers.Count);
        Assert.IsTrue(workers.All(x => x.Post is not null));
    }

    [TestMethod]
    public void TestMethod_Read_Empty()
    {
        var workers = _database.Read();
        Assert.IsNotNull(workers);
        Assert.AreEqual(0, workers.Count);
    }

    [TestMethod]
    public void TestMethod_Read_Id()
    {
        using var connection = _manualMigration.Connection;
        var workerId = InsertWorker(connection, 1);
        var workerFromDb = _database.Read(workerId);
        Assert.IsNotNull(workerFromDb);
        Assert.AreEqual(workerId, workerFromDb.Id);
        Assert.IsNotNull(workerFromDb.Post);
    }

    [TestMethod]
    public void TestMethod_Read_Id_WrongId()
    {
        using var connection = _manualMigration.Connection;
        InsertWorker(connection, 1);
        var workerFromDb = _database.Read(Guid.NewGuid());
        Assert.IsNull(workerFromDb);
    }

    [TestMethod]
    public void TestMethod_ReadWorkerPreviousPosts()
    {
        using var connection = _manualMigration.Connection;

        var postId1 = GetPostId(connection, "Post Worker 1");
        var postId2 = GetPostId(connection, "Post Worker 2");
        var postId3 = GetPostId(connection, "Post Worker 3");

        var workerId = Guid.NewGuid();

        var queryWorker = $@"
            INSERT INTO {WorkerTableSchema.TableName}
            ({WorkerTableSchema.ColumnaIdName}, {WorkerTableSchema.ColumnaPersonalNumberName},
             {WorkerTableSchema.ColumnaPostIdName}, {WorkerTableSchema.ColumnaFIOName},
             {WorkerTableSchema.ColumnaBirthDateName}, {WorkerTableSchema.ColumnaEmploymentDateName})
            VALUES (@Id, @PersonalNumber, @PostId, @FIO, @BirthDate, @EmploymentDate)";

        connection.Execute(queryWorker, new Worker
        {
            Id = workerId,
            PersonalNumber = "1111111111",
            PostId = postId1,
            FIO = "Test Worker",
            BirthDate = DateTime.UtcNow.AddDays(-20),
            EmploymentDate = DateTime.UtcNow
        });

        var updateHistoryQuery = $@"
            UPDATE {WorkerTableSchema.TableName}
            SET 
                {WorkerTableSchema.ColumnaPostBeforeIdName} = @PostBeforeId,
                {WorkerTableSchema.ColumnaPostBeforeBeforeIdName} = @PostBeforeBeforeId
            WHERE {WorkerTableSchema.ColumnaIdName} = @Id";

        connection.Execute(updateHistoryQuery, new { Id = workerId, PostBeforeId = postId2, PostBeforeBeforeId = postId3 });

        var posts = _database.ReadWorkerPreviousPosts(workerId);
        Assert.IsNotNull(posts);
        Assert.IsTrue(posts.Count >= 2);
    }

    [TestMethod]
    public void TestMethod_Create()
    {
        using var connection = _manualMigration.Connection;
        var worker = CreateWorker(connection);
        var result = _database.Create(worker);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestMethod_Create_IdExists()
    {
        using var connection = _manualMigration.Connection;
        var workerId = InsertWorker(connection, 1);
        var worker = CreateWorker(connection, workerId);
        var result = _database.Create(worker);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TestMethod_Create_PersonalNumberExists()
    {
        using var connection = _manualMigration.Connection;
        var existingPersonalNumber = "9999999999";
        InsertWorkerWithPersonalNumber(connection, existingPersonalNumber);

        var newWorker = CreateWorker(connection, Guid.NewGuid(), existingPersonalNumber);
        var result = _database.Create(newWorker);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TestMethod_Create_PostIdNotExists()
    {
        using var connection = _manualMigration.Connection;
        var worker = CreateWorker(connection);
        worker.PostId = Guid.NewGuid();
        var result = _database.Create(worker);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TestMethod_Update()
    {
        using var connection = _manualMigration.Connection;
        var workerId = InsertWorker(connection, 1);
        var worker = new Worker
        {
            Id = workerId,
            PersonalNumber = "8888888888",
            PostId = GetPostId(connection, "New Post"),
            FIO = "Updated FIO",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            EmploymentDate = DateTime.UtcNow.AddDays(-5)
        };
        var result = _database.Update(worker);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestMethod_Update_NotExists()
    {
        var worker = CreateWorker(_manualMigration.Connection);
        worker.Id = Guid.NewGuid();
        var result = _database.Update(worker);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TestMethod_Update_PersonalNumberExists()
    {
        using var connection = _manualMigration.Connection;
        var personalNumber1 = "7777777777";
        var personalNumber2 = "8888888888";

        var worker1Id = InsertWorkerWithPersonalNumber(connection, personalNumber1);
        var worker2Id = InsertWorkerWithPersonalNumber(connection, personalNumber2);

        var workerToUpdate = new Worker
        {
            Id = worker2Id,
            PersonalNumber = personalNumber1,
            PostId = GetPostId(connection, "Some Post"),
            FIO = "Test",
            BirthDate = DateTime.UtcNow.AddYears(-20),
            EmploymentDate = DateTime.UtcNow
        };
        var result = _database.Update(workerToUpdate);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TestMethod_Delete()
    {
        using var connection = _manualMigration.Connection;
        var workerId = InsertWorker(connection, 1);
        var result = _database.Delete(workerId);
        Assert.IsTrue(result);

        var worker = _database.Read(workerId);
        Assert.IsNull(worker);
    }

    [TestMethod]
    public void TestMethod_Delete_NotExists()
    {
        var result = _database.Delete(Guid.NewGuid());
        Assert.IsFalse(result);
    }

    private static Guid InsertWorker(IDbConnection connection, int count)
    {
        var lastId = Guid.NewGuid();
        for (int i = 0; i < count; i++)
        {
            lastId = Guid.NewGuid();
            var query = $@"
                INSERT INTO {WorkerTableSchema.TableName}
                ({WorkerTableSchema.ColumnaIdName}, {WorkerTableSchema.ColumnaPersonalNumberName},
                 {WorkerTableSchema.ColumnaPostIdName}, {WorkerTableSchema.ColumnaFIOName},
                 {WorkerTableSchema.ColumnaBirthDateName}, {WorkerTableSchema.ColumnaEmploymentDateName})
                VALUES (@Id, @PersonalNumber, @PostId, @FIO, @BirthDate, @EmploymentDate)";

            connection.Execute(query, new Worker
            {
                Id = lastId,
                PersonalNumber = $"12345678{i}",
                PostId = GetPostId(connection, $"Post Worker {i}"),
                FIO = $"FIO {i}",
                BirthDate = DateTime.UtcNow.AddDays(-20),
                EmploymentDate = DateTime.UtcNow
            });
        }
        return lastId;
    }

    private static Guid InsertWorkerWithPersonalNumber(IDbConnection connection, string personalNumber)
    {
        var workerId = Guid.NewGuid();
        var query = $@"
            INSERT INTO {WorkerTableSchema.TableName}
            ({WorkerTableSchema.ColumnaIdName}, {WorkerTableSchema.ColumnaPersonalNumberName},
             {WorkerTableSchema.ColumnaPostIdName}, {WorkerTableSchema.ColumnaFIOName},
             {WorkerTableSchema.ColumnaBirthDateName}, {WorkerTableSchema.ColumnaEmploymentDateName})
            VALUES (@Id, @PersonalNumber, @PostId, @FIO, @BirthDate, @EmploymentDate)";

        connection.Execute(query, new Worker
        {
            Id = workerId,
            PersonalNumber = personalNumber,
            PostId = GetPostId(connection, "Post Worker"),
            FIO = "Test FIO",
            BirthDate = DateTime.UtcNow.AddYears(-20),
            EmploymentDate = DateTime.UtcNow
        });
        return workerId;
    }

    private static Worker CreateWorker(IDbConnection connection)
    {
        return CreateWorker(connection, Guid.NewGuid());
    }

    private static Worker CreateWorker(IDbConnection connection, Guid id)
    {
        return CreateWorker(connection, id, "1234567890");
    }

    private static Worker CreateWorker(IDbConnection connection, string personalNumber)
    {
        return CreateWorker(connection, Guid.NewGuid(), personalNumber);
    }

    private static Worker CreateWorker(IDbConnection connection, Guid id, string personalNumber)
    {
        return new Worker
        {
            Id = id,
            PersonalNumber = personalNumber,
            PostId = GetPostId(connection, "Post Worker"),
            FIO = "Test FIO",
            BirthDate = DateTime.UtcNow.AddYears(-20),
            EmploymentDate = DateTime.UtcNow
        };
    }

    private static Guid GetPostId(IDbConnection connection, string postName)
    {
        var queryGetPost = $@"SELECT {PostTableSchema.ColumnaIdName} FROM {PostTableSchema.TableName} WHERE {PostTableSchema.ColumnaPostNameName} = @PostName";
        var postId = connection.QueryFirstOrDefault<Guid?>(queryGetPost, new { PostName = postName });

        if (postId.HasValue)
        {
            return postId.Value;
        }

        var newPostId = Guid.NewGuid();
        var queryPost = $@"
            INSERT INTO {PostTableSchema.TableName}
            ({PostTableSchema.ColumnaIdName}, {PostTableSchema.ColumnaPostNameName},
             {PostTableSchema.ColumnaPostTypeName}, {PostTableSchema.ColumnaSalaryName})
            VALUES (@Id, @PostName, @PostType, @SalaryPerHour)";

        connection.Execute(queryPost, new Post
        {
            Id = newPostId,
            PostName = postName,
            PostType = PostType.Engineer,
            SalaryPerHour = 500
        });
        return newPostId;
    }
}