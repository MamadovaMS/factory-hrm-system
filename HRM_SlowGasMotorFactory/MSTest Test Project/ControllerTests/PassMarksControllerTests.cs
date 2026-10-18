using System.Net.Http.Json;

namespace HRM_SlowGasMotorFactory_TestsMSTest.ControllerTests;

[TestClass]
public sealed class PassMarksControllerTests : BaseControllerTests
{
    [TestMethod]
    public async Task TestMethod_Auth()
    {
        await CheckAuth("/api/passmarks");
    }

    [TestMethod]
    public async Task TestMethod_Get_All_WithoutFilters()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        var markId1 = InsertPassMark(connection, workerId, "PASS001", DateTime.UtcNow.AddHours(-2), null);
        var markId2 = InsertPassMark(connection, workerId, "PASS002", DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);
        var markId3 = InsertPassMark(connection, workerId, "PASS003", DateTime.UtcNow, null);

        var response = await Client.GetAsync("/api/passmarks");
        var content = await GetPassMarksFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(3, content.Count);
        Assert.IsTrue(content.Any(x => x.Id == markId1));
        Assert.IsTrue(content.Any(x => x.Id == markId2));
        Assert.IsTrue(content.Any(x => x.Id == markId3));
    }

    [TestMethod]
    public async Task TestMethod_Get_All_WithWorkerIdFilter()
    {
        using var connection = ManualMigration.Connection;
        var workerId1 = GetWorkerId(connection, "1111111111");
        var workerId2 = GetWorkerId(connection, "2222222222");

        InsertPassMark(connection, workerId1, "PASS001", DateTime.UtcNow, null);
        InsertPassMark(connection, workerId1, "PASS002", DateTime.UtcNow, null);
        InsertPassMark(connection, workerId2, "PASS003", DateTime.UtcNow, null);

        var response = await Client.GetAsync($"/api/passmarks?workerId={workerId1}");
        var content = await GetPassMarksFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(2, content.Count);
        Assert.IsTrue(content.All(x => x.WorkerId == workerId1));
    }

    [TestMethod]
    public async Task TestMethod_Get_All_WithPassNumberFilter()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        InsertPassMark(connection, workerId, "PASS001", DateTime.UtcNow.AddHours(-2), null);
        var markId = InsertPassMark(connection, workerId, "PASS002", DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);
        InsertPassMark(connection, workerId, "PASS003", DateTime.UtcNow, null);

        var response = await Client.GetAsync("/api/passmarks?passNumber=PASS002");
        var content = await GetPassMarksFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(1, content.Count);
        Assert.AreEqual(markId, content.First().Id);
        Assert.AreEqual("PASS002", content.First().PassNumber);
    }

    [TestMethod]
    public async Task TestMethod_Get_All_WithDateFromFilter()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        var now = DateTime.UtcNow;

        InsertPassMark(connection, workerId, "PASS001", now.AddDays(-2), null);
        InsertPassMark(connection, workerId, "PASS002", now.AddDays(-1), null);
        var markId = InsertPassMark(connection, workerId, "PASS003", now, null);

        var response = await Client.GetAsync($"/api/passmarks?dateFrom={now.AddDays(-0.5):yyyy-MM-ddTHH:mm:ss}");
        var content = await GetPassMarksFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(1, content.Count);
        Assert.AreEqual(markId, content.First().Id);
    }

    [TestMethod]
    public async Task TestMethod_Get_All_WithDateToFilter()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        var now = DateTime.UtcNow;

        var markId = InsertPassMark(connection, workerId, "PASS001", now.AddDays(-2), null);
        InsertPassMark(connection, workerId, "PASS002", now.AddDays(-1), null);
        InsertPassMark(connection, workerId, "PASS003", now, null);

        var response = await Client.GetAsync($"/api/passmarks?dateTo={now.AddDays(-1.5):yyyy-MM-ddTHH:mm:ss}");
        var content = await GetPassMarksFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(1, content.Count);
        Assert.AreEqual(markId, content.First().Id);
    }

    [TestMethod]
    public async Task TestMethod_Get_All_WithDateRangeFilter()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        var now = DateTime.UtcNow;

        InsertPassMark(connection, workerId, "PASS001", now.AddDays(-2), null);
        var markId = InsertPassMark(connection, workerId, "PASS002", now.AddDays(-1), null);
        InsertPassMark(connection, workerId, "PASS003", now, null);

        var response = await Client.GetAsync($"/api/passmarks?dateFrom={now.AddDays(-1.5):yyyy-MM-ddTHH:mm:ss}&dateTo={now.AddDays(-0.5):yyyy-MM-ddTHH:mm:ss}");
        var content = await GetPassMarksFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(1, content.Count);
        Assert.AreEqual(markId, content.First().Id);
    }

    [TestMethod]
    public async Task TestMethod_Get_All_WithCombinedFilters()
    {
        using var connection = ManualMigration.Connection;
        var workerId1 = GetWorkerId(connection, "1111111111");
        var workerId2 = GetWorkerId(connection, "2222222222");

        InsertPassMark(connection, workerId1, "PASS001", DateTime.UtcNow.AddHours(-2), null);
        var markId = InsertPassMark(connection, workerId1, "PASS002", DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);
        InsertPassMark(connection, workerId2, "PASS003", DateTime.UtcNow, null);

        var response = await Client.GetAsync($"/api/passmarks?workerId={workerId1}&passNumber=PASS002");
        var content = await GetPassMarksFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(1, content.Count);
        Assert.AreEqual(markId, content.First().Id);
        Assert.AreEqual(workerId1, content.First().WorkerId);
        Assert.AreEqual("PASS002", content.First().PassNumber);
    }

    [TestMethod]
    public async Task TestMethod_Get_All_NoRecords()
    {
        var response = await Client.GetAsync("/api/passmarks");
        var content = await GetPassMarksFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(0, content.Count);
    }

    [TestMethod]
    public async Task TestMethod_Get_All_FilterNoMatches()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        InsertPassMark(connection, workerId, "PASS001", DateTime.UtcNow, null);

        var response = await Client.GetAsync("/api/passmarks?passNumber=NONEXISTENT");
        var content = await GetPassMarksFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(0, content.Count);
    }

    [TestMethod]
    public async Task TestMethod_WorkerIn()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        var passMark = CreatePassMark(Guid.NewGuid(), workerId, "PASS001", DateTime.Today, null);

        var response = await Client.PostAsync("/api/passmarks/in", JsonContent.Create(passMark));
        var checkMark = GetPassMarkById(connection, passMark.Id);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        Assert.IsNotNull(checkMark);
        Assert.AreEqual(passMark.WorkerId, checkMark.WorkerId);
        Assert.AreEqual(passMark.PassNumber, checkMark.PassNumber);
        Assert.AreEqual(passMark.MarkDateTimeIn.Date, checkMark.MarkDateTimeIn.Date);
        Assert.IsNull(checkMark.MarkDateTimeOut);
    }

    [TestMethod]
    public async Task TestMethod_WorkerIn_Validation_PassNumberTooShort()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        var passMark = CreatePassMark(Guid.NewGuid(), workerId, "A", DateTime.UtcNow, null);

        var response = await Client.PostAsync("/api/passmarks/in", JsonContent.Create(passMark));
        var checkMark = GetPassMarkById(connection, passMark.Id);
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsTrue(content.Contains("One or more validation errors occurred"));
        Assert.IsNull(checkMark);
    }

    [TestMethod]
    public async Task TestMethod_WorkerIn_Validation_PassNumberTooLong()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        var passMark = CreatePassMark(Guid.NewGuid(), workerId, "ABCDEFGHIJKLMNOPQRST", DateTime.UtcNow, null);

        var response = await Client.PostAsync("/api/passmarks/in", JsonContent.Create(passMark));
        var checkMark = GetPassMarkById(connection, passMark.Id);
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsTrue(content.Contains("One or more validation errors occurred"));
        Assert.IsNull(checkMark);
    }

    [TestMethod]
    public async Task TestMethod_WorkerIn_Validation_EmptyPassNumber()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        var passMark = CreatePassMark(Guid.NewGuid(), workerId, "", DateTime.UtcNow, null);

        var response = await Client.PostAsync("/api/passmarks/in", JsonContent.Create(passMark));
        var checkMark = GetPassMarkById(connection, passMark.Id);
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsTrue(content.Contains("One or more validation errors occurred"));
        Assert.IsNull(checkMark);
    }

    [TestMethod]
    public async Task TestMethod_WorkerIn_Validation_NullPassNumber()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        var passMark = CreatePassMark(Guid.NewGuid(), workerId, null!, DateTime.UtcNow, null);

        var response = await Client.PostAsync("/api/passmarks/in", JsonContent.Create(passMark));
        var checkMark = GetPassMarkById(connection, passMark.Id);
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsTrue(content.Contains("One or more validation errors occurred"));
        Assert.IsNull(checkMark);
    }

    [TestMethod]
    public async Task TestMethod_WorkerIn_WorkerNotFound()
    {
        var passMark = CreatePassMark(Guid.NewGuid(), Guid.NewGuid(), "PASS001", DateTime.Today, null);

        var response = await Client.PostAsync("/api/passmarks/in", JsonContent.Create(passMark));
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task TestMethod_WorkerOut()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        var passMarkIn = CreatePassMark(Guid.NewGuid(), workerId, "PASS001", DateTime.Today, null);
        await Client.PostAsync("/api/passmarks/in", JsonContent.Create(passMarkIn));

        var passMarkOut = CreatePassMark(passMarkIn.Id, workerId, "PASS001", passMarkIn.MarkDateTimeIn, DateTime.Today);

        var response = await Client.PostAsync("/api/passmarks/out", JsonContent.Create(passMarkOut));
        var checkMark = GetPassMarkById(connection, passMarkOut.Id);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        Assert.IsNotNull(checkMark);
        Assert.IsNotNull(checkMark.MarkDateTimeOut);
        Assert.AreEqual(DateTime.Today.Date, checkMark.MarkDateTimeOut.Value.Date);
    }

    [TestMethod]
    public async Task TestMethod_WorkerOut_NoActiveEntry()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        var passMark = CreatePassMark(Guid.NewGuid(), workerId, "PASS001", DateTime.Today, DateTime.Today);

        var response = await Client.PostAsync("/api/passmarks/out", JsonContent.Create(passMark));
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.AreEqual("Нет активной записи входа", content);
    }

    [TestMethod]
    public async Task TestMethod_WorkerOut_DifferentPassNumber()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        var passMarkIn = CreatePassMark(Guid.NewGuid(), workerId, "PASS001", DateTime.Today, null);
        await Client.PostAsync("/api/passmarks/in", JsonContent.Create(passMarkIn));

        var passMarkOut = CreatePassMark(passMarkIn.Id, workerId, "DIFFERENT", passMarkIn.MarkDateTimeIn, DateTime.Today);

        var response = await Client.PostAsync("/api/passmarks/out", JsonContent.Create(passMarkOut));
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.AreEqual("Нет активной записи входа", content);
    }

    [TestMethod]
    public async Task TestMethod_WorkerOut_WrongWorkerId()
    {
        using var connection = ManualMigration.Connection;
        var workerId1 = GetWorkerId(connection, "1111111111");
        var workerId2 = GetWorkerId(connection, "2222222222");

        var passMarkIn = CreatePassMark(Guid.NewGuid(), workerId1, "PASS001", DateTime.Today, null);
        await Client.PostAsync("/api/passmarks/in", JsonContent.Create(passMarkIn));

        var passMarkOut = CreatePassMark(passMarkIn.Id, workerId2, "PASS001", passMarkIn.MarkDateTimeIn, DateTime.Today);

        var response = await Client.PostAsync("/api/passmarks/out", JsonContent.Create(passMarkOut));
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.AreEqual("Нет активной записи входа", content);
    }

    [TestMethod]
    public async Task TestMethod_WorkerOut_Validation_PassNumberTooShort()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        var passMark = CreatePassMark(Guid.NewGuid(), workerId, "A", DateTime.UtcNow.AddHours(-8), DateTime.UtcNow);

        var response = await Client.PostAsync("/api/passmarks/out", JsonContent.Create(passMark));
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsTrue(content.Contains("One or more validation errors occurred"));
    }

    [TestMethod]
    public async Task TestMethod_WorkerOut_Validation_EmptyPassNumber()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        var passMark = CreatePassMark(Guid.NewGuid(), workerId, "", DateTime.UtcNow.AddHours(-8), DateTime.UtcNow);

        var response = await Client.PostAsync("/api/passmarks/out", JsonContent.Create(passMark));
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsTrue(content.Contains("One or more validation errors occurred"));
    }

    [TestMethod]
    public async Task TestMethod_WorkerOut_WorkerNotFound()
    {
        var passMark = CreatePassMark(Guid.NewGuid(), Guid.NewGuid(), "PASS001", DateTime.Today, DateTime.Today);

        var response = await Client.PostAsync("/api/passmarks/out", JsonContent.Create(passMark));
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.AreEqual("Нет активной записи входа", content);
    }

    [TestMethod]
    public async Task TestMethod_MultipleWorkersInOut()
    {
        using var connection = ManualMigration.Connection;
        var workerId1 = GetWorkerId(connection, "1111111111");
        var workerId2 = GetWorkerId(connection, "2222222222");

        // Worker 1 входит
        var passMark1In = CreatePassMark(Guid.NewGuid(), workerId1, "PASS001", DateTime.Today, null);
        await Client.PostAsync("/api/passmarks/in", JsonContent.Create(passMark1In));

        // Worker 2 входит
        var passMark2In = CreatePassMark(Guid.NewGuid(), workerId2, "PASS002", DateTime.Today, null);
        await Client.PostAsync("/api/passmarks/in", JsonContent.Create(passMark2In));

        // Worker 1 выходит
        var passMark1Out = CreatePassMark(passMark1In.Id, workerId1, "PASS001", passMark1In.MarkDateTimeIn, DateTime.Today);
        var response1 = await Client.PostAsync("/api/passmarks/out", JsonContent.Create(passMark1Out));

        // Worker 2 выходит
        var passMark2Out = CreatePassMark(passMark2In.Id, workerId2, "PASS002", passMark2In.MarkDateTimeIn, DateTime.Today);
        var response2 = await Client.PostAsync("/api/passmarks/out", JsonContent.Create(passMark2Out));

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response1.StatusCode);
        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response2.StatusCode);

        var checkMark1 = GetPassMarkById(connection, passMark1Out.Id);
        var checkMark2 = GetPassMarkById(connection, passMark2Out.Id);

        Assert.IsNotNull(checkMark1.MarkDateTimeOut);
        Assert.IsNotNull(checkMark2.MarkDateTimeOut);
    }
}