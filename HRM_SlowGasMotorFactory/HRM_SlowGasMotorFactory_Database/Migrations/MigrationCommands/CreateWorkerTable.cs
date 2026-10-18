using HRM_SlowGasMotorFactory_Database.SchemaDto;

namespace HRM_SlowGasMotorFactory_Database.Migrations.MigrationCommands;

internal class CreateWorkerTable : IMigrationCommand
{
    public string Version => "0.2.0";

    public string Command => $@"
        CREATE TABLE {WorkerTableSchema.TableName} (
            {WorkerTableSchema.ColumnaIdName} UUID PRIMARY KEY,
            {WorkerTableSchema.ColumnaPersonalNumberName} VARCHAR(10) UNIQUE NOT NULL,
            {WorkerTableSchema.ColumnaPostIdName} UUID NOT NULL,
            {WorkerTableSchema.ColumnaPostBeforeIdName} UUID,
            {WorkerTableSchema.ColumnaPostBeforeBeforeIdName} UUID,
            {WorkerTableSchema.ColumnaFIOName} VARCHAR(255) NOT NULL,
            {WorkerTableSchema.ColumnaBirthDateName} DATE NOT NULL,
            {WorkerTableSchema.ColumnaEmploymentDateName} DATE NOT NULL,
            {WorkerTableSchema.ColumnaIsDeletedName} BOOLEAN NOT NULL DEFAULT FALSE,
            FOREIGN KEY ({WorkerTableSchema.ColumnaPostIdName}) REFERENCES {PostTableSchema.TableName}(Id),
            FOREIGN KEY ({WorkerTableSchema.ColumnaPostBeforeIdName}) REFERENCES {PostTableSchema.TableName}(Id),
            FOREIGN KEY ({WorkerTableSchema.ColumnaPostBeforeBeforeIdName}) REFERENCES {PostTableSchema.TableName}(Id)
        );";
}