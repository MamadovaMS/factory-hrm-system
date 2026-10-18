namespace HRM_SlowGasMotorFactory_Database.Migrations;

internal interface IMigrationCommand
{
    string Version { get; }
    string Command { get; }
}