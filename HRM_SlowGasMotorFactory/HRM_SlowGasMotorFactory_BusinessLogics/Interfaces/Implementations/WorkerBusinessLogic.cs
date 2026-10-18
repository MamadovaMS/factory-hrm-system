using HRM_SlowGasMotorFactory_DataModels.DataModels;
using System.ComponentModel.DataAnnotations;

namespace HRM_SlowGasMotorFactory_BusinessLogics.Interfaces.Implementations;

public class WorkerBusinessLogic(IWorkerDatabase database) : IWorkerBusinessLogic
{
    private readonly IWorkerDatabase _database = database;
    private string _error = string.Empty;

    public string ErrorMessage => _error;

    public bool Add(Worker worker)
    {
        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(worker, new ValidationContext(worker), results, true);

        if (!validate)
        {
            _error = string.Join(", ", results.Select(x => x.ErrorMessage));
            return false;
        }

        var checkPersonalNumber = _database.Read().Where(x => x.PersonalNumber == worker.PersonalNumber);
        if (checkPersonalNumber.Any())
        {
            _error = "Сотрудник с таким табельным номером уже существует";
            return false;
        }

        return _database.Create(worker);
    }

    public bool Delete(Guid id)
    {
        var existingWorker = _database.Read(id);
        if (existingWorker == null)
        {
            _error = "Сотрудник не найден";
            return false;
        }

        if (existingWorker.IsDeleted)
        {
            _error = "Сотрудник уже удалён";
            return false;
        }

        return _database.Delete(id);
    }

    public Worker? Get(Guid id)
    {
        var worker = _database.Read(id);
        if (worker == null)
        {
            _error = "Сотрудник не найден";
            return null;
        }
        return worker;
    }

    public List<Worker> Get(string? personalNumber = null, string? fio = null,
                            Guid? postId = null, DateTime? birthDateStart = null,
                            DateTime? birthDateFinish = null,
                            DateTime? employmentDateStart = null,
                            DateTime? employmentDateFinish = null,
                            bool? isDeleted = false)
    {
        return [.. _database.Read().Where(x =>
                (string.IsNullOrEmpty(personalNumber) || x.PersonalNumber == personalNumber) &&
                (string.IsNullOrEmpty(fio) || x.FIO.Contains(fio)) &&
                (!postId.HasValue || x.PostId == postId) &&
                (!birthDateStart.HasValue || x.BirthDate >= birthDateStart) &&
                (!birthDateFinish.HasValue || x.BirthDate <= birthDateFinish) &&
                (!employmentDateStart.HasValue || x.EmploymentDate >= employmentDateStart) &&
                (!employmentDateFinish.HasValue || x.EmploymentDate <= employmentDateFinish) &&
                (!isDeleted.HasValue || x.IsDeleted == isDeleted)
            )];
    }

    public List<Post> GetWorkerPreviousPosts(Guid workerId)
    {
        var worker = _database.Read(workerId);
        if (worker == null)
        {
            _error = "Сотрудник не найден";
            return [];
        }

        return _database.ReadWorkerPreviousPosts(workerId);
    }

    public bool Update(Worker worker)
    {
        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(worker, new ValidationContext(worker), results, true);

        if (!validate)
        {
            _error = string.Join(", ", results.Select(x => x.ErrorMessage));
            return false;
        }

        var existingWorker = _database.Read(worker.Id);
        if (existingWorker == null)
        {
            _error = "Сотрудник не найден";
            return false;
        }

        if (existingWorker.IsDeleted)
        {
            _error = "Нельзя изменить удалённого сотрудника";
            return false;
        }

        var checkPersonalNumber = _database.Read()
            .Where(x => x.PersonalNumber == worker.PersonalNumber && x.Id != worker.Id);
        if (checkPersonalNumber.Any())
        {
            _error = "Сотрудник с таким табельным номером уже существует";
            return false;
        }

        return _database.Update(worker);
    }
}