using HRM_SlowGasMotorFactory_DataModels.CustomValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace HRM_SlowGasMotorFactory_DataModels.DataModels;

public class WorkerSalary
{
    [GuidNoEmpty]
    public Guid Id { get; set; }

    [GuidNoEmpty]
    public Guid WorkerId { get; set; }

    public Worker? Worker { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Зарплата должна быть больше 0")]
    public double Salary { get; set; }

    [NoFutureDate(ErrorMessage = "Дата расчёта не может быть в будущем")]
    public DateTime CalculateDate { get; set; }
}