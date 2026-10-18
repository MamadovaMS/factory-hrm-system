using HRM_SlowGasMotorFactory_DataModels.DataModels;

namespace HRM_SlowGasMotorFactory_BusinessLogics.Interfaces.Database;

public interface IWorkerSalaryDatabase
{
    List<WorkerSalary> Read();
    bool Create(WorkerSalary workerSalary);
}