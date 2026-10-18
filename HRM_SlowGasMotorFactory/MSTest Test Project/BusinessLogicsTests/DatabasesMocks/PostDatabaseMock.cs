using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;
using HRM_SlowGasMotorFactory_DataModels.DataModels;

namespace HRM_SlowGasMotorFactory_TestsMSTest.BusinessLogicsTests.DatabasesMocks;

internal class PostDatabaseMock : IPostDatabase
{
    public readonly List<Post> Values = [];

    public readonly Dictionary<string, int> Calls = new()
    {
        { "Create", 0 },
        { "Read", 0 },
        { "ReadSingle", 0 },
        { "Update", 0 },
        { "Delete", 0 }
    };

    public bool Create(Post post)
    {
        Values.Add(post);
        Calls["Create"]++;
        return true;
    }

    public List<Post> Read()
    {
        Calls["Read"]++;
        return Values;
    }

    public Post? Read(Guid id)
    {
        Calls["ReadSingle"]++;
        return Values.FirstOrDefault(x => x.Id == id);
    }

    public bool Update(Post post)
    {
        Values.RemoveAll(x => x.Id == post.Id);
        Values.Add(post);
        Calls["Update"]++;
        return true;
    }

    public bool Delete(Guid id)
    {
        Values.RemoveAll(x => x.Id == id);
        Calls["Delete"]++;
        return true;
    }
}