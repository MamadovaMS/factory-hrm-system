using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces.Implementations;
using HRM_SlowGasMotorFactory_DataModels.DataModels;
using HRM_SlowGasMotorFactory_TestsMSTest.BusinessLogicsTests.DatabasesMocks;

namespace HRM_SlowGasMotorFactory_TestsMSTest.BusinessLogicsTests;

[TestClass]
public sealed class WorkerBusinessLogicTests
{
    private WorkerDatabaseMock _database = null!;
    private WorkerBusinessLogic _logic = null!;

    [TestInitialize]
    public void TestInit()
    {
        _database = new WorkerDatabaseMock();
        _logic = new WorkerBusinessLogic(_database);
    }

    [TestMethod]
    public void TestMethod_Get_List()
    {
        // Arrange
        var workers = new List<Worker>
            {
                new() { Id = Guid.NewGuid(), PersonalNumber = "1234567890", FIO = "Иванов Иван",
                       PostId = Guid.NewGuid(), BirthDate = new DateTime(1990,1,1),
                       EmploymentDate = DateTime.Today.AddDays(-100) },
                new() { Id = Guid.NewGuid(), PersonalNumber = "2234567890", FIO = "Петров Петр",
                       PostId = Guid.NewGuid(), BirthDate = new DateTime(1985,5,10),
                       EmploymentDate = DateTime.Today.AddDays(-200) },
                new() { Id = Guid.NewGuid(), PersonalNumber = "3234567890", FIO = "Сидоров Сидор",
                       PostId = Guid.NewGuid(), BirthDate = new DateTime(1995,3,15),
                       EmploymentDate = DateTime.Today.AddDays(-50) }
            };
        _database.Values.AddRange(workers);

        // Act
        var result = _logic.Get();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count);
        Assert.IsTrue(result.Any(x => x.Id == workers[0].Id));
        Assert.IsTrue(result.Any(x => x.Id == workers[1].Id));
        Assert.IsTrue(result.Any(x => x.Id == workers[2].Id));
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
    public void TestMethod_Get_ByPersonalNumber()
    {
        // Arrange
        var workers = new List<Worker>
            {
                new() { Id = Guid.NewGuid(), PersonalNumber = "1234567890", FIO = "Иванов Иван",
                       PostId = Guid.NewGuid(), BirthDate = new DateTime(1990,1,1),
                       EmploymentDate = DateTime.Today.AddDays(-100) },
                new() { Id = Guid.NewGuid(), PersonalNumber = "2234567890", FIO = "Петров Петр",
                       PostId = Guid.NewGuid(), BirthDate = new DateTime(1985,5,10),
                       EmploymentDate = DateTime.Today.AddDays(-200) }
            };
        _database.Values.AddRange(workers);

        // Act
        var result = _logic.Get(personalNumber: "1234567890");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(workers[0].Id, result.First().Id);
        Assert.AreEqual(1, _database.Calls["Read"]);
    }

    [TestMethod]
    public void TestMethod_Get_ByFIO()
    {
        // Arrange
        var workers = new List<Worker>
            {
                new() { Id = Guid.NewGuid(), PersonalNumber = "1234567890", FIO = "Иванов Иван",
                       PostId = Guid.NewGuid(), BirthDate = new DateTime(1990,1,1),
                       EmploymentDate = DateTime.Today.AddDays(-100) },
                new() { Id = Guid.NewGuid(), PersonalNumber = "2234567890", FIO = "Петров Петр",
                       PostId = Guid.NewGuid(), BirthDate = new DateTime(1985,5,10),
                       EmploymentDate = DateTime.Today.AddDays(-200) }
            };
        _database.Values.AddRange(workers);

        // Act
        var result = _logic.Get(fio: "Петров Петр");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(workers[1].Id, result.First().Id);
        Assert.AreEqual(1, _database.Calls["Read"]);
    }

    [TestMethod]
    public void TestMethod_Get_ByPostId()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var workers = new List<Worker>
            {
                new() { Id = Guid.NewGuid(), PersonalNumber = "1234567890", FIO = "Иванов Иван",
                       PostId = postId, BirthDate = new DateTime(1990,1,1),
                       EmploymentDate = DateTime.Today.AddDays(-100) },
                new() { Id = Guid.NewGuid(), PersonalNumber = "2234567890", FIO = "Петров Петр",
                       PostId = Guid.NewGuid(), BirthDate = new DateTime(1985,5,10),
                       EmploymentDate = DateTime.Today.AddDays(-200) }
            };
        _database.Values.AddRange(workers);

        // Act
        var result = _logic.Get(postId: postId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(workers[0].Id, result.First().Id);
        Assert.AreEqual(1, _database.Calls["Read"]);
    }

    [TestMethod]
    public void TestMethod_Get_ByBirthDateRange()
    {
        // Arrange
        var workers = new List<Worker>
            {
                new() { Id = Guid.NewGuid(), PersonalNumber = "1234567890", FIO = "Иванов Иван",
                       PostId = Guid.NewGuid(), BirthDate = new DateTime(1990,1,1),
                       EmploymentDate = DateTime.Today.AddDays(-100) },
                new() { Id = Guid.NewGuid(), PersonalNumber = "2234567890", FIO = "Петров Петр",
                       PostId = Guid.NewGuid(), BirthDate = new DateTime(1985,5,10),
                       EmploymentDate = DateTime.Today.AddDays(-200) },
                new() { Id = Guid.NewGuid(), PersonalNumber = "3234567890", FIO = "Сидоров Сидор",
                       PostId = Guid.NewGuid(), BirthDate = new DateTime(1995,3,15),
                       EmploymentDate = DateTime.Today.AddDays(-50) }
            };
        _database.Values.AddRange(workers);

        // Act
        var result = _logic.Get(birthDateStart: new DateTime(1988, 1, 1),
                                birthDateFinish: new DateTime(1992, 12, 31));

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(workers[0].Id, result.First().Id);
        Assert.AreEqual(1, _database.Calls["Read"]);
    }

    [TestMethod]
    public void TestMethod_Get_ById()
    {
        // Arrange
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            FIO = "Иванов Иван",
            PostId = Guid.NewGuid(),
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-100)
        };
        _database.Values.Add(worker);

        // Act
        var result = _logic.Get(worker.Id);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(worker.Id, result.Id);
        Assert.AreEqual(worker.PersonalNumber, result.PersonalNumber);
        Assert.AreEqual(worker.FIO, result.FIO);
        Assert.AreEqual(1, _database.Calls["ReadSingle"]);
    }

    [TestMethod]
    public void TestMethod_Get_ById_WrongId()
    {
        // Arrange
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            FIO = "Иванов Иван",
            PostId = Guid.NewGuid(),
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-100)
        };
        _database.Values.Add(worker);

        // Act
        var result = _logic.Get(Guid.NewGuid());

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual("Сотрудник не найден", _logic.ErrorMessage);
        Assert.AreEqual(1, _database.Calls["ReadSingle"]);
    }

    [TestMethod]
    public void TestMethod_GetWorkerPreviousPosts()
    {
        // Arrange
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            FIO = "Иванов Иван",
            PostId = Guid.NewGuid(),
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-100)
        };
        _database.Values.Add(worker);

        // Act
        var result = _logic.GetWorkerPreviousPosts(worker.Id);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(1, _database.Calls["ReadWorkerPreviousPosts"]);
    }

    [TestMethod]
    public void TestMethod_GetWorkerPreviousPosts_WrongId()
    {
        // Act
        var result = _logic.GetWorkerPreviousPosts(Guid.NewGuid());

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
        Assert.AreEqual("Сотрудник не найден", _logic.ErrorMessage);
        Assert.AreEqual(0, _database.Calls["ReadWorkerPreviousPosts"]);
    }

    [TestMethod]
    public void TestMethod_Add()
    {
        // Arrange
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            FIO = "Иванов Иван",
            PostId = Guid.NewGuid(),
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-100)
        };

        // Act
        var result = _logic.Add(worker);
        var element = _database.Values.FirstOrDefault(x => x.Id == worker.Id);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(element);
        Assert.AreEqual(worker.FIO, element.FIO);
        Assert.AreEqual(worker.PersonalNumber, element.PersonalNumber);
        Assert.AreEqual(1, _database.Calls["Read"]);
        Assert.AreEqual(1, _database.Calls["Create"]);
    }

    [TestMethod]
    public void TestMethod_Add_DuplicatePersonalNumber()
    {
        // Arrange
        var worker1 = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            FIO = "Иванов Иван",
            PostId = Guid.NewGuid(),
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-100)
        };
        _database.Values.Add(worker1);

        var worker2 = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            FIO = "Петров Петр",
            PostId = Guid.NewGuid(),
            BirthDate = new DateTime(1985, 5, 10),
            EmploymentDate = DateTime.Today.AddDays(-200)
        };

        // Act
        var result = _logic.Add(worker2);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual("Сотрудник с таким табельным номером уже существует", _logic.ErrorMessage);
        Assert.AreEqual(1, _database.Calls["Read"]);
        Assert.AreEqual(0, _database.Calls["Create"]);
    }

    [TestMethod]
    public void TestMethod_Add_InvalidData()
    {
        // Arrange
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "123",
            FIO = "Иванов Иван",
            PostId = Guid.NewGuid(),
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-100)
        };

        // Act
        var result = _logic.Add(worker);

        // Assert
        Assert.IsFalse(result);
        Assert.IsTrue(!string.IsNullOrEmpty(_logic.ErrorMessage));
        Assert.AreEqual(0, _database.Calls["Read"]);
        Assert.AreEqual(0, _database.Calls["Create"]);
    }

    [TestMethod]
    public void TestMethod_Update()
    {
        // Arrange
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            FIO = "Иванов Иван",
            PostId = Guid.NewGuid(),
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-100)
        };
        _database.Values.Add(worker);

        var updatedWorker = new Worker
        {
            Id = worker.Id,
            PersonalNumber = "1234567890",
            FIO = "Иванов Иван Петрович",
            PostId = worker.PostId,
            BirthDate = worker.BirthDate,
            EmploymentDate = worker.EmploymentDate
        };

        // Act
        var result = _logic.Update(updatedWorker);
        var element = _database.Values.FirstOrDefault(x => x.Id == worker.Id);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(element);
        Assert.AreEqual("Иванов Иван Петрович", element.FIO);
        Assert.AreEqual(1, _database.Calls["ReadSingle"]);
        Assert.AreEqual(1, _database.Calls["Update"]);
    }

    [TestMethod]
    public void TestMethod_Update_WrongId()
    {
        // Arrange
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            FIO = "Иванов Иван",
            PostId = Guid.NewGuid(),
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-100)
        };
        _database.Values.Add(worker);

        var updatedWorker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            FIO = "Иванов Иван Петрович",
            PostId = worker.PostId,
            BirthDate = worker.BirthDate,
            EmploymentDate = worker.EmploymentDate
        };

        // Act
        var result = _logic.Update(updatedWorker);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual("Сотрудник не найден", _logic.ErrorMessage);
        Assert.AreEqual(1, _database.Calls["ReadSingle"]);
        Assert.AreEqual(0, _database.Calls["Update"]);
    }

    [TestMethod]
    public void TestMethod_Update_DuplicatePersonalNumber()
    {
        // Arrange
        var worker1 = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            FIO = "Иванов Иван",
            PostId = Guid.NewGuid(),
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-100)
        };

        var worker2 = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "2234567890",
            FIO = "Петров Петр",
            PostId = Guid.NewGuid(),
            BirthDate = new DateTime(1985, 5, 10),
            EmploymentDate = DateTime.Today.AddDays(-200)
        };

        _database.Values.Add(worker1);
        _database.Values.Add(worker2);

        var updatedWorker = new Worker
        {
            Id = worker2.Id,
            PersonalNumber = "1234567890",
            FIO = "Петров Петр Петрович",
            PostId = worker2.PostId,
            BirthDate = worker2.BirthDate,
            EmploymentDate = worker2.EmploymentDate
        };

        // Act
        var result = _logic.Update(updatedWorker);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual("Сотрудник с таким табельным номером уже существует", _logic.ErrorMessage);
        Assert.AreEqual(1, _database.Calls["Read"]);
        Assert.AreEqual(1, _database.Calls["ReadSingle"]);
        Assert.AreEqual(0, _database.Calls["Update"]);
    }

    [TestMethod]
    public void TestMethod_Delete()
    {
        // Arrange
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            FIO = "Иванов Иван",
            PostId = Guid.NewGuid(),
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-100)
        };
        _database.Values.Add(worker);

        // Act
        var result = _logic.Delete(worker.Id);
        var element = _database.Values.FirstOrDefault(x => x.Id == worker.Id);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(element);
        Assert.IsTrue(element.IsDeleted);
        Assert.AreEqual(1, _database.Calls["ReadSingle"]);
        Assert.AreEqual(1, _database.Calls["Delete"]);
    }

    [TestMethod]
    public void TestMethod_Delete_WrongId()
    {
        // Arrange
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            FIO = "Иванов Иван",
            PostId = Guid.NewGuid(),
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-100)
        };
        _database.Values.Add(worker);

        // Act
        var result = _logic.Delete(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual("Сотрудник не найден", _logic.ErrorMessage);
        Assert.AreEqual(1, _database.Calls["ReadSingle"]);
        Assert.AreEqual(0, _database.Calls["Delete"]);
    }

    [TestMethod]
    public void TestMethod_Update_DeletedWorker_ShouldFail()
    {
        // Arrange
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            FIO = "Иванов Иван",
            PostId = Guid.NewGuid(),
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-100),
            IsDeleted = true
        };
        _database.Values.Add(worker);

        var updatedWorker = new Worker
        {
            Id = worker.Id,
            PersonalNumber = "1234567890",
            FIO = "Иванов Иван Петрович",
            PostId = worker.PostId,
            BirthDate = worker.BirthDate,
            EmploymentDate = worker.EmploymentDate,
            IsDeleted = true
        };

        // Act
        var result = _logic.Update(updatedWorker);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual("Нельзя изменить удалённого сотрудника", _logic.ErrorMessage);
        Assert.AreEqual(1, _database.Calls["ReadSingle"]);
        Assert.AreEqual(0, _database.Calls["Update"]);
    }

    [TestMethod]
    public void TestMethod_Delete_AlreadyDeletedWorker_ShouldFail()
    {
        // Arrange
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            FIO = "Иванов Иван",
            PostId = Guid.NewGuid(),
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-100),
            IsDeleted = true
        };
        _database.Values.Add(worker);

        // Act
        var result = _logic.Delete(worker.Id);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual("Сотрудник уже удалён", _logic.ErrorMessage);
        Assert.AreEqual(1, _database.Calls["ReadSingle"]);
        Assert.AreEqual(0, _database.Calls["Delete"]);
    }

    [TestMethod]
    public void TestMethod_Update_DeletedWorkerWithDifferentId_ShouldFail()
    {
        // Arrange
        var activeWorker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            FIO = "Иванов Иван",
            PostId = Guid.NewGuid(),
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-100),
            IsDeleted = false
        };

        var deletedWorker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "2234567890",
            FIO = "Петров Петр",
            PostId = Guid.NewGuid(),
            BirthDate = new DateTime(1985, 5, 10),
            EmploymentDate = DateTime.Today.AddDays(-200),
            IsDeleted = true
        };

        _database.Values.Add(activeWorker);
        _database.Values.Add(deletedWorker);

        var updatedWorker = new Worker
        {
            Id = deletedWorker.Id,
            PersonalNumber = "2234567890",
            FIO = "Петров Петр Петрович",
            PostId = deletedWorker.PostId,
            BirthDate = deletedWorker.BirthDate,
            EmploymentDate = deletedWorker.EmploymentDate,
            IsDeleted = true
        };

        // Act
        var result = _logic.Update(updatedWorker);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual("Нельзя изменить удалённого сотрудника", _logic.ErrorMessage);
        Assert.AreEqual(1, _database.Calls["ReadSingle"]);
        Assert.AreEqual(0, _database.Calls["Update"]);
    }
}