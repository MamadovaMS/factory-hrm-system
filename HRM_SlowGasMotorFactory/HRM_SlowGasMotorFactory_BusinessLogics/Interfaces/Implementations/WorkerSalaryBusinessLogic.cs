using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces.Business;
using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces.Database;
using HRM_SlowGasMotorFactory_DataModels.DataModels;
using System.ComponentModel.DataAnnotations;

namespace HRM_SlowGasMotorFactory_BusinessLogics.Interfaces.Implementations;

public class WorkerSalaryBusinessLogic : IWorkerSalaryBusinessLogic
{
    private readonly IWorkerSalaryDatabase _database;
    private readonly IWorkerDatabase _workerDatabase;
    private readonly IPassMarksDatabase _passMarksDatabase;
    private readonly ISalesInfoClient _salesInfoClient;
    private string _error = string.Empty;

    public WorkerSalaryBusinessLogic(
        IWorkerSalaryDatabase database,
        IWorkerDatabase workerDatabase,
        IPassMarksDatabase passMarksDatabase,
        ISalesInfoClient salesInfoClient)
    {
        _database = database;
        _workerDatabase = workerDatabase;
        _passMarksDatabase = passMarksDatabase;
        _salesInfoClient = salesInfoClient;
    }

    public string ErrorMessage => _error;

    public List<WorkerSalary> Get(Guid? workerId = null, DateTime? calculateDateStart = null, DateTime? calculateDateFinish = null)
    {
        var salaries = _database.Read();

        var query = salaries.AsEnumerable();

        if (workerId.HasValue)
            query = query.Where(x => x.WorkerId == workerId.Value);

        if (calculateDateStart.HasValue)
            query = query.Where(x => x.CalculateDate >= calculateDateStart.Value);

        if (calculateDateFinish.HasValue)
            query = query.Where(x => x.CalculateDate <= calculateDateFinish.Value);

        return query.ToList();
    }

    public void CalculateSalary(DateTime date)
    {
        var beforeMonthDate = date.AddMonths(-1);
        var startDate = new DateTime(beforeMonthDate.Year, beforeMonthDate.Month, 1);
        var finishDate = startDate.AddMonths(1).AddDays(-1);

        var workers = _workerDatabase.Read().Where(x => !x.IsDeleted).ToList();

        var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

        Parallel.ForEach(workers, options, worker =>
        {
            try
            {
                var marks = _passMarksDatabase.Read()
                    .Where(x => x.WorkerId == worker.Id &&
                                x.MarkDateTimeIn.Date >= startDate &&
                                x.MarkDateTimeOut.HasValue &&
                                x.MarkDateTimeOut.Value.Date <= finishDate)
                    .ToList();

                var work = marks
                    .Select(x => (x.MarkDateTimeOut!.Value - x.MarkDateTimeIn).TotalHours * (worker.Post?.SalaryPerHour ?? 0))
                    .Sum();

                var sales = _salesInfoClient.GetSales(worker.Id, startDate, finishDate) ?? new List<SaleInfo>();
                var percent = sales.Select(x => x.Sum * 0.2).Sum();

                var workerSalary = new WorkerSalary
                {
                    Id = Guid.NewGuid(),
                    WorkerId = worker.Id,
                    CalculateDate = date,
                    Salary = Math.Round(work + percent, 2)
                };

                var validationResults = new List<ValidationResult>();
                var isValid = Validator.TryValidateObject(workerSalary, new ValidationContext(workerSalary), validationResults, true);

                if (isValid)
                {
                    _database.Create(workerSalary);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        });
    }
}