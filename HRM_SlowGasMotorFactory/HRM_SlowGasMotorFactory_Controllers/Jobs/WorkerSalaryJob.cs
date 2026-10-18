using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces.Implementations;
using Quartz;

namespace HRM_SlowGasMotorFactory_Controllers.Jobs;

public class WorkerSalaryJob : IJob
{
    private readonly WorkerSalaryBusinessLogic _logic;

    public WorkerSalaryJob(WorkerSalaryBusinessLogic logic)
    {
        _logic = logic;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logic.CalculateSalary(DateTime.UtcNow);
        return Task.CompletedTask;
    }
}