using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces.Implementations;
using HRM_SlowGasMotorFactory_DataModels.DataModels;
using HRM_SlowGasMotorFactory_DataModels.Enums;
using HRM_SlowGasMotorFactory_TestsMSTest.BusinessLogicsTests.DatabasesMocks;

namespace HRM_SlowGasMotorFactory_TestsMSTest.BusinessLogicsTests;

[TestClass]
public sealed class PostBusinessLogicTests
{
    private PostDatabaseMock _database = null!;
    private PostBusinessLogic _logic = null!;

    [TestInitialize]
    public void TestInit()
    {
        _database = new PostDatabaseMock();
        _logic = new PostBusinessLogic(_database);
    }

    [TestMethod]
    public void TestMethod_Get_List()
    {
        var posts = new List<Post>
        {
            new() { Id = Guid.NewGuid(), PostName = "Инженер", PostType = PostType.Engineer, SalaryPerHour = 500 },
            new() { Id = Guid.NewGuid(), PostName = "Сборщик", PostType = PostType.Assembler, SalaryPerHour = 300 },
            new() { Id = Guid.NewGuid(), PostName = "Логист", PostType = PostType.Logistician, SalaryPerHour = 400 }
        };
        _database.Values.AddRange(posts);

        var result = _logic.Get();

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(1, _database.Calls["Read"]);
    }

    [TestMethod]
    public void TestMethod_Get_EmptyList()
    {
        var result = _logic.Get();
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
        Assert.AreEqual(1, _database.Calls["Read"]);
    }

    [TestMethod]
    public void TestMethod_Get_ByPostName()
    {
        var posts = new List<Post>
        {
            new() { Id = Guid.NewGuid(), PostName = "Инженер", PostType = PostType.Engineer, SalaryPerHour = 500 },
            new() { Id = Guid.NewGuid(), PostName = "Сборщик", PostType = PostType.Assembler, SalaryPerHour = 300 }
        };
        _database.Values.AddRange(posts);

        var result = _logic.Get(postName: "Инженер");

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, _database.Calls["Read"]);
    }

    [TestMethod]
    public void TestMethod_Get_ByPostType()
    {
        var posts = new List<Post>
        {
            new() { Id = Guid.NewGuid(), PostName = "Инженер", PostType = PostType.Engineer, SalaryPerHour = 500 },
            new() { Id = Guid.NewGuid(), PostName = "Сборщик", PostType = PostType.Assembler, SalaryPerHour = 300 }
        };
        _database.Values.AddRange(posts);

        var result = _logic.Get(postType: PostType.Assembler);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, _database.Calls["Read"]);
    }

    [TestMethod]
    public void TestMethod_Get_BySalaryMin()
    {
        var posts = new List<Post>
        {
            new() { Id = Guid.NewGuid(), PostName = "Инженер", PostType = PostType.Engineer, SalaryPerHour = 500 },
            new() { Id = Guid.NewGuid(), PostName = "Сборщик", PostType = PostType.Assembler, SalaryPerHour = 300 },
            new() { Id = Guid.NewGuid(), PostName = "Логист", PostType = PostType.Logistician, SalaryPerHour = 400 }
        };
        _database.Values.AddRange(posts);

        var result = _logic.Get(salaryMin: 350);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(1, _database.Calls["Read"]);
    }

    [TestMethod]
    public void TestMethod_Get_BySalaryMax()
    {
        var posts = new List<Post>
        {
            new() { Id = Guid.NewGuid(), PostName = "Инженер", PostType = PostType.Engineer, SalaryPerHour = 500 },
            new() { Id = Guid.NewGuid(), PostName = "Сборщик", PostType = PostType.Assembler, SalaryPerHour = 300 },
            new() { Id = Guid.NewGuid(), PostName = "Логист", PostType = PostType.Logistician, SalaryPerHour = 400 }
        };
        _database.Values.AddRange(posts);

        var result = _logic.Get(salaryMax: 350);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, _database.Calls["Read"]);
    }

    [TestMethod]
    public void TestMethod_Get_ById()
    {
        var post = new Post { Id = Guid.NewGuid(), PostName = "Инженер", PostType = PostType.Engineer, SalaryPerHour = 500 };
        _database.Values.Add(post);

        var result = _logic.Get(post.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(post.Id, result.Id);
        Assert.AreEqual(1, _database.Calls["ReadSingle"]);
    }

    [TestMethod]
    public void TestMethod_Get_ById_WrongId()
    {
        var post = new Post { Id = Guid.NewGuid(), PostName = "Инженер", PostType = PostType.Engineer, SalaryPerHour = 500 };
        _database.Values.Add(post);
        _logic.Get(Guid.NewGuid());

        Assert.AreEqual("Должность не найдена", _logic.ErrorMessage);
        Assert.AreEqual(1, _database.Calls["ReadSingle"]);
    }

    [TestMethod]
    public void TestMethod_Add()
    {
        var post = new Post { Id = Guid.NewGuid(), PostName = "Инженер завода", PostType = PostType.Engineer, SalaryPerHour = 500 };

        var result = _logic.Add(post);

        Assert.IsTrue(result);
        Assert.AreEqual(1, _database.Calls["Read"]);
        Assert.AreEqual(1, _database.Calls["Create"]);
    }

    [TestMethod]
    public void TestMethod_Add_DuplicateName()
    {
        var existingPost = new Post { Id = Guid.NewGuid(), PostName = "Инженер завода", PostType = PostType.Engineer, SalaryPerHour = 500 };
        _database.Values.Add(existingPost);

        var newPost = new Post { Id = Guid.NewGuid(), PostName = "Инженер завода", PostType = PostType.Assembler, SalaryPerHour = 300 };

        var result = _logic.Add(newPost);

        Assert.IsFalse(result);
        Assert.AreEqual("Есть должность с таким названием", _logic.ErrorMessage);
        Assert.AreEqual(1, _database.Calls["Read"]);
        Assert.AreEqual(0, _database.Calls["Create"]);
    }

    [TestMethod]
    public void TestMethod_Add_InvalidData()
    {
        var post = new Post { Id = Guid.NewGuid(), PostName = "", PostType = PostType.Engineer, SalaryPerHour = 500 };

        var result = _logic.Add(post);

        Assert.IsFalse(result);
        Assert.IsTrue(!string.IsNullOrEmpty(_logic.ErrorMessage));
        Assert.AreEqual(0, _database.Calls["Read"]);
        Assert.AreEqual(0, _database.Calls["Create"]);
    }

    [TestMethod]
    public void TestMethod_Update()
    {
        var post = new Post { Id = Guid.NewGuid(), PostName = "Инженер", PostType = PostType.Engineer, SalaryPerHour = 500 };
        _database.Values.Add(post);

        var updatedPost = new Post { Id = post.Id, PostName = "Главный инженер", PostType = PostType.Engineer, SalaryPerHour = 600 };

        var result = _logic.Update(updatedPost);

        Assert.IsTrue(result);
        Assert.AreEqual(1, _database.Calls["ReadSingle"]);
        Assert.AreEqual(1, _database.Calls["Update"]);
    }

    [TestMethod]
    public void TestMethod_Update_WrongId()
    {
        var post = new Post { Id = Guid.NewGuid(), PostName = "Инженер", PostType = PostType.Engineer, SalaryPerHour = 500 };
        _database.Values.Add(post);

        var updatedPost = new Post { Id = Guid.NewGuid(), PostName = "Главный инженер", PostType = PostType.Engineer, SalaryPerHour = 600 };

        var result = _logic.Update(updatedPost);

        Assert.IsFalse(result);
        Assert.AreEqual("Должность не найдена", _logic.ErrorMessage);
        Assert.AreEqual(1, _database.Calls["ReadSingle"]);
        Assert.AreEqual(0, _database.Calls["Update"]);
    }

    [TestMethod]
    public void TestMethod_Delete()
    {
        var post = new Post { Id = Guid.NewGuid(), PostName = "Инженер", PostType = PostType.Engineer, SalaryPerHour = 500 };
        _database.Values.Add(post);

        var result = _logic.Delete(post.Id);

        Assert.IsTrue(result);
        Assert.AreEqual(1, _database.Calls["ReadSingle"]);
        Assert.AreEqual(1, _database.Calls["Delete"]);
    }

    [TestMethod]
    public void TestMethod_Delete_WrongId()
    {
        var post = new Post { Id = Guid.NewGuid(), PostName = "Инженер", PostType = PostType.Engineer, SalaryPerHour = 500 };
        _database.Values.Add(post);

        var result = _logic.Delete(Guid.NewGuid());

        Assert.IsFalse(result);
        Assert.AreEqual("Должность не найдена", _logic.ErrorMessage);
        Assert.AreEqual(1, _database.Calls["ReadSingle"]);
        Assert.AreEqual(0, _database.Calls["Delete"]);
    }
}