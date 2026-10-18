using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces.Implementations;
using HRM_SlowGasMotorFactory_DataModels.DataModels;
using HRM_SlowGasMotorFactory_TestsMSTest.BusinessLogicsTests.DatabasesMocks;

namespace HRM_SlowGasMotorFactory_TestsMSTest.BusinessLogicsTests;

[TestClass]
public sealed class PassMarksBusinessLogicTests
{
    private PassMarksDatabaseMock _database = null!;
    private PassMarksBusinessLogic _logic = null!;

    [TestInitialize]
    public void TestInit()
    {
        _database = new PassMarksDatabaseMock();
        _logic = new PassMarksBusinessLogic(_database);
    }

    [TestMethod]
    public void TestMethod_Get_List()
    {
        // Arrange
        var workerId = Guid.NewGuid();
        var passMarks = new List<PassMarks>
            {
                new() { Id = Guid.NewGuid(), WorkerId = workerId, PassNumber = "A12",
                       MarkDateTimeIn = DateTime.Today, MarkDateTimeOut = null },
                new() { Id = Guid.NewGuid(), WorkerId = workerId, PassNumber = "A12",
                       MarkDateTimeIn = DateTime.Today.AddDays(-1), MarkDateTimeOut = DateTime.Today.AddDays(-1) },
                new() { Id = Guid.NewGuid(), WorkerId = Guid.NewGuid(), PassNumber = "B34",
                       MarkDateTimeIn = DateTime.Today, MarkDateTimeOut = DateTime.Today }
            };
        _database.Values.AddRange(passMarks);

        // Act
        var result = _logic.Get();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count);
        Assert.IsTrue(result.Any(x => x.Id == passMarks[0].Id));
        Assert.IsTrue(result.Any(x => x.Id == passMarks[1].Id));
        Assert.IsTrue(result.Any(x => x.Id == passMarks[2].Id));
        Assert.AreEqual(1, _database.Calls["Read"]);
    }

    [TestMethod]
    public void TestMethod_Get_EmptyList()
    {
        // Act
        var result = _logic.Get();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
        Assert.AreEqual(1, _database.Calls["Read"]);
    }

    [TestMethod]
    public void TestMethod_Get_ByWorkerId()
    {
        // Arrange
        var workerId = Guid.NewGuid();
        var passMarks = new List<PassMarks>
            {
                new() { Id = Guid.NewGuid(), WorkerId = workerId, PassNumber = "A12",
                       MarkDateTimeIn = DateTime.Today, MarkDateTimeOut = null },
                new() { Id = Guid.NewGuid(), WorkerId = workerId, PassNumber = "A12",
                       MarkDateTimeIn = DateTime.Today.AddDays(-1), MarkDateTimeOut = DateTime.Today.AddDays(-1) },
                new() { Id = Guid.NewGuid(), WorkerId = Guid.NewGuid(), PassNumber = "B34",
                       MarkDateTimeIn = DateTime.Today, MarkDateTimeOut = DateTime.Today }
            };
        _database.Values.AddRange(passMarks);

        // Act
        var result = _logic.Get(workerId: workerId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(x => x.Id == passMarks[0].Id));
        Assert.IsTrue(result.Any(x => x.Id == passMarks[1].Id));
        Assert.AreEqual(1, _database.Calls["Read"]);
    }

    [TestMethod]
    public void TestMethod_Get_ByPassNumber()
    {
        // Arrange
        var passMarks = new List<PassMarks>
            {
                new() { Id = Guid.NewGuid(), WorkerId = Guid.NewGuid(), PassNumber = "A12",
                       MarkDateTimeIn = DateTime.Today, MarkDateTimeOut = null },
                new() { Id = Guid.NewGuid(), WorkerId = Guid.NewGuid(), PassNumber = "A12",
                       MarkDateTimeIn = DateTime.Today.AddDays(-1), MarkDateTimeOut = DateTime.Today.AddDays(-1) },
                new() { Id = Guid.NewGuid(), WorkerId = Guid.NewGuid(), PassNumber = "B34",
                       MarkDateTimeIn = DateTime.Today, MarkDateTimeOut = DateTime.Today }
            };
        _database.Values.AddRange(passMarks);

        // Act
        var result = _logic.Get(passNumber: "A12");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(x => x.Id == passMarks[0].Id));
        Assert.IsTrue(result.Any(x => x.Id == passMarks[1].Id));
        Assert.AreEqual(1, _database.Calls["Read"]);
    }

    [TestMethod]
    public void TestMethod_Get_ByDateRange()
    {
        // Arrange
        var passMarks = new List<PassMarks>
            {
                new() { Id = Guid.NewGuid(), WorkerId = Guid.NewGuid(), PassNumber = "A12",
                       MarkDateTimeIn = DateTime.Today, MarkDateTimeOut = null },
                new() { Id = Guid.NewGuid(), WorkerId = Guid.NewGuid(), PassNumber = "A12",
                       MarkDateTimeIn = DateTime.Today.AddDays(-2), MarkDateTimeOut = DateTime.Today.AddDays(-2) },
                new() { Id = Guid.NewGuid(), WorkerId = Guid.NewGuid(), PassNumber = "B34",
                       MarkDateTimeIn = DateTime.Today.AddDays(-1), MarkDateTimeOut = DateTime.Today.AddDays(-1) }
            };
        _database.Values.AddRange(passMarks);

        // Act
        var result = _logic.Get(dateFrom: DateTime.Today.AddDays(-1), dateTo: DateTime.Today);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(x => x.Id == passMarks[0].Id));
        Assert.IsTrue(result.Any(x => x.Id == passMarks[2].Id));
        Assert.AreEqual(1, _database.Calls["Read"]);
    }

    [TestMethod]
    public void TestMethod_WorkerIn()
    {
        // Arrange
        var passMarks = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            PassNumber = "A12",
            MarkDateTimeIn = DateTime.Today,
            MarkDateTimeOut = null
        };

        // Act
        var result = _logic.WorkerIn(passMarks);
        var element = _database.Values.FirstOrDefault(x => x.Id == passMarks.Id);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(element);
        Assert.AreEqual(passMarks.WorkerId, element.WorkerId);
        Assert.AreEqual(passMarks.PassNumber, element.PassNumber);
        Assert.AreEqual(1, _database.Calls["Create"]);
    }

    [TestMethod]
    public void TestMethod_WorkerIn_AlreadyActive()
    {
        // Arrange
        var workerId = Guid.NewGuid();
        var passNumber = "A12";

        var activePassMarks = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = workerId,
            PassNumber = passNumber,
            MarkDateTimeIn = DateTime.Today.AddHours(-2),
            MarkDateTimeOut = null
        };
        _database.Values.Add(activePassMarks);

        var newPassMarks = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = workerId,
            PassNumber = passNumber,
            MarkDateTimeIn = DateTime.Today,
            MarkDateTimeOut = null
        };

        // Act
        var result = _logic.WorkerIn(newPassMarks);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual("Сотрудник уже на рабочем месте", _logic.ErrorMessage);
        Assert.AreEqual(1, _database.Calls["Read"]);
        Assert.AreEqual(0, _database.Calls["Create"]);
    }

    [TestMethod]
    public void TestMethod_WorkerIn_InvalidData()
    {
        // Arrange
        var passMarks = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            PassNumber = "A",
            MarkDateTimeIn = DateTime.Today,
            MarkDateTimeOut = null
        };

        // Act
        var result = _logic.WorkerIn(passMarks);

        // Assert
        Assert.IsFalse(result);
        Assert.IsTrue(!string.IsNullOrEmpty(_logic.ErrorMessage));
        Assert.AreEqual(0, _database.Calls["Read"]);
        Assert.AreEqual(0, _database.Calls["Create"]);
    }

    [TestMethod]
    public void TestMethod_WorkerOut()
    {
        // Arrange
        var workerId = Guid.NewGuid();
        var passNumber = "A12";

        var activePassMarks = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = workerId,
            PassNumber = passNumber,
            MarkDateTimeIn = DateTime.Today,
            MarkDateTimeOut = null
        };
        _database.Values.Add(activePassMarks);

        var outPassMarks = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = workerId,
            PassNumber = passNumber,
            MarkDateTimeIn = activePassMarks.MarkDateTimeIn,
            MarkDateTimeOut = DateTime.Today
        };

        // Act
        var result = _logic.WorkerOut(outPassMarks);
        var element = _database.Values.FirstOrDefault(x => x.Id == activePassMarks.Id);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(element);
        Assert.AreEqual(DateTime.Today, element.MarkDateTimeOut);
        Assert.AreEqual(1, _database.Calls["Read"]);
        Assert.AreEqual(1, _database.Calls["Update"]);
    }

    [TestMethod]
    public void TestMethod_WorkerOut_NoActiveEntry()
    {
        // Arrange
        var workerId = Guid.NewGuid();

        var outPassMarks = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = workerId,
            PassNumber = "A12",
            MarkDateTimeIn = DateTime.Today,
            MarkDateTimeOut = DateTime.Today
        };

        // Act
        var result = _logic.WorkerOut(outPassMarks);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual("Нет активной записи входа", _logic.ErrorMessage);
        Assert.AreEqual(1, _database.Calls["Read"]);
        Assert.AreEqual(0, _database.Calls["Update"]);
    }

    [TestMethod]
    public void TestMethod_WorkerOut_InvalidData()
    {
        // Arrange
        var outPassMarks = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            PassNumber = "A",
            MarkDateTimeIn = DateTime.Today.AddHours(-8),
            MarkDateTimeOut = DateTime.Today
        };

        // Act
        var result = _logic.WorkerOut(outPassMarks);

        // Assert
        Assert.IsFalse(result);
        Assert.IsTrue(!string.IsNullOrEmpty(_logic.ErrorMessage));
        Assert.AreEqual(0, _database.Calls["Read"]);
        Assert.AreEqual(0, _database.Calls["Update"]);
    }
}