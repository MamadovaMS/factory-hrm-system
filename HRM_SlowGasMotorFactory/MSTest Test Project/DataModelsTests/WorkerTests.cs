using HRM_SlowGasMotorFactory_DataModels.DataModels;
using System.ComponentModel.DataAnnotations;

namespace HRM_SlowGasMotorFactory_TestsMSTest.DataModelsTests;

[TestClass]
public sealed class WorkerTests
{
    [TestMethod]
    public void TestMethod_ValidData()
    {
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            PostId = Guid.NewGuid(),
            FIO = "Иванов Иван",
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-1)
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(worker, new ValidationContext(worker), results, true);

        Assert.IsTrue(validate);
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void TestMethod_EmptyId()
    {
        var worker = new Worker
        {
            Id = Guid.Empty,
            PersonalNumber = "1234567890",
            PostId = Guid.NewGuid(),
            FIO = "Иванов Иван",
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-1)
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(worker, new ValidationContext(worker), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("Id", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_PersonalNumber9Chars()
    {
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "123456789",
            PostId = Guid.NewGuid(),
            FIO = "Иванов Иван",
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-1)
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(worker, new ValidationContext(worker), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("PersonalNumber", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_PersonalNumber11Chars()
    {
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "12345678901",
            PostId = Guid.NewGuid(),
            FIO = "Иванов Иван",
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-1)
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(worker, new ValidationContext(worker), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("PersonalNumber", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_PersonalNumberExact10Chars()
    {
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            PostId = Guid.NewGuid(),
            FIO = "Иванов Иван",
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-1)
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(worker, new ValidationContext(worker), results, true);

        Assert.IsTrue(validate);
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void TestMethod_FIOWrongFormatOneWord()
    {
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            PostId = Guid.NewGuid(),
            FIO = "Иванов",
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-1)
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(worker, new ValidationContext(worker), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("FIO", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_FIOWrongFormatLowerCase()
    {
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            PostId = Guid.NewGuid(),
            FIO = "иванов иван",
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-1)
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(worker, new ValidationContext(worker), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("FIO", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_FIOWithPatronymic()
    {
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            PostId = Guid.NewGuid(),
            FIO = "Иванов Иван Петрович",
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(-1)
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(worker, new ValidationContext(worker), results, true);

        Assert.IsTrue(validate);
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void TestMethod_BirthDate15YearsOld()
    {
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            PostId = Guid.NewGuid(),
            FIO = "Иванов Иван",
            BirthDate = DateTime.Today.AddYears(-15),
            EmploymentDate = DateTime.Today.AddDays(-1)
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(worker, new ValidationContext(worker), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("BirthDate", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_BirthDate16YearsOld()
    {
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            PostId = Guid.NewGuid(),
            FIO = "Иванов Иван",
            BirthDate = DateTime.Today.AddYears(-16),
            EmploymentDate = DateTime.Today.AddDays(-1)
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(worker, new ValidationContext(worker), results, true);

        Assert.IsTrue(validate);
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void TestMethod_EmploymentDateTomorrow()
    {
        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "1234567890",
            PostId = Guid.NewGuid(),
            FIO = "Иванов Иван",
            BirthDate = new DateTime(1990, 1, 1),
            EmploymentDate = DateTime.Today.AddDays(1)
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(worker, new ValidationContext(worker), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("EmploymentDate", results.First().MemberNames.First());
    }
}