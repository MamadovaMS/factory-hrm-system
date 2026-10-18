using HRM_SlowGasMotorFactory_DataModels.DataModels;

namespace HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;

public interface IPostDatabase
{
    List<Post> Read();
    Post? Read(Guid id);
    bool Create(Post post);
    bool Update(Post post);
    bool Delete(Guid id);
}