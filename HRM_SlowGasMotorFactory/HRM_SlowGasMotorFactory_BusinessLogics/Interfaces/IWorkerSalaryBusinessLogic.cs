using HRM_SlowGasMotorFactory_DataModels.DataModels;

namespace HRM_SlowGasMotorFactory_BusinessLogics.Interfaces.Business;

public interface IWorkerSalaryBusinessLogic
{
    string ErrorMessage { get; }
    List<WorkerSalary> Get(Guid? workerId = null, DateTime? calculateDateStart = null, DateTime? calculateDateFinish = null);
    void CalculateSalary(DateTime date);
}