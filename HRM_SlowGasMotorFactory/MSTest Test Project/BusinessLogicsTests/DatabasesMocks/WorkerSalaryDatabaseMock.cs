using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces.Database;
using HRM_SlowGasMotorFactory_DataModels.DataModels;

namespace HRM_SlowGasMotorFactory_TestsMSTest.BusinessLogicsTests.DatabasesMocks;

internal class WorkerSalaryDatabaseMock : IWorkerSalaryDatabase
{
    public readonly List<WorkerSalary> Values = [];
    public readonly Dictionary<string, int> Calls = new()
    {
        { "Read", 0 },
        { "Create", 0 }
    };

    public List<WorkerSalary> Read()
    {
        Calls["Read"]++;
        return Values;
    }

    public bool Create(WorkerSalary workerSalary)
    {
        Values.Add(workerSalary);
        Calls["Create"]++;
        return true;
    }
}