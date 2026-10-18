using HRM_SlowGasMotorFactory_Database.Migrations;
using Microsoft.Extensions.Configuration;

namespace HRM_SlowGasMotorFactory_TestsMSTest.DatabaseTest;

internal static class ManualMigrationCreator
{
    public static ManualMigration Create()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "ConnectionStrings:ManualConnection", "Host=localhost;Port=5432;Database=SlowGasMotorFactoryTest;Username=postgres;Password=12345;" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        return new ManualMigration(configuration, new FakeLogger<ManualMigration>());
    }
}