using HRM_SlowGasMotorFactory_DataModels.DataModels;

namespace HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;

public interface IPassMarksBusinessLogic
{
    string ErrorMessage { get; }

    List<PassMarks> Get(Guid? workerId = null, string? passNumber = null,
                      DateTime? dateFrom = null, DateTime? dateTo = null);
    bool WorkerIn(PassMarks passMarks);
    bool WorkerOut(PassMarks passMarks);
}