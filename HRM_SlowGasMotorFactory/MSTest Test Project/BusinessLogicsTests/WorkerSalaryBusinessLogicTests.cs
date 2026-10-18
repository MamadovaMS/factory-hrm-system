using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces.Implementations;
using HRM_SlowGasMotorFactory_DataModels.DataModels;
using HRM_SlowGasMotorFactory_DataModels.Enums;
using HRM_SlowGasMotorFactory_TestsMSTest.BusinessLogicsTests.DatabasesMocks;

namespace HRM_SlowGasMotorFactory_TestsMSTest.BusinessLogicsTests;

[TestClass]
public sealed class WorkerSalaryBusinessLogicTests
{
    private WorkerSalaryDatabaseMock _salaryDatabase = null!;
    private WorkerDatabaseMock _workerDatabase = null!;
    private PassMarksDatabaseMock _passMarksDatabase = null!;
    private SalesInfoClientMock _salesInfoClient = null!;
    private WorkerSalaryBusinessLogic _logic = null!;

    [TestInitialize]
    public void TestInit()
    {
        _salaryDatabase = new WorkerSalaryDatabaseMock();
        _workerDatabase = new WorkerDatabaseMock();
        _passMarksDatabase = new PassMarksDatabaseMock();
        _salesInfoClient = new SalesInfoClientMock();

        _logic = new WorkerSalaryBusinessLogic(
            _salaryDatabase,
            _workerDatabase,
            _passMarksDatabase,
            _salesInfoClient);
    }

    [TestMethod]
    public void TestMethod_Get_List()
    {
        var salaries = new List<WorkerSalary>
        {
            new() { Id = Guid.NewGuid(), WorkerId = Guid.NewGuid(), Salary = 50000, CalculateDate = DateTime.UtcNow.AddDays(-30) },
            new() { Id = Guid.NewGuid(), WorkerId = Guid.NewGuid(), Salary = 55000, CalculateDate = DateTime.UtcNow.AddDays(-30) }
        };
        _salaryDatabase.Values.AddRange(salaries);

        var result = _logic.Get();

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(1, _salaryDatabase.Calls["Read"]);
    }

    [TestMethod]
    public void TestMethod_Get_EmptyList()
    {
        var result = _logic.Get();
        Assert.AreEqual(0, result.Count);
        Assert.AreEqual(1, _salaryDatabase.Calls["Read"]);
    }

    [TestMethod]
    public void TestMethod_Get_ByWorkerId()
    {
        var workerId = Guid.NewGuid();
        var salaries = new List<WorkerSalary>
        {
            new() { Id = Guid.NewGuid(), WorkerId = workerId, Salary = 50000, CalculateDate = DateTime.UtcNow.AddDays(-30) },
            new() { Id = Guid.NewGuid(), WorkerId = Guid.NewGuid(), Salary = 55000, CalculateDate = DateTime.UtcNow.AddDays(-30) }
        };
        _salaryDatabase.Values.AddRange(salaries);

        var result = _logic.Get(workerId: workerId);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(workerId, result.First().WorkerId);
    }

    [TestMethod]
    public void TestMethod_CalculateSalary_ForOneWorker()
    {
        var date = DateTime.Today.AddDays(-1);
        var beforeMonthDate = date.AddMonths(-1);
        var startDate = new DateTime(beforeMonthDate.Year, beforeMonthDate.Month, 1);
        var finishDate = startDate.AddMonths(1).AddDays(-1);

        var post = new Post { Id = Guid.NewGuid(), PostName = "Инженер", PostType = PostType.Engineer, SalaryPerHour = 500 };
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            FIO = "Иванов Иван",
            PostId = post.Id,
            Post = post,
            IsDeleted = false
        };
        _workerDatabase.Values.Add(worker);

        var passMarks = new List<PassMarks>
        {
            new() { Id = Guid.NewGuid(), WorkerId = worker.Id, PassNumber = "A01", MarkDateTimeIn = startDate.AddHours(9), MarkDateTimeOut = startDate.AddHours(17) },
            new() { Id = Guid.NewGuid(), WorkerId = worker.Id, PassNumber = "A01", MarkDateTimeIn = startDate.AddDays(1).AddHours(9), MarkDateTimeOut = startDate.AddDays(1).AddHours(17) }
        };
        _passMarksDatabase.Values.AddRange(passMarks);

        var sales = new List<SaleInfo>
        {
            new() { CashierId = worker.Id, SaleDate = startDate.AddDays(5), Sum = 10000 },
            new() { CashierId = worker.Id, SaleDate = startDate.AddDays(10), Sum = 15000 }
        };
        _salesInfoClient.Values.AddRange(sales);

        _logic.CalculateSalary(date);

        Assert.AreEqual(1, _salaryDatabase.Calls["Create"]);
        Assert.AreEqual(1, _salaryDatabase.Values.Count);
        Assert.AreEqual(worker.Id, _salaryDatabase.Values.First().WorkerId);
    }

    [TestMethod]
    public void TestMethod_CalculateSalary_ForMultipleWorkers()
    {
        var date = DateTime.Today.AddDays(-1);
        var beforeMonthDate = date.AddMonths(-1);
        var startDate = new DateTime(beforeMonthDate.Year, beforeMonthDate.Month, 1);
        var finishDate = startDate.AddMonths(1).AddDays(-1);

        var workers = new List<Worker>
        {
            new() { Id = Guid.NewGuid(), PersonalNumber = "1111111111", FIO = "Работник 1", Post = new Post { SalaryPerHour = 500 }, IsDeleted = false },
            new() { Id = Guid.NewGuid(), PersonalNumber = "2222222222", FIO = "Работник 2", Post = new Post { SalaryPerHour = 600 }, IsDeleted = false },
            new() { Id = Guid.NewGuid(), PersonalNumber = "3333333333", FIO = "Работник 3", Post = new Post { SalaryPerHour = 700 }, IsDeleted = false }
        };
        _workerDatabase.Values.AddRange(workers);

        foreach (var worker in workers)
        {
            _passMarksDatabase.Values.Add(new PassMarks
            {
                Id = Guid.NewGuid(),
                WorkerId = worker.Id,
                PassNumber = "A01",
                MarkDateTimeIn = startDate.AddHours(9),
                MarkDateTimeOut = startDate.AddHours(17)
            });
        }

        _logic.CalculateSalary(date);
        Thread.Sleep(1000);

        Assert.AreEqual(3, _salaryDatabase.Calls["Create"]);
        Assert.AreEqual(3, _salaryDatabase.Values.Count);
    }
}