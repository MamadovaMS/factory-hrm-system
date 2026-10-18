using HRM_SlowGasMotorFactory_DataModels.DataModels;
using System.ComponentModel.DataAnnotations;

namespace HRM_SlowGasMotorFactory_BusinessLogics.Interfaces.Implementations;

public class PassMarksBusinessLogic(IPassMarksDatabase database) : IPassMarksBusinessLogic
{
    private readonly IPassMarksDatabase _database = database;
    private string _error = string.Empty;

    public string ErrorMessage => _error;

    public List<PassMarks> Get(Guid? workerId = null, string? passNumber = null,
                              DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        return [.. _database.Read().Where(x =>
                (!workerId.HasValue || x.WorkerId == workerId) &&
                (string.IsNullOrEmpty(passNumber) || x.PassNumber == passNumber) &&
                (!dateFrom.HasValue || x.MarkDateTimeIn >= dateFrom) &&
                (!dateTo.HasValue || x.MarkDateTimeIn <= dateTo)
            )];
    }

    public bool WorkerIn(PassMarks passMarks)
    {
        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(passMarks, new ValidationContext(passMarks), results, true);

        if (!validate)
        {
            _error = string.Join(", ", results.Select(x => x.ErrorMessage));
            return false;
        }

        var activeEntry = _database.Read()
            .FirstOrDefault(x => x.WorkerId == passMarks.WorkerId &&
                                 x.PassNumber == passMarks.PassNumber &&
                                 x.MarkDateTimeOut == null);

        if (activeEntry != null)
        {
            _error = "Сотрудник уже на рабочем месте";
            return false;
        }

        return _database.Create(passMarks);
    }

    public bool WorkerOut(PassMarks passMarks)
    {
        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(passMarks, new ValidationContext(passMarks), results, true);

        if (!validate)
        {
            _error = string.Join(", ", results.Select(x => x.ErrorMessage));
            return false;
        }

        var activeEntry = _database.Read()
            .FirstOrDefault(x => x.WorkerId == passMarks.WorkerId &&
                                 x.PassNumber == passMarks.PassNumber &&
                                 x.MarkDateTimeOut == null);

        if (activeEntry == null)
        {
            _error = "Нет активной записи входа";
            return false;
        }

        activeEntry.MarkDateTimeOut = passMarks.MarkDateTimeOut ?? DateTime.Now;
        return _database.Update(activeEntry);
    }
}