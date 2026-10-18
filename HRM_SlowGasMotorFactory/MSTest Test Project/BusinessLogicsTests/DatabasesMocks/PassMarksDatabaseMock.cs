using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;
using HRM_SlowGasMotorFactory_DataModels.DataModels;

namespace HRM_SlowGasMotorFactory_TestsMSTest.BusinessLogicsTests.DatabasesMocks;

internal class PassMarksDatabaseMock : IPassMarksDatabase
{
    public readonly List<PassMarks> Values = [];

    public readonly Dictionary<string, int> Calls = new()
    {
        { "Create", 0 },
        { "Read", 0 },
        { "Update", 0 }
    };

    public bool Create(PassMarks passMarks)
    {
        Values.Add(passMarks);
        Calls["Create"]++;
        return true;
    }

    public List<PassMarks> Read()
    {
        Calls["Read"]++;
        return Values;
    }

    public bool Update(PassMarks passMarks)
    {
        Values.RemoveAll(x => x.WorkerId == passMarks.WorkerId &&
                              x.PassNumber == passMarks.PassNumber &&
                              x.MarkDateTimeOut is null);
        Values.Add(passMarks);
        Calls["Update"]++;
        return true;
    }
}