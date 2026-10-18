using HRM_SlowGasMotorFactory_DataModels.DataModels;

namespace HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;

public interface IPassMarksDatabase
{
    List<PassMarks> Read();
    bool Create(PassMarks passMarks);
    bool Update(PassMarks passMarks);
}