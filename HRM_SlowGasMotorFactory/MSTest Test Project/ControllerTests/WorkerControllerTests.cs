using Dapper;
using HRM_SlowGasMotorFactory_Database.SchemaDto;
using HRM_SlowGasMotorFactory_DataModels.DataModels;
using System.Data;
using System.Net.Http.Json;

namespace HRM_SlowGasMotorFactory_TestsMSTest.ControllerTests;

[TestClass]
public sealed class WorkerControllerTests : BaseControllerTests
{
    [TestMethod]
    public async Task TestMethod_Auth()
    {
        await CheckAuth("/api/worker");
    }

    // ==================== GET ALL TESTS ====================

    [TestMethod]
    public async Task TestMethod_Get_All_WithoutFilters()
    {
        using var connection = ManualMigration.Connection;
        var workerId = InsertWorker(connection, "0123456789", "Иванов Иван Петрович", false);
        InsertWorker(connection, "1234567890", "Петров Пётр Петрович", false);
        InsertWorker(connection, "2345678901", "Сидоров Сидор Сидорович", false);

        var response = await Client.GetAsync("/api/worker");
        var content = await GetWorkersFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(3, content.Count);
        var worker = content.FirstOrDefault(x => x.Id == workerId);
        Assert.IsNotNull(worker);
        Assert.AreEqual("0123456789", worker.PersonalNumber);
        Assert.AreEqual("Иванов Иван Петрович", worker.FIO);
        Assert.IsFalse(worker.IsDeleted);
    }

    [TestMethod]
    public async Task TestMethod_Get_All_WithPersonalNumberFilter()
    {
        using var connection = ManualMigration.Connection;
        InsertWorker(connection, "0123456789", "Иванов Иван Петрович", false);
        var workerId = InsertWorker(connection, "1234567890", "Петров Пётр Петрович", false);
        InsertWorker(connection, "2345678901", "Сидоров Сидор Сидорович", false);

        var response = await Client.GetAsync("/api/worker?personalNumber=1234567890");
        var content = await GetWorkersFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(1, content.Count);
        Assert.IsTrue(content.Any(x => x.Id == workerId));
        Assert.AreEqual("1234567890", content.First().PersonalNumber);
    }

    [TestMethod]
    public async Task TestMethod_Get_All_WithPostIdFilter()
    {
        using var connection = ManualMigration.Connection;
        var postId1 = GetPostId(connection, "Post Worker 1");
        var postId2 = GetPostId(connection, "Post Worker 2");

        var workerId1 = InsertWorkerWithPostId(connection, "0123456789", "Иванов Иван", postId1, false);
        InsertWorkerWithPostId(connection, "1234567890", "Петров Пётр", postId2, false);

        var response = await Client.GetAsync($"/api/worker?postId={postId1}");
        var content = await GetWorkersFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(1, content.Count);
        Assert.AreEqual(workerId1, content.First().Id);
    }

    [TestMethod]
    public async Task TestMethod_Get_All_NoRecords()
    {
        var response = await Client.GetAsync("/api/worker");
        var content = await GetWorkersFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(0, content.Count);
    }

    [TestMethod]
    public async Task TestMethod_Get_All_FilterNoMatches()
    {
        using var connection = ManualMigration.Connection;
        InsertWorker(connection, "1234567890", "Иванов Иван Петрович", false);

        var response = await Client.GetAsync("/api/worker?personalNumber=9999999999");
        var content = await GetWorkersFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(0, content.Count);
    }

    // ==================== GET SINGLE TESTS ====================

    [TestMethod]
    public async Task TestMethod_Get_Single_HaveRecord()
    {
        using var connection = ManualMigration.Connection;
        var workerId = InsertWorker(connection, "1234567890", "Иванов Иван Петрович", false);

        var response = await Client.GetAsync($"/api/worker/{workerId}");
        var content = await GetWorkerFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual("1234567890", content.PersonalNumber);
        Assert.AreEqual("Иванов Иван Петрович", content.FIO);
        Assert.IsFalse(content.IsDeleted);
    }

    [TestMethod]
    public async Task TestMethod_Get_Single_NoRecord()
    {
        using var connection = ManualMigration.Connection;
        InsertWorker(connection, "1234567890", "Иванов Иван Петрович", false);

        var response = await Client.GetAsync($"/api/worker/{Guid.NewGuid()}");

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ==================== CREATE TESTS ====================

    [TestMethod]
    public async Task TestMethod_Create()
    {
        using var connection = ManualMigration.Connection;
        var postId = GetPostId(connection, "Post Worker");
        var newWorker = CreateWorker(Guid.NewGuid(), "0123456789", postId);

        var response = await Client.PostAsync("/api/worker", JsonContent.Create(newWorker));
        var checkWorker = GetWorkerById(connection, newWorker.Id);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        Assert.IsNotNull(checkWorker);
        Assert.AreEqual(newWorker.PersonalNumber, checkWorker.PersonalNumber);
        Assert.AreEqual(newWorker.FIO, checkWorker.FIO);
        Assert.IsFalse(checkWorker.IsDeleted);
    }

    [TestMethod]
    public async Task TestMethod_Create_WithPatronymic()
    {
        using var connection = ManualMigration.Connection;
        var postId = GetPostId(connection, "Post Worker");
        var newWorker = CreateWorker(Guid.NewGuid(), "0123456789", postId);
        newWorker.FIO = "Иванов Иван Иванович";

        var response = await Client.PostAsync("/api/worker", JsonContent.Create(newWorker));
        var checkWorker = GetWorkerById(connection, newWorker.Id);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        Assert.IsNotNull(checkWorker);
        Assert.AreEqual("Иванов Иван Иванович", checkWorker.FIO);
    }

    [TestMethod]
    public async Task TestMethod_Create_Validation_PersonalNumberTooShort()
    {
        using var connection = ManualMigration.Connection;
        var postId = GetPostId(connection, "Post Worker");
        var newWorker = CreateWorker(Guid.NewGuid(), "12345", postId);

        var response = await Client.PostAsync("/api/worker", JsonContent.Create(newWorker));
        var checkWorker = GetWorkerById(connection, newWorker.Id);
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsTrue(content.Contains("One or more validation errors occurred"));
        Assert.IsNull(checkWorker);
    }

    [TestMethod]
    public async Task TestMethod_Create_Validation_TooYoung()
    {
        using var connection = ManualMigration.Connection;
        var postId = GetPostId(connection, "Post Worker");
        var newWorker = CreateWorker(Guid.NewGuid(), "0123456789", postId);
        newWorker.BirthDate = DateTime.UtcNow.AddYears(-15);

        var response = await Client.PostAsync("/api/worker", JsonContent.Create(newWorker));
        var checkWorker = GetWorkerById(connection, newWorker.Id);
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsTrue(content.Contains("One or more validation errors occurred"));
        Assert.IsNull(checkWorker);
    }

    [TestMethod]
    public async Task TestMethod_Create_PostIdNotExists()
    {
        using var connection = ManualMigration.Connection;
        var newWorker = CreateWorker(Guid.NewGuid(), "0123456789", Guid.NewGuid());

        var response = await Client.PostAsync("/api/worker", JsonContent.Create(newWorker));
        var checkWorker = GetWorkerById(connection, newWorker.Id);
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.IsNull(checkWorker);
    }

    [TestMethod]
    public async Task TestMethod_Create_IdExists()
    {
        using var connection = ManualMigration.Connection;
        var workerId = InsertWorker(connection, "1234567890", "Иванов Иван Петрович", false);
        var postId = GetPostId(connection, "Post Worker");
        var newWorker = CreateWorker(workerId, "0123456789", postId);

        var response = await Client.PostAsync("/api/worker", JsonContent.Create(newWorker));
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.AreEqual(string.Empty, content);
    }

    // ==================== UPDATE TESTS ====================

    [TestMethod]
    public async Task TestMethod_Update()
    {
        using var connection = ManualMigration.Connection;
        var workerId = InsertWorker(connection, "1234567890", "Иванов Иван Петрович", false);
        var postId = GetPostId(connection, "Post Worker");
        var worker = CreateWorker(workerId, "0123456789", postId);

        var response = await Client.PutAsync("/api/worker", JsonContent.Create(worker));
        var checkWorker = GetWorkerById(connection, worker.Id);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        Assert.IsNotNull(checkWorker);
        Assert.AreEqual(worker.PersonalNumber, checkWorker.PersonalNumber);
        Assert.AreEqual(worker.FIO, checkWorker.FIO);
    }

    [TestMethod]
    public async Task TestMethod_Update_OnlyPersonalNumber()
    {
        using var connection = ManualMigration.Connection;
        var workerId = InsertWorker(connection, "1234567890", "Иванов Иван Петрович", false);

        var worker = GetWorkerById(connection, workerId);
        worker.PersonalNumber = "9999999999";

        var response = await Client.PutAsync("/api/worker", JsonContent.Create(worker));
        var checkWorker = GetWorkerById(connection, workerId);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        Assert.IsNotNull(checkWorker);
        Assert.AreEqual("9999999999", checkWorker.PersonalNumber);
        Assert.AreEqual(worker.FIO, checkWorker.FIO);
    }

    [TestMethod]
    public async Task TestMethod_Update_OnlyFIO()
    {
        using var connection = ManualMigration.Connection;
        var workerId = InsertWorker(connection, "1234567890", "Иванов Иван Петрович", false);

        var worker = GetWorkerById(connection, workerId);
        worker.FIO = "Петров Пётр Петрович";

        var response = await Client.PutAsync("/api/worker", JsonContent.Create(worker));
        var checkWorker = GetWorkerById(connection, workerId);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        Assert.IsNotNull(checkWorker);
        Assert.AreEqual("Петров Пётр Петрович", checkWorker.FIO);
        Assert.AreEqual(worker.PersonalNumber, checkWorker.PersonalNumber);
    }

    // ==================== DELETE TESTS ====================

    [TestMethod]
    public async Task TestMethod_Delete()
    {
        using var connection = ManualMigration.Connection;
        var workerId = InsertWorker(connection, "1234567890", "Иванов Иван Петрович", false);

        var response = await Client.DeleteAsync($"/api/worker/{workerId}");
        var checkWorker = GetWorkerById(connection, workerId);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        Assert.IsNotNull(checkWorker);
        Assert.IsTrue(checkWorker.IsDeleted);
    }

    [TestMethod]
    public async Task TestMethod_Delete_NotExists()
    {
        using var connection = ManualMigration.Connection;
        InsertWorker(connection, "1234567890", "Иванов Иван Петрович", false);

        var response = await Client.DeleteAsync($"/api/worker/{Guid.NewGuid()}");
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.AreEqual("Сотрудник не найден", content);
    }

    // ==================== HELPER METHODS ====================

    private static Guid InsertWorkerWithPostId(IDbConnection connection, string personalNumber, string fio, Guid postId, bool isDeleted)
    {
        var queryWorker = $@"
            INSERT INTO {WorkerTableSchema.TableName}
            ({WorkerTableSchema.ColumnaIdName}, {WorkerTableSchema.ColumnaPersonalNumberName},
             {WorkerTableSchema.ColumnaPostIdName}, {WorkerTableSchema.ColumnaFIOName},
             {WorkerTableSchema.ColumnaBirthDateName}, {WorkerTableSchema.ColumnaEmploymentDateName},
             {WorkerTableSchema.ColumnaIsDeletedName})
            VALUES (@Id, @PersonalNumber, @PostId, @FIO, @BirthDate, @EmploymentDate, @IsDeleted);";

        var workerId = Guid.NewGuid();
        connection.Execute(queryWorker, new Worker
        {
            Id = workerId,
            PersonalNumber = personalNumber,
            PostId = postId,
            FIO = fio,
            BirthDate = DateTime.UtcNow.AddYears(-20),
            EmploymentDate = DateTime.UtcNow.AddDays(-100),
            IsDeleted = isDeleted
        });
        return workerId;
    }

    private static Guid InsertWorkerWithBirthDate(IDbConnection connection, string personalNumber, string fio, DateTime birthDate, bool isDeleted)
    {
        var queryWorker = $@"
            INSERT INTO {WorkerTableSchema.TableName}
            ({WorkerTableSchema.ColumnaIdName}, {WorkerTableSchema.ColumnaPersonalNumberName},
             {WorkerTableSchema.ColumnaPostIdName}, {WorkerTableSchema.ColumnaFIOName},
             {WorkerTableSchema.ColumnaBirthDateName}, {WorkerTableSchema.ColumnaEmploymentDateName},
             {WorkerTableSchema.ColumnaIsDeletedName})
            VALUES (@Id, @PersonalNumber, @PostId, @FIO, @BirthDate, @EmploymentDate, @IsDeleted);";

        var workerId = Guid.NewGuid();
        connection.Execute(queryWorker, new Worker
        {
            Id = workerId,
            PersonalNumber = personalNumber,
            PostId = GetPostId(connection, "Post Worker"),
            FIO = fio,
            BirthDate = birthDate,
            EmploymentDate = DateTime.UtcNow.AddDays(-100),
            IsDeleted = isDeleted
        });
        return workerId;
    }

    private static Guid InsertWorkerWithEmploymentDate(IDbConnection connection, string personalNumber, string fio, DateTime employmentDate, bool isDeleted)
    {
        var queryWorker = $@"
            INSERT INTO {WorkerTableSchema.TableName}
            ({WorkerTableSchema.ColumnaIdName}, {WorkerTableSchema.ColumnaPersonalNumberName},
             {WorkerTableSchema.ColumnaPostIdName}, {WorkerTableSchema.ColumnaFIOName},
             {WorkerTableSchema.ColumnaBirthDateName}, {WorkerTableSchema.ColumnaEmploymentDateName},
             {WorkerTableSchema.ColumnaIsDeletedName})
            VALUES (@Id, @PersonalNumber, @PostId, @FIO, @BirthDate, @EmploymentDate, @IsDeleted);";

        var workerId = Guid.NewGuid();
        connection.Execute(queryWorker, new Worker
        {
            Id = workerId,
            PersonalNumber = personalNumber,
            PostId = GetPostId(connection, "Post Worker"),
            FIO = fio,
            BirthDate = DateTime.UtcNow.AddYears(-20),
            EmploymentDate = employmentDate,
            IsDeleted = isDeleted
        });
        return workerId;
    }
}