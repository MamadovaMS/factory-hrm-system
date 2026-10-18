using HRM_SlowGasMotorFactory_Database.SchemaDto;

namespace HRM_SlowGasMotorFactory_Database.Migrations.MigrationCommands;

internal class CreateWorkerSalaryTable : IMigrationCommand
{
    public string Version => "0.4.0";

    public string Command => $@"
        CREATE TABLE {WorkerSalaryTableSchema.TableName} (
            {WorkerSalaryTableSchema.ColumnaIdName} UUID PRIMARY KEY,
            {WorkerSalaryTableSchema.ColumnaWorkerIdName} UUID NOT NULL,
            {WorkerSalaryTableSchema.ColumnaSalaryName} DOUBLE PRECISION NOT NULL,
            {WorkerSalaryTableSchema.ColumnaCalculateDateName} DATE NOT NULL,
            FOREIGN KEY ({WorkerSalaryTableSchema.ColumnaWorkerIdName}) REFERENCES {WorkerTableSchema.TableName}(Id)
        );";
}