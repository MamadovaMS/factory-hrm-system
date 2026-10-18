using Dapper;
using HRM_SlowGasMotorFactory_Database.SchemaDto;
using HRM_SlowGasMotorFactory_DataModels.Enums;
using System.Net;

namespace HRM_SlowGasMotorFactory_TestsMSTest.ControllerTests;

[TestClass]
public class WorkerPostHistoryControllerTests : BaseControllerTests
{
    private const string BaseUrl = "/api/workerposthistory";

    [TestMethod]
    public async Task Get_WorkerWithPostHistory_ReturnsPosts()
    {
        using var connection = ManualMigration.Connection;

        var postId1 = InsertPost(connection, "Инженер завода", PostType.Engineer, 500);
        var postId2 = InsertPost(connection, "Сборщик двигателей", PostType.Assembler, 300);
        var postId3 = InsertPost(connection, "Логист", PostType.Logistician, 400);

        var workerId = Guid.NewGuid();
        var workerQuery = $@"
            INSERT INTO {WorkerTableSchema.TableName} 
            ({WorkerTableSchema.ColumnaIdName}, {WorkerTableSchema.ColumnaPersonalNumberName},
             {WorkerTableSchema.ColumnaPostIdName}, {WorkerTableSchema.ColumnaPostBeforeIdName},
             {WorkerTableSchema.ColumnaPostBeforeBeforeIdName}, {WorkerTableSchema.ColumnaFIOName},
             {WorkerTableSchema.ColumnaBirthDateName}, {WorkerTableSchema.ColumnaEmploymentDateName},
             {WorkerTableSchema.ColumnaIsDeletedName})
            VALUES (@Id, @PersonalNumber, @PostId, @PostBeforeId, @PostBeforeBeforeId, 
                    @FIO, @BirthDate, @EmploymentDate, @IsDeleted);";

        connection.Execute(workerQuery, new
        {
            Id = workerId,
            PersonalNumber = "1234567890",
            PostId = postId3,
            PostBeforeId = postId2,
            PostBeforeBeforeId = postId1,
            FIO = "Иванов Иван Петрович",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            EmploymentDate = DateTime.UtcNow.AddDays(-100),
            IsDeleted = false
        });

        var response = await Client.GetAsync($"{BaseUrl}/{workerId}");
        var posts = await GetPostsFromResponseAsync(response);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(posts);
        Assert.AreEqual(3, posts.Count);
        Assert.IsTrue(posts.Any(x => x.Id == postId1));
        Assert.IsTrue(posts.Any(x => x.Id == postId2));
        Assert.IsTrue(posts.Any(x => x.Id == postId3));
    }

    [TestMethod]
    public async Task Get_WorkerWithoutHistory_ReturnsCurrentPostOnly()
    {
        using var connection = ManualMigration.Connection;

        InsertPost(connection, "Инженер завода", PostType.Engineer, 500);
        var workerId = InsertWorker(connection, "1234567890", "Иванов Иван Петрович", false);

        var worker = GetWorkerById(connection, workerId);
        var expectedPostId = worker.PostId;

        var response = await Client.GetAsync($"{BaseUrl}/{workerId}");
        var posts = await GetPostsFromResponseAsync(response);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(posts);
        Assert.AreEqual(1, posts.Count);
        Assert.AreEqual(expectedPostId, posts.First().Id);
    }

    [TestMethod]
    public async Task Get_NonExistentWorker_ReturnsEmptyList()
    {
        var response = await Client.GetAsync($"{BaseUrl}/{Guid.NewGuid()}");
        var posts = await GetPostsFromResponseAsync(response);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(posts);
        Assert.AreEqual(0, posts.Count);
    }

    [TestMethod]
    public async Task Get_CheckAuthentication_ReturnsUnauthorized()
    {
        await CheckAuth($"{BaseUrl}/{Guid.NewGuid()}");
    }
}