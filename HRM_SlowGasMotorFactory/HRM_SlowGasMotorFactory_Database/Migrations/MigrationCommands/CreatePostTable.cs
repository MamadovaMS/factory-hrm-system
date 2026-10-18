using HRM_SlowGasMotorFactory_Database.SchemaDto;

namespace HRM_SlowGasMotorFactory_Database.Migrations.MigrationCommands;

internal class CreatePostTable : IMigrationCommand
{
    public string Version => "0.1.0";

    public string Command => $@"
        CREATE TABLE {PostTableSchema.TableName} (
            {PostTableSchema.ColumnaIdName} UUID PRIMARY KEY,
            {PostTableSchema.ColumnaPostNameName} VARCHAR(255) UNIQUE NOT NULL,
            {PostTableSchema.ColumnaPostTypeName} INTEGER NOT NULL,
            {PostTableSchema.ColumnaSalaryName} DOUBLE PRECISION NOT NULL
        );";
}