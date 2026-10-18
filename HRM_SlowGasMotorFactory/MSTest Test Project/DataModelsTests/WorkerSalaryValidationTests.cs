using HRM_SlowGasMotorFactory_DataModels.DataModels;
using System.ComponentModel.DataAnnotations;

namespace HRM_SlowGasMotorFactory_TestsMSTest.DataModelsTests;

[TestClass]
public sealed class WorkerSalaryValidationTests
{
    [TestMethod]
    public void TestMethod_ValidData()
    {
        var workerSalary = new WorkerSalary()
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            Salary = 50000,
            CalculateDate = DateTime.Today.AddDays(-30)
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(workerSalary, new ValidationContext(workerSalary), results, true);

        Assert.IsTrue(validate);
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void TestMethod_IdIsEmpty()
    {
        var workerSalary = new WorkerSalary()
        {
            Id = Guid.Empty,
            WorkerId = Guid.NewGuid(),
            Salary = 50000,
            CalculateDate = DateTime.Today.AddDays(-30)
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(workerSalary, new ValidationContext(workerSalary), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("Id", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_WorkerIdIsEmpty()
    {
        var workerSalary = new WorkerSalary()
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.Empty,
            Salary = 50000,
            CalculateDate = DateTime.Today.AddDays(-30)
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(workerSalary, new ValidationContext(workerSalary), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("WorkerId", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_SalaryIsZero()
    {
        var workerSalary = new WorkerSalary()
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            Salary = 0,
            CalculateDate = DateTime.Today.AddDays(-30)
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(workerSalary, new ValidationContext(workerSalary), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("Salary", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_SalaryIsNegative()
    {
        var workerSalary = new WorkerSalary()
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            Salary = -100,
            CalculateDate = DateTime.Today.AddDays(-30)
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(workerSalary, new ValidationContext(workerSalary), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("Salary", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_CalculateDateIsFuture()
    {
        var workerSalary = new WorkerSalary()
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            Salary = 50000,
            CalculateDate = DateTime.Today.AddDays(30)
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(workerSalary, new ValidationContext(workerSalary), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("CalculateDate", results.First().MemberNames.First());
    }
}