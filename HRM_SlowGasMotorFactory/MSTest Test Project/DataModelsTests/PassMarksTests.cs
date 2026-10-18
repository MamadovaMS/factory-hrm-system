using HRM_SlowGasMotorFactory_DataModels.DataModels;
using System.ComponentModel.DataAnnotations;

namespace HRM_SlowGasMotorFactory_TestsMSTest.DataModelsTests;

[TestClass]
public sealed class PassMarksTests
{
    [TestMethod]
    public void TestMethod_ValidData()
    {
        var passMark = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            PassNumber = "A12",
            MarkDateTimeIn = DateTime.Today,
            MarkDateTimeOut = DateTime.Today
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(passMark, new ValidationContext(passMark), results, true);

        Assert.IsTrue(validate);
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void TestMethod_ValidDataWithoutMarkOut()
    {
        var passMark = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            PassNumber = "A12",
            MarkDateTimeIn = DateTime.Today,
            MarkDateTimeOut = null
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(passMark, new ValidationContext(passMark), results, true);

        Assert.IsTrue(validate);
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void TestMethod_EmptyId()
    {
        var passMark = new PassMarks
        {
            Id = Guid.Empty,
            WorkerId = Guid.NewGuid(),
            PassNumber = "A12",
            MarkDateTimeIn = DateTime.Today,
            MarkDateTimeOut = DateTime.Today
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(passMark, new ValidationContext(passMark), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("Id", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_EmptyWorkerId()
    {
        var passMark = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.Empty,
            PassNumber = "A12",
            MarkDateTimeIn = DateTime.Today,
            MarkDateTimeOut = DateTime.Today
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(passMark, new ValidationContext(passMark), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("WorkerId", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_PassNumber2Chars()
    {
        var passMark = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            PassNumber = "A1",
            MarkDateTimeIn = DateTime.Today,
            MarkDateTimeOut = DateTime.Today
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(passMark, new ValidationContext(passMark), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("PassNumber", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_PassNumber11Chars()
    {
        var passMark = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            PassNumber = "A1234567890",
            MarkDateTimeIn = DateTime.Today,
            MarkDateTimeOut = DateTime.Today
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(passMark, new ValidationContext(passMark), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("PassNumber", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_PassNumberExact3Chars()
    {
        var passMark = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            PassNumber = "A12",
            MarkDateTimeIn = DateTime.Today,
            MarkDateTimeOut = DateTime.Today
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(passMark, new ValidationContext(passMark), results, true);

        Assert.IsTrue(validate);
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void TestMethod_PassNumberExact10Chars()
    {
        var passMark = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            PassNumber = "A123456789",
            MarkDateTimeIn = DateTime.Today,
            MarkDateTimeOut = DateTime.Today
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(passMark, new ValidationContext(passMark), results, true);

        Assert.IsTrue(validate);
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void TestMethod_MarkDateTimeInYesterday()
    {
        var passMark = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            PassNumber = "A12",
            MarkDateTimeIn = DateTime.Today.AddDays(-1),
            MarkDateTimeOut = DateTime.Today
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(passMark, new ValidationContext(passMark), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("MarkDateTimeIn", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_MarkDateTimeInTomorrow()
    {
        var passMark = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            PassNumber = "A12",
            MarkDateTimeIn = DateTime.Today.AddDays(1),
            MarkDateTimeOut = DateTime.Today.AddDays(2)
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(passMark, new ValidationContext(passMark), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("MarkDateTimeIn", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_MarkDateTimeOutYesterday()
    {
        var passMark = new PassMarks
        {
            Id = Guid.NewGuid(),
            WorkerId = Guid.NewGuid(),
            PassNumber = "A12",
            MarkDateTimeIn = DateTime.Today,
            MarkDateTimeOut = DateTime.Today.AddDays(-1)
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(passMark, new ValidationContext(passMark), results, true);

        Assert.IsTrue(validate);
        Assert.AreEqual(0, results.Count);
    }
}