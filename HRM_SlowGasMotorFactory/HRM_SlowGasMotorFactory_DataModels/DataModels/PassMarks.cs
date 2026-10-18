using HRM_SlowGasMotorFactory_DataModels.CustomValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace HRM_SlowGasMotorFactory_DataModels.DataModels;

public class PassMarks
{
    [GuidNoEmpty]
    public Guid Id { get; set; }

    [GuidNoEmpty]
    public Guid WorkerId { get; set; }

    public Worker? Worker { get; set; }

    [Required]
    [MinLength(3, ErrorMessage = "Номер пропуска должен быть не менее 3 символов")]
    [MaxLength(10, ErrorMessage = "Номер пропуска должен быть не более 10 символов")]
    public string PassNumber { get; set; } = string.Empty;

    [CurrentDate(ErrorMessage = "Дата входа должна быть сегодня")]
    public DateTime MarkDateTimeIn { get; set; } = DateTime.UtcNow;

    public DateTime? MarkDateTimeOut { get; set; }
}