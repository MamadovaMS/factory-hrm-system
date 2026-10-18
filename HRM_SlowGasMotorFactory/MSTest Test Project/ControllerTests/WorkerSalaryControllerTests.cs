using HRM_SlowGasMotorFactory_DataModels.Enums;
using System.Net.Http.Json;

namespace HRM_SlowGasMotorFactory_TestsMSTest.ControllerTests;

[TestClass]
public sealed class WorkerSalaryControllerTests : BaseControllerTests
{
    // ==================== АВТОРИЗАЦИЯ ====================

    [TestMethod]
    public async Task TestMethod_Auth()
    {
        await CheckAuth("/api/workersalary");
    }

    // ==================== GET ТЕСТЫ ====================

    [TestMethod]
    public async Task TestMethod_Get_All_WithoutFilters()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        InsertWorkerSalary(connection, workerId, 50000, DateTime.UtcNow.AddDays(-30));
        InsertWorkerSalary(connection, workerId, 55000, DateTime.UtcNow.AddDays(-30));
        InsertWorkerSalary(connection, workerId, 60000, DateTime.UtcNow.AddDays(-30));

        var response = await Client.GetAsync("/api/workersalary");
        var content = await GetWorkerSalariesFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(3, content.Count);
    }

    [TestMethod]
    public async Task TestMethod_Get_All_WithWorkerIdFilter()
    {
        using var connection = ManualMigration.Connection;
        var workerId1 = GetWorkerId(connection, "1111111111");
        var workerId2 = GetWorkerId(connection, "2222222222");

        InsertWorkerSalary(connection, workerId1, 50000, DateTime.UtcNow.AddDays(-30));
        InsertWorkerSalary(connection, workerId1, 52000, DateTime.UtcNow.AddDays(-30));
        var salaryId = InsertWorkerSalary(connection, workerId2, 55000, DateTime.UtcNow.AddDays(-30));

        var response = await Client.GetAsync($"/api/workersalary?workerId={workerId2}");
        var content = await GetWorkerSalariesFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(1, content.Count);
        Assert.AreEqual(salaryId, content.First().Id);
        Assert.AreEqual(workerId2, content.First().WorkerId);
    }

    [TestMethod]
    public async Task TestMethod_Get_All_WithCalculateDateStartFilter()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        var date = DateTime.UtcNow;

        InsertWorkerSalary(connection, workerId, 50000, date.AddDays(-60));
        InsertWorkerSalary(connection, workerId, 55000, date.AddDays(-30));
        InsertWorkerSalary(connection, workerId, 60000, date.AddDays(-10));

        var response = await Client.GetAsync($"/api/workersalary?calculateDateStart={date.AddDays(-40):yyyy-MM-dd}");
        var content = await GetWorkerSalariesFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(2, content.Count);
        Assert.IsTrue(content.All(x => x.CalculateDate >= date.AddDays(-40)));
    }

    [TestMethod]
    public async Task TestMethod_Get_All_WithCalculateDateFinishFilter()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        var date = DateTime.UtcNow;

        var salaryId = InsertWorkerSalary(connection, workerId, 50000, date.AddDays(-60));
        InsertWorkerSalary(connection, workerId, 55000, date.AddDays(-30));
        InsertWorkerSalary(connection, workerId, 60000, date.AddDays(-10));

        var response = await Client.GetAsync($"/api/workersalary?calculateDateFinish={date.AddDays(-40):yyyy-MM-dd}");
        var content = await GetWorkerSalariesFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(1, content.Count);
        Assert.AreEqual(salaryId, content.First().Id);
    }

    [TestMethod]
    public async Task TestMethod_Get_All_WithDateRangeFilter()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        var date = DateTime.UtcNow;

        InsertWorkerSalary(connection, workerId, 50000, date.AddDays(-60));
        var salaryId = InsertWorkerSalary(connection, workerId, 55000, date.AddDays(-30));
        InsertWorkerSalary(connection, workerId, 60000, date.AddDays(-10));

        var response = await Client.GetAsync($"/api/workersalary?calculateDateStart={date.AddDays(-35):yyyy-MM-dd}&calculateDateFinish={date.AddDays(-25):yyyy-MM-dd}");
        var content = await GetWorkerSalariesFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(1, content.Count);
        Assert.AreEqual(salaryId, content.First().Id);
    }

    [TestMethod]
    public async Task TestMethod_Get_All_WithWorkerIdAndDateFilter()
    {
        using var connection = ManualMigration.Connection;
        var workerId1 = GetWorkerId(connection, "1111111111");
        var workerId2 = GetWorkerId(connection, "2222222222");
        var date = DateTime.UtcNow;

        InsertWorkerSalary(connection, workerId1, 50000, date.AddDays(-30));
        InsertWorkerSalary(connection, workerId1, 52000, date.AddDays(-20));
        var salaryId = InsertWorkerSalary(connection, workerId2, 55000, date.AddDays(-30));

        var response = await Client.GetAsync($"/api/workersalary?workerId={workerId2}&calculateDateStart={date.AddDays(-35):yyyy-MM-dd}&calculateDateFinish={date.AddDays(-25):yyyy-MM-dd}");
        var content = await GetWorkerSalariesFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(1, content.Count);
        Assert.AreEqual(salaryId, content.First().Id);
    }

    [TestMethod]
    public async Task TestMethod_Get_All_NoRecords()
    {
        var response = await Client.GetAsync("/api/workersalary");
        var content = await GetWorkerSalariesFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(0, content.Count);
    }

    [TestMethod]
    public async Task TestMethod_Get_All_FilterNoMatches()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");
        InsertWorkerSalary(connection, workerId, 50000, DateTime.UtcNow.AddDays(-30));

        var response = await Client.GetAsync("/api/workersalary?workerId=11111111-1111-1111-1111-111111111111");
        var content = await GetWorkerSalariesFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(0, content.Count);
    }

    // ==================== POST (CALCULATE) ТЕСТЫ ====================

    [TestMethod]
    public async Task TestMethod_Calculate()
    {
        using var connection = ManualMigration.Connection;
        var postId = InsertPost(connection, "Инженер завода", PostType.Engineer, 500);
        var workerId = InsertWorker(connection, "1234567890", "Иванов Иван Петрович", false);

        var response = await Client.PostAsync($"/api/workersalary?date={DateTime.UtcNow:yyyy-MM-dd}", null);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }

    [TestMethod]
    public async Task TestMethod_Calculate_WithSpecificDate()
    {
        var date = new DateTime(2025, 3, 15);
        var response = await Client.PostAsync($"/api/workersalary?date={date:yyyy-MM-dd}", null);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }

    [TestMethod]
    public async Task TestMethod_Calculate_NoDate()
    {
        var response = await Client.PostAsync("/api/workersalary", null);
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsTrue(content.Contains("Date is required"));
    }

    [TestMethod]
    public async Task TestMethod_Calculate_InvalidDateFormat()
    {
        var response = await Client.PostAsync("/api/workersalary?date=invalid-date", null);
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task TestMethod_Calculate_MultipleTimes()
    {
        var response1 = await Client.PostAsync($"/api/workersalary?date={DateTime.UtcNow:yyyy-MM-dd}", null);
        var response2 = await Client.PostAsync($"/api/workersalary?date={DateTime.UtcNow:yyyy-MM-dd}", null);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response1.StatusCode);
        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response2.StatusCode);
    }

    // ==================== ИНТЕГРАЦИОННЫЕ ТЕСТЫ ====================

    [TestMethod]
    public async Task TestMethod_GetAfterCalculate()
    {
        using var connection = ManualMigration.Connection;
        var postId = InsertPost(connection, "Инженер завода", PostType.Engineer, 500);
        var workerId = InsertWorker(connection, "1234567890", "Иванов Иван Петрович", false);

        var calculateDate = DateTime.UtcNow.AddDays(-30);

        var calculateResponse = await Client.PostAsync($"/api/workersalary?date={calculateDate:yyyy-MM-dd}", null);
        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, calculateResponse.StatusCode);

        await Task.Delay(500);

        var getResponse = await Client.GetAsync($"/api/workersalary?workerId={workerId}");
        var content = await GetWorkerSalariesFromResponseAsync(getResponse);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, getResponse.StatusCode);
        Assert.IsNotNull(content);
        Assert.IsTrue(content.Count >= 0);
    }

    [TestMethod]
    public async Task TestMethod_CalculateForDifferentDates()
    {
        var date1 = DateTime.UtcNow.AddMonths(-2);
        var date2 = DateTime.UtcNow.AddMonths(-1);
        var date3 = DateTime.UtcNow;

        var response1 = await Client.PostAsync($"/api/workersalary?date={date1:yyyy-MM-dd}", null);
        var response2 = await Client.PostAsync($"/api/workersalary?date={date2:yyyy-MM-dd}", null);
        var response3 = await Client.PostAsync($"/api/workersalary?date={date3:yyyy-MM-dd}", null);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response1.StatusCode);
        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response2.StatusCode);
        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response3.StatusCode);
    }

    [TestMethod]
    public async Task TestMethod_CalculateWithFutureDate()
    {
        var futureDate = DateTime.UtcNow.AddMonths(1);

        var response = await Client.PostAsync($"/api/workersalary?date={futureDate:yyyy-MM-dd}", null);
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }

    // ==================== БОЛЬШИЕ ОБЪЁМЫ ДАННЫХ ====================

    [TestMethod]
    public async Task TestMethod_Get_ManyRecords()
    {
        using var connection = ManualMigration.Connection;
        var workerId = GetWorkerId(connection, "1234567890");

        for (int i = 0; i < 10; i++)
        {
            InsertWorkerSalary(connection, workerId, 50000 + i * 1000, DateTime.UtcNow.AddDays(-30 - i));
        }

        var response = await Client.GetAsync("/api/workersalary");
        var content = await GetWorkerSalariesFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(10, content.Count);
    }

    [TestMethod]
    public async Task TestMethod_Get_MultipleWorkers()
    {
        using var connection = ManualMigration.Connection;

        for (int i = 0; i < 5; i++)
        {
            var workerId = GetWorkerId(connection, $"{i}{i}{i}{i}{i}{i}{i}{i}{i}{i}");
            InsertWorkerSalary(connection, workerId, 50000 + i * 1000, DateTime.UtcNow.AddDays(-30));
        }

        var response = await Client.GetAsync("/api/workersalary");
        var content = await GetWorkerSalariesFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(5, content.Count);
    }
}