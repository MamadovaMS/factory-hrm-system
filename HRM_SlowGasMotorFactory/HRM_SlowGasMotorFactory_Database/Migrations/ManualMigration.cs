using Dapper;
using HRM_SlowGasMotorFactory_Database.Migrations.MigrationCommands;
using HRM_SlowGasMotorFactory_Database.SchemaDto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

namespace HRM_SlowGasMotorFactory_Database.Migrations;

public class ManualMigration
{
    private const string TableMigrationName = "MigrationsTable";
    private readonly Queue<IMigrationCommand> _migrations = new();
    private readonly ILogger<ManualMigration> _logger;
    private readonly string _connectionString;

    public ManualMigration(IConfiguration configuration, ILogger<ManualMigration> logger)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("ManualConnection") ??
            throw new InvalidOperationException("Отсутствует строка подключения к базе данных");

        _migrations.Enqueue(new CreatePostTable());
        _migrations.Enqueue(new CreateWorkerTable());
        _migrations.Enqueue(new CreatePassMarkTable());
        _migrations.Enqueue(new CreateWorkerSalaryTable());
    }

    public IDbConnection Connection => OpenConnection();

    public void ApplyMigrations()
    {
        try
        {
            CheckDatabaseExistsAndCreateIfNotExists();
            using var connection = OpenConnection();
            var versions = GetVersions(connection);

            foreach (var version in versions)
            {
                if (_migrations.Count == 0)
                {
                    _logger.LogWarning($"Очередь миграций пуста, но в БД есть версия {version}. Пропускаем проверку.");
                    break;
                }
                var migration = _migrations.Dequeue();
                if (version != migration.Version)
                {
                    throw new InvalidOperationException($"Порядок миграций нарушен: ожидалась {version}, получена {migration.Version}");
                }
            }

            while (_migrations.Count > 0)
            {
                var newMigration = _migrations.Dequeue();
                connection.Execute(newMigration.Command);
                connection.Execute($@"
                INSERT INTO {TableMigrationName} (Version, DateApply) 
                VALUES (@Version, @DateApply)",
                    new { newMigration.Version, DateApply = DateTime.UtcNow });

                _logger.LogInformation($"Применена миграция {newMigration.Version}");
            }

            _logger.LogInformation("Все миграции успешно применены");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при применении миграций");
            throw;
        }
    }

    public void DropDatabase()
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(_connectionString);
            var databaseName = builder.Database ?? throw new InvalidOperationException("Нет имени БД");
            var connectionStringWithoutDatabase = _connectionString.Replace($"Database={databaseName};", "");

            using var connection = new NpgsqlConnection(connectionStringWithoutDatabase);
            connection.Open();
            connection.Execute($"DROP DATABASE IF EXISTS {databaseName};");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dropping database");
        }
    }

    private void CheckDatabaseExistsAndCreateIfNotExists()
    {
        var builder = new NpgsqlConnectionStringBuilder(_connectionString);
        var databaseName = builder.Database ?? throw new InvalidOperationException("Нет имени БД");
        var connectionStringWithoutDatabase = _connectionString.Replace($"Database={databaseName};", "Database=postgres;");

        using var connection = new NpgsqlConnection(connectionStringWithoutDatabase);
        connection.Open();
        var exists = connection.QueryFirst<bool>($@"
            SELECT EXISTS(SELECT datname FROM pg_catalog.pg_database WHERE lower(datname) = lower('{databaseName}'))");

        if (!exists)
        {
            connection.Execute($@"CREATE DATABASE ""{databaseName}""");
        }
    }

    private NpgsqlConnection OpenConnection()
    {
        var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        return conn;
    }

    private static List<string> GetVersions(NpgsqlConnection connection)
    {
        var existsCheck = connection.QueryFirst<bool>($@"
            SELECT EXISTS (SELECT FROM information_schema.tables 
            WHERE table_schema = 'public' AND table_name = '{TableMigrationName.ToLower()}')");

        if (!existsCheck)
        {
            connection.Execute($@"
                CREATE TABLE {TableMigrationName.ToLower()} (
                    Id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    Version VARCHAR(255) NOT NULL,
                    DateApply DATE DEFAULT CURRENT_DATE
                )");
            return new List<string>();
        }

        return connection.Query<string>($@"SELECT Version FROM {TableMigrationName} ORDER BY DateApply ASC").ToList();
    }
}