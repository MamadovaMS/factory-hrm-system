using HRM_SlowGasMotorFactory_Database.SchemaDto;

namespace HRM_SlowGasMotorFactory_Database.Migrations.MigrationCommands;

internal class CreatePassMarkTable : IMigrationCommand
{
    public string Version => "0.3.0";

    public string Command => $@"
        CREATE TABLE {PassMarkTableSchema.TableName} (
            {PassMarkTableSchema.ColumnaIdName} UUID PRIMARY KEY,
            {PassMarkTableSchema.ColumnaWorkerIdName} UUID NOT NULL,
            {PassMarkTableSchema.ColumnaPassNumberName} VARCHAR(10) NOT NULL,
            {PassMarkTableSchema.ColumnaMarkDateTimeInName} TIMESTAMP NOT NULL,
            {PassMarkTableSchema.ColumnaMarkDateTimeOutName} TIMESTAMP,
            FOREIGN KEY ({PassMarkTableSchema.ColumnaWorkerIdName}) REFERENCES {WorkerTableSchema.TableName}(Id)
        );";
}