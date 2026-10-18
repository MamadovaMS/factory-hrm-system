using Dapper;
using HRM_SlowGasMotorFactory_Database.Implementations;
using HRM_SlowGasMotorFactory_Database.Migrations;
using HRM_SlowGasMotorFactory_Database.SchemaDto;
using HRM_SlowGasMotorFactory_DataModels.DataModels;
using HRM_SlowGasMotorFactory_DataModels.Enums;
using HRM_SlowGasMotorFactory_TestsMSTest.DatabaseTest;
using System.Data;

namespace HRM_SlowGasMotorFactory_TestsMSTest.DatabaseTest;

[TestClass]
public sealed class PostDatabaseTests
{
    private static ManualMigration _manualMigration = null!;
    private PostDatabase _database = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
        _manualMigration = ManualMigrationCreator.Create();
        _manualMigration.ApplyMigrations();
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _manualMigration?.DropDatabase();
    }

    [TestInitialize]
    public void TestInit()
    {
        _database = new PostDatabase(_manualMigration, new FakeLogger<PostDatabase>());
    }

    [TestCleanup]
    public void TestCleanup()
    {
        using var connection = _manualMigration.Connection;
        connection.Execute($"DELETE FROM {WorkerTableSchema.TableName};");
        connection.Execute($"DELETE FROM {PostTableSchema.TableName};");
    }

    [TestMethod]
    public void TestMethod_Read_All()
    {
        using var connection = _manualMigration.Connection;
        InsertPost(connection, 3);
        var posts = _database.Read();
        Assert.IsNotNull(posts);
        Assert.AreEqual(3, posts.Count);
    }

    [TestMethod]
    public void TestMethod_Read_Empty()
    {
        var posts = _database.Read();
        Assert.IsNotNull(posts);
        Assert.AreEqual(0, posts.Count);
    }

    [TestMethod]
    public void TestMethod_Read_Id()
    {
        using var connection = _manualMigration.Connection;
        var postId = InsertPost(connection, 1);
        var postFromDb = _database.Read(postId);
        Assert.IsNotNull(postFromDb);
        Assert.AreEqual(postId, postFromDb.Id);
    }

    [TestMethod]
    public void TestMethod_Read_Id_WrongId()
    {
        using var connection = _manualMigration.Connection;
        InsertPost(connection, 1);
        var postFromDb = _database.Read(Guid.NewGuid());
        Assert.IsNull(postFromDb);
    }

    [TestMethod]
    public void TestMethod_Create()
    {
        var post = new Post
        {
            Id = Guid.NewGuid(),
            PostName = "Инженер",
            PostType = PostType.Engineer,
            SalaryPerHour = 500
        };
        var result = _database.Create(post);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestMethod_Create_IdExists()
    {
        using var connection = _manualMigration.Connection;
        var postId = InsertPost(connection, 1);
        var post = new Post { Id = postId, PostName = "Новая", PostType = PostType.Assembler, SalaryPerHour = 300 };
        var result = _database.Create(post);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TestMethod_Create_PostNameExists()
    {
        using var connection = _manualMigration.Connection;
        var existingPost = new Post
        {
            Id = Guid.NewGuid(),
            PostName = "Уникальная должность",
            PostType = PostType.Engineer,
            SalaryPerHour = 500
        };
        _database.Create(existingPost);

        var newPost = new Post
        {
            Id = Guid.NewGuid(),
            PostName = existingPost.PostName,
            PostType = PostType.Assembler,
            SalaryPerHour = 300
        };
        var result = _database.Create(newPost);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TestMethod_Update()
    {
        using var connection = _manualMigration.Connection;
        var postId = InsertPost(connection, 1);
        var post = new Post { Id = postId, PostName = "Обновленная", PostType = PostType.Engineer, SalaryPerHour = 600 };
        var result = _database.Update(post);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestMethod_Update_NotExists()
    {
        var post = new Post { Id = Guid.NewGuid(), PostName = "Новая", PostType = PostType.Engineer, SalaryPerHour = 500 };
        var result = _database.Update(post);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TestMethod_Delete()
    {
        using var connection = _manualMigration.Connection;
        var postId = InsertPost(connection, 1);
        var result = _database.Delete(postId);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestMethod_Delete_NotExists()
    {
        var result = _database.Delete(Guid.NewGuid());
        Assert.IsFalse(result);
    }

    private static Guid InsertPost(IDbConnection connection, int count)
    {
        var query = $@"
            INSERT INTO {PostTableSchema.TableName}
            ({PostTableSchema.ColumnaIdName}, {PostTableSchema.ColumnaPostNameName}, 
             {PostTableSchema.ColumnaPostTypeName}, {PostTableSchema.ColumnaSalaryName})
            VALUES (@Id, @PostName, @PostType, @SalaryPerHour)";

        var lastId = Guid.NewGuid();
        for (int i = 0; i < count; i++)
        {
            lastId = Guid.NewGuid();
            connection.Execute(query, new Post
            {
                Id = lastId,
                PostName = $"Должность {i}",
                PostType = PostType.Engineer,
                SalaryPerHour = 500 + i * 100
            });
        }
        return lastId;
    }
}