using HRM_SlowGasMotorFactory_DataModels.DataModels;

namespace HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;

public interface IWorkerBusinessLogic
{
    string ErrorMessage { get; }

    List<Worker> Get(string? personalNumber = null, string? fio = null, Guid? postId = null,
        DateTime? birthDateStart = null, DateTime? birthDateFinish = null,
        DateTime? employmentDateStart = null, DateTime? employmentDateFinish = null,
        bool? isDeleted = false);

    Worker? Get(Guid id);
    List<Post> GetWorkerPreviousPosts(Guid workerId);
    bool Add(Worker worker);
    bool Update(Worker worker);
    bool Delete(Guid id);
}