using HRM_SlowGasMotorFactory_DataModels.DataModels;

namespace HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;

public interface IWorkerDatabase
{
    List<Worker> Read();
    Worker? Read(Guid id);
    List<Post> ReadWorkerPreviousPosts(Guid workerId);
    bool Create(Worker worker);
    bool Update(Worker worker);
    bool Delete(Guid id);
}