using Dapper;
using HRM_SlowGasMotorFactory_Database.Migrations;
using HRM_SlowGasMotorFactory_Database.SchemaDto;
using HRM_SlowGasMotorFactory_DataModels.DataModels;
using HRM_SlowGasMotorFactory_DataModels.Enums;
using HRM_SlowGasMotorFactory_TestsMSTest.DatabaseTest;
using HRM_SlowGasMotorFactory_TestsMSTest.DatabaseTests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Data;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HRM_SlowGasMotorFactory_TestsMSTest.ControllerTests;

[TestClass]
public class BaseControllerTests
{
    private static WebApplicationFactory<Program> _factory = null!;

    protected static ManualMigration ManualMigration { get; private set; } = null!;

    protected HttpClient Client { get; private set; } = null!;

    [AssemblyInitialize]
    public static void AssemblyInit(TestContext context)
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
        });

        ManualMigration = ManualMigrationCreator.Create();
        ManualMigration.ApplyMigrations();
    }

    [AssemblyCleanup]
    public static void CleanUp()
    {
        ManualMigration?.DropDatabase();
        _factory.Dispose();
    }

    [TestInitialize]
    public void Initialize()
    {
        Client = _factory.CreateClient();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("BasicAuth",
            Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser:testpassword")));
    }

    [TestCleanup]
    public void TestCleanup()
    {
        using var connection = ManualMigration.Connection;
        connection.Execute($@"DELETE FROM {WorkerSalaryTableSchema.TableName};");
        connection.Execute($@"DELETE FROM {PassMarkTableSchema.TableName};");
        connection.Execute($@"DELETE FROM {WorkerTableSchema.TableName};");
        connection.Execute($@"DELETE FROM {PostTableSchema.TableName};");
        Client.Dispose();
    }

    protected static async Task CheckAuth(string path)
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync(path);
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    protected static async Task<List<Post>> GetPostsFromResponseAsync(HttpResponseMessage response) =>
        JsonSerializer.Deserialize<List<Post>>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

    protected static async Task<Post> GetPostFromResponseAsync(HttpResponseMessage response) =>
        JsonSerializer.Deserialize<Post>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

    protected static async Task<List<Worker>> GetWorkersFromResponseAsync(HttpResponseMessage response) =>
        JsonSerializer.Deserialize<List<Worker>>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

    protected static async Task<Worker> GetWorkerFromResponseAsync(HttpResponseMessage response) =>
        JsonSerializer.Deserialize<Worker>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

    protected static async Task<List<PassMarks>> GetPassMarksFromResponseAsync(HttpResponseMessage response) =>
        JsonSerializer.Deserialize<List<PassMarks>>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

    protected static async Task<PassMarks> GetPassMarkFromResponseAsync(HttpResponseMessage response) =>
        JsonSerializer.Deserialize<PassMarks>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

    protected static async Task<List<WorkerSalary>> GetWorkerSalariesFromResponseAsync(HttpResponseMessage response) =>
        JsonSerializer.Deserialize<List<WorkerSalary>>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

    protected static async Task<WorkerSalary> GetWorkerSalaryFromResponseAsync(HttpResponseMessage response) =>
        JsonSerializer.Deserialize<WorkerSalary>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

    protected static Guid InsertPost(IDbConnection connection, string postName, PostType postType, double salary)
    {
        var query = $@"
            INSERT INTO {PostTableSchema.TableName}
            ({PostTableSchema.ColumnaIdName}, {PostTableSchema.ColumnaPostNameName},
             {PostTableSchema.ColumnaPostTypeName}, {PostTableSchema.ColumnaSalaryName})
            VALUES (@Id, @PostName, @PostType, @SalaryPerHour);";

        var postId = Guid.NewGuid();
        connection.Execute(query, new Post
        {
            Id = postId,
            PostName = postName,
            PostType = postType,
            SalaryPerHour = salary
        });
        return postId;
    }

    protected static Guid InsertWorker(IDbConnection connection, string personalNumber, string fio, bool isDeleted)
    {
        var queryWorker = $@"
            INSERT INTO {WorkerTableSchema.TableName}
            ({WorkerTableSchema.ColumnaIdName}, {WorkerTableSchema.ColumnaPersonalNumberName},
             {WorkerTableSchema.ColumnaPostIdName}, {WorkerTableSchema.ColumnaFIOName},
             {WorkerTableSchema.ColumnaBirthDateName}, {WorkerTableSchema.ColumnaEmploymentDateName},
             {WorkerTableSchema.ColumnaIsDeletedName})
            VALUES (@Id, @PersonalNumber, @PostId, @FIO, @BirthDate, @EmploymentDate, @IsDeleted);";

        var workerId = Guid.NewGuid();
        connection.Execute(queryWorker, new Worker
        {
            Id = workerId,
            PersonalNumber = personalNumber,
            PostId = GetPostId(connection, "Post Worker"),
            FIO = fio,
            BirthDate = DateTime.UtcNow.AddYears(-20),
            EmploymentDate = DateTime.UtcNow.AddDays(-100),
            IsDeleted = isDeleted
        });
        return workerId;
    }

    protected static Guid InsertPassMark(IDbConnection connection, Guid workerId, string passNumber,
        DateTime markDateTimeIn, DateTime? markDateTimeOut)
    {
        var queryPassMark = $@"
            INSERT INTO {PassMarkTableSchema.TableName}
            ({PassMarkTableSchema.ColumnaIdName}, {PassMarkTableSchema.ColumnaWorkerIdName},
             {PassMarkTableSchema.ColumnaPassNumberName}, {PassMarkTableSchema.ColumnaMarkDateTimeInName},
             {PassMarkTableSchema.ColumnaMarkDateTimeOutName})
            VALUES (@Id, @WorkerId, @PassNumber, @MarkDateTimeIn, @MarkDateTimeOut)";

        var id = Guid.NewGuid();
        connection.Execute(queryPassMark, new PassMarks
        {
            Id = id,
            WorkerId = workerId,
            PassNumber = passNumber,
            MarkDateTimeIn = markDateTimeIn,
            MarkDateTimeOut = markDateTimeOut
        });
        return id;
    }

    protected static Guid InsertWorkerSalary(IDbConnection connection, Guid workerId, double salary, DateTime calculateDate)
    {
        var query = $@"
            INSERT INTO {WorkerSalaryTableSchema.TableName}
            ({WorkerSalaryTableSchema.ColumnaIdName},
             {WorkerSalaryTableSchema.ColumnaWorkerIdName},
             {WorkerSalaryTableSchema.ColumnaSalaryName},
             {WorkerSalaryTableSchema.ColumnaCalculateDateName})
            VALUES
            (@Id, @WorkerId, @Salary, @CalculateDate);";

        var id = Guid.NewGuid();
        connection.Execute(query, new WorkerSalary
        {
            Id = id,
            WorkerId = workerId,
            Salary = salary,
            CalculateDate = calculateDate
        });
        return id;
    }

    protected static Post? GetPostById(IDbConnection connection, Guid id)
    {
        var query = $@"SELECT * FROM {PostTableSchema.TableName} WHERE {PostTableSchema.ColumnaIdName} = @Id;";
        return connection.QueryFirstOrDefault<Post>(query, new { Id = id });
    }

    protected static Guid GetPostId(IDbConnection connection, string postName)
    {
        var queryGetPost = $@"SELECT * FROM {PostTableSchema.TableName} WHERE {PostTableSchema.ColumnaPostNameName} = @PostName;";
        var postId = connection.QueryFirstOrDefault<Post>(queryGetPost, new { PostName = postName })?.Id;

        if (postId.HasValue)
            return postId.Value;

        var queryPost = $@"
            INSERT INTO {PostTableSchema.TableName}
            ({PostTableSchema.ColumnaIdName}, {PostTableSchema.ColumnaPostNameName},
             {PostTableSchema.ColumnaPostTypeName}, {PostTableSchema.ColumnaSalaryName})
            VALUES (@Id, @PostName, @PostType, @SalaryPerHour);";

        postId = Guid.NewGuid();
        connection.Execute(queryPost, new Post
        {
            Id = postId.Value,
            PostName = postName,
            PostType = PostType.Engineer,
            SalaryPerHour = 500
        });
        return postId.Value;
    }

    protected static Worker? GetWorkerById(IDbConnection connection, Guid id)
    {
        var query = $@"SELECT * FROM {WorkerTableSchema.TableName} WHERE Id = @Id;";
        return connection.QueryFirstOrDefault<Worker>(query, new { Id = id });
    }

    protected static PassMarks? GetPassMarkById(IDbConnection connection, Guid id)
    {
        var query = $@"SELECT * FROM {PassMarkTableSchema.TableName} WHERE {PassMarkTableSchema.ColumnaIdName} = @Id;";
        return connection.QueryFirstOrDefault<PassMarks>(query, new { Id = id });
    }

    protected static WorkerSalary? GetWorkerSalaryById(IDbConnection connection, Guid id)
    {
        var query = $@"SELECT * FROM {WorkerSalaryTableSchema.TableName} WHERE Id = @Id;";
        return connection.QueryFirstOrDefault<WorkerSalary>(query, new { Id = id });
    }

    protected static Guid GetWorkerId(IDbConnection connection, string personalNumber)
    {
        var queryWorker = $@"
            INSERT INTO {WorkerTableSchema.TableName}
            ({WorkerTableSchema.ColumnaIdName}, {WorkerTableSchema.ColumnaPersonalNumberName},
             {WorkerTableSchema.ColumnaPostIdName}, {WorkerTableSchema.ColumnaFIOName},
             {WorkerTableSchema.ColumnaBirthDateName}, {WorkerTableSchema.ColumnaEmploymentDateName})
            VALUES (@Id, @PersonalNumber, @PostId, @FIO, @BirthDate, @EmploymentDate);";

        var workerId = Guid.NewGuid();
        connection.Execute(queryWorker, new Worker
        {
            Id = workerId,
            PersonalNumber = personalNumber,
            PostId = GetPostId(connection, "Post Worker"),
            FIO = "Test Worker",
            BirthDate = DateTime.UtcNow.AddYears(-20),
            EmploymentDate = DateTime.UtcNow,
            IsDeleted = false
        });
        return workerId;
    }

    protected static Post CreatePost(Guid id, string postName, PostType postType, double salary)
    {
        return new Post { Id = id, PostName = postName, PostType = postType, SalaryPerHour = salary };
    }

    protected static Worker CreateWorker(Guid id, string personalNumber, Guid postId)
    {
        return new Worker
        {
            Id = id,
            PersonalNumber = personalNumber,
            PostId = postId,
            FIO = "Иванов Иван Петрович",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            EmploymentDate = DateTime.UtcNow.AddDays(-100),
            IsDeleted = false
        };
    }

    protected static PassMarks CreatePassMark(Guid id, Guid workerId, string passNumber,
        DateTime markDateTimeIn, DateTime? markDateTimeOut)
    {
        return new PassMarks
        {
            Id = id,
            WorkerId = workerId,
            PassNumber = passNumber,
            MarkDateTimeIn = markDateTimeIn,
            MarkDateTimeOut = markDateTimeOut
        };
    }

    protected static WorkerSalary CreateWorkerSalary(Guid id, Guid workerId, double salary, DateTime calculateDate)
    {
        return new WorkerSalary
        {
            Id = id,
            WorkerId = workerId,
            Salary = salary,
            CalculateDate = calculateDate
        };
    }
}