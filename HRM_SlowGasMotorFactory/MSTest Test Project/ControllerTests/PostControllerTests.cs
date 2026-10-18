using HRM_SlowGasMotorFactory_DataModels.Enums;
using System.Net.Http.Json;

namespace HRM_SlowGasMotorFactory_TestsMSTest.ControllerTests;

[TestClass]
public sealed class PostControllerTests : BaseControllerTests
{
    [TestMethod]
    public async Task TestMethod_Auth()
    {
        await CheckAuth("/api/post");
    }

    [TestMethod]
    public async Task TestMethod_Get_All_WithoutFilters()
    {
        using var connection = ManualMigration.Connection;
        var postId = InsertPost(connection, "Инженер завода", PostType.Engineer, 500);
        InsertPost(connection, "Сборщик двигателей", PostType.Assembler, 300);
        InsertPost(connection, "Логист", PostType.Logistician, 400);

        var response = await Client.GetAsync("/api/post");
        var content = await GetPostsFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(3, content.Count);
        Assert.IsTrue(content.Any(x => x.Id == postId));
    }

    [TestMethod]
    public async Task TestMethod_Get_All_WithFilters()
    {
        using var connection = ManualMigration.Connection;
        InsertPost(connection, "Инженер завода", PostType.Engineer, 500);
        var postId = InsertPost(connection, "Сборщик двигателей", PostType.Assembler, 300);
        InsertPost(connection, "Логист", PostType.Logistician, 400);

        var response = await Client.GetAsync("/api/post?postType=Assembler");
        var content = await GetPostsFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(1, content.Count);
        Assert.IsTrue(content.Any(x => x.Id == postId));
        Assert.AreEqual("Сборщик двигателей", content.First().PostName);
    }

    [TestMethod]
    public async Task TestMethod_Get_All_NoRecords()
    {
        var response = await Client.GetAsync("/api/post");
        var content = await GetPostsFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual(0, content.Count);
    }

    [TestMethod]
    public async Task TestMethod_Get_Single_HaveRecord()
    {
        using var connection = ManualMigration.Connection;
        var postId = InsertPost(connection, "Инженер завода", PostType.Engineer, 500);

        var response = await Client.GetAsync($"/api/post/{postId}");
        var content = await GetPostFromResponseAsync(response);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(content);
        Assert.AreEqual("Инженер завода", content.PostName);
        Assert.AreEqual(PostType.Engineer, content.PostType);
        Assert.AreEqual(500, content.SalaryPerHour);
    }

    [TestMethod]
    public async Task TestMethod_Get_Single_NoRecord()
    {
        using var connection = ManualMigration.Connection;
        InsertPost(connection, "Инженер завода", PostType.Engineer, 500);

        var response = await Client.GetAsync($"/api/post/{Guid.NewGuid()}");

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task TestMethod_Create()
    {
        using var connection = ManualMigration.Connection;
        var newPost = CreatePost(Guid.NewGuid(), "Новая должность", PostType.Engineer, 600);

        var response = await Client.PostAsync("/api/post", JsonContent.Create(newPost));
        var checkPost = GetPostById(connection, newPost.Id);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        Assert.IsNotNull(checkPost);
        Assert.AreEqual(newPost.PostName, checkPost.PostName);
        Assert.AreEqual(newPost.SalaryPerHour, checkPost.SalaryPerHour);
    }

    [TestMethod]
    public async Task TestMethod_Create_Validation()
    {
        using var connection = ManualMigration.Connection;
        var newPost = CreatePost(Guid.NewGuid(), "Корот", PostType.Engineer, 600);

        var response = await Client.PostAsync("/api/post", JsonContent.Create(newPost));
        var checkPost = GetPostById(connection, newPost.Id);
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsTrue(content.Contains("One or more validation errors occurred"));
        Assert.IsNull(checkPost);
    }

    [TestMethod]
    public async Task TestMethod_Create_PostNameExists()
    {
        using var connection = ManualMigration.Connection;
        InsertPost(connection, "Инженер завода", PostType.Engineer, 500);
        var newPost = CreatePost(Guid.NewGuid(), "Инженер завода", PostType.Assembler, 300);

        var response = await Client.PostAsync("/api/post", JsonContent.Create(newPost));
        var checkPost = GetPostById(connection, newPost.Id);
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.AreEqual("Есть должность с таким названием", content);
        Assert.IsNull(checkPost);
    }

    [TestMethod]
    public async Task TestMethod_Create_IdExists()
    {
        using var connection = ManualMigration.Connection;
        var postId = InsertPost(connection, "Инженер завода", PostType.Engineer, 500);
        var newPost = CreatePost(postId, "Новая должность", PostType.Assembler, 300);

        var response = await Client.PostAsync("/api/post", JsonContent.Create(newPost));
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.AreEqual(string.Empty, content);
    }

    [TestMethod]
    public async Task TestMethod_Update()
    {
        using var connection = ManualMigration.Connection;
        var postId = InsertPost(connection, "Инженер завода", PostType.Engineer, 500);
        var updatedPost = CreatePost(postId, "Главный инженер", PostType.Engineer, 600);

        var response = await Client.PutAsync("/api/post", JsonContent.Create(updatedPost));
        var checkPost = GetPostById(connection, postId);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        Assert.IsNotNull(checkPost);
        Assert.AreEqual("Главный инженер", checkPost.PostName);
        Assert.AreEqual(600, checkPost.SalaryPerHour);
    }

    [TestMethod]
    public async Task TestMethod_Update_NotExists()
    {
        using var connection = ManualMigration.Connection;
        InsertPost(connection, "Инженер завода", PostType.Engineer, 500);
        var updatedPost = CreatePost(Guid.NewGuid(), "Главный инженер", PostType.Engineer, 600);

        var response = await Client.PutAsync("/api/post", JsonContent.Create(updatedPost));
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.AreEqual("Должность не найдена", content);
    }

    [TestMethod]
    public async Task TestMethod_Update_PostNameExists()
    {
        using var connection = ManualMigration.Connection;
        InsertPost(connection, "Инженер завода", PostType.Engineer, 500);
        var postId = InsertPost(connection, "Сборщик двигателей", PostType.Assembler, 300);
        var updatedPost = CreatePost(postId, "Инженер завода", PostType.Assembler, 300);

        var response = await Client.PutAsync("/api/post", JsonContent.Create(updatedPost));
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.AreEqual("Есть должность с таким названием", content);
    }

    [TestMethod]
    public async Task TestMethod_Update_Validation()
    {
        using var connection = ManualMigration.Connection;
        var postId = InsertPost(connection, "Инженер завода", PostType.Engineer, 500);
        var updatedPost = CreatePost(postId, "Корот", PostType.Engineer, 600);

        var response = await Client.PutAsync("/api/post", JsonContent.Create(updatedPost));
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsTrue(content.Contains("One or more validation errors occurred"));
    }

    [TestMethod]
    public async Task TestMethod_Delete()
    {
        using var connection = ManualMigration.Connection;
        var postId = InsertPost(connection, "Инженер завода", PostType.Engineer, 500);

        var response = await Client.DeleteAsync($"/api/post/{postId}");
        var checkPost = GetPostById(connection, postId);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        Assert.IsNull(checkPost);
    }

    [TestMethod]
    public async Task TestMethod_Delete_NotExists()
    {
        using var connection = ManualMigration.Connection;
        InsertPost(connection, "Инженер завода", PostType.Engineer, 500);

        var response = await Client.DeleteAsync($"/api/post/{Guid.NewGuid()}");
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.AreEqual("Должность не найдена", content);
    }
}