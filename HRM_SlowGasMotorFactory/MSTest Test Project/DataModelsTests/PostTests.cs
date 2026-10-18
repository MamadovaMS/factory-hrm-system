using HRM_SlowGasMotorFactory_DataModels.DataModels;
using HRM_SlowGasMotorFactory_DataModels.Enums;
using System.ComponentModel.DataAnnotations;

namespace HRM_SlowGasMotorFactory_TestsMSTest.DataModelsTests;

[TestClass]
public sealed class PostTests
{
    [TestMethod]
    public void TestMethod_ValidData()
    {
        var id = Guid.NewGuid();
        var postName = "Инженер завода";
        var postType = PostType.Engineer;
        var salary = 500.0;
        var post = new Post()
        {
            Id = id,
            PostName = postName,
            PostType = postType,
            SalaryPerHour = salary
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(post, new ValidationContext(post), results, true);

        Assert.IsTrue(validate);
        Assert.AreEqual(0, results.Count);
        Assert.AreEqual(id, post.Id);
        Assert.AreEqual(postName, post.PostName);
        Assert.AreEqual(postType, post.PostType);
        Assert.AreEqual(salary, post.SalaryPerHour);
    }

    [TestMethod]
    public void TestMethod_IdIsEmpty()
    {
        var post = new Post()
        {
            Id = Guid.Empty,
            PostName = "Инженер завода",
            PostType = PostType.Engineer,
            SalaryPerHour = 500
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(post, new ValidationContext(post), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("Id", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_PostNameIsNull()
    {
        var post = new Post()
        {
            Id = Guid.NewGuid(),
            PostName = null!,
            PostType = PostType.Engineer,
            SalaryPerHour = 500
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(post, new ValidationContext(post), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("PostName", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_PostNameIsEmpty()
    {
        var post = new Post()
        {
            Id = Guid.NewGuid(),
            PostName = string.Empty,
            PostType = PostType.Engineer,
            SalaryPerHour = 500
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(post, new ValidationContext(post), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("PostName", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_PostNameIsShort()
    {
        var post = new Post()
        {
            Id = Guid.NewGuid(),
            PostName = "Коротк",
            PostType = PostType.Engineer,
            SalaryPerHour = 500
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(post, new ValidationContext(post), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("PostName", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_PostTypeIsNone()
    {
        var post = new Post()
        {
            Id = Guid.NewGuid(),
            PostName = "Инженер завода",
            PostType = PostType.None,
            SalaryPerHour = 500
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(post, new ValidationContext(post), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("PostType", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_SalaryIsZero()
    {
        var post = new Post()
        {
            Id = Guid.NewGuid(),
            PostName = "Инженер завода",
            PostType = PostType.Engineer,
            SalaryPerHour = 0
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(post, new ValidationContext(post), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("SalaryPerHour", results.First().MemberNames.First());
    }

    [TestMethod]
    public void TestMethod_SalaryIsNegative()
    {
        var post = new Post()
        {
            Id = Guid.NewGuid(),
            PostName = "Инженер завода",
            PostType = PostType.Engineer,
            SalaryPerHour = -100
        };

        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(post, new ValidationContext(post), results, true);

        Assert.IsFalse(validate);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("SalaryPerHour", results.First().MemberNames.First());
    }
}