using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;
using HRM_SlowGasMotorFactory_DataModels.DataModels;
using HRM_SlowGasMotorFactory_DataModels.Enums;

namespace HRM_SlowGasMotorFactory_TestsMSTest.BusinessLogicsTests.DatabasesMocks;

internal class WorkerDatabaseMock : IWorkerDatabase
{
    public readonly List<Worker> Values = [];

    public readonly Dictionary<string, int> Calls = new()
    {
        { "Create", 0 },
        { "Read", 0 },
        { "ReadSingle", 0 },
        { "ReadWorkerPreviousPosts", 0 },
        { "Update", 0 },
        { "Delete", 0 }
    };

    public bool Create(Worker worker)
    {
        Values.Add(worker);
        Calls["Create"]++;
        return true;
    }

    public List<Worker> Read()
    {
        Calls["Read"]++;
        return Values;
    }

    public Worker? Read(Guid id)
    {
        Calls["ReadSingle"]++;
        return Values.FirstOrDefault(x => x.Id == id);
    }

    public List<Post> ReadWorkerPreviousPosts(Guid id)
    {
        var elem = Values.FirstOrDefault(x => x.Id == id);
        Calls["ReadWorkerPreviousPosts"]++;
        return
        [
            new() { Id = elem?.PostId ?? Guid.NewGuid(), PostName = "Старая должность 1", PostType = PostType.Assembler, SalaryPerHour = 100 },
            new() { Id = Guid.NewGuid(), PostName = "Старая должность 2", PostType = PostType.Engineer, SalaryPerHour = 200 }
        ];
    }

    public bool Update(Worker worker)
    {
        Values.RemoveAll(x => x.Id == worker.Id);
        Values.Add(worker);
        Calls["Update"]++;
        return true;
    }

    public bool Delete(Guid id)
    {
        var elem = Values.FirstOrDefault(x => x.Id == id);
        if (elem != null)
        {
            elem.IsDeleted = true;
        }
        Calls["Delete"]++;
        return true;
    }
}