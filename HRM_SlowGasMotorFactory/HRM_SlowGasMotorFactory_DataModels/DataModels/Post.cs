using HRM_SlowGasMotorFactory_DataModels.CustomValidationAttributes;
using HRM_SlowGasMotorFactory_DataModels.Enums;
using System.ComponentModel.DataAnnotations;

namespace HRM_SlowGasMotorFactory_DataModels.DataModels;

public class Post
{
    [GuidNoEmpty]
    public Guid Id { get; set; }

    [Required]
    [MinLength(10, ErrorMessage = "Название должности должно быть не менее 10 символов")]
    public string PostName { get; set; } = string.Empty;

    [PostTypeNoNone]
    public PostType PostType { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Ставка за час должна быть больше 0")]
    public double SalaryPerHour { get; set; }

    public List<Worker> Workers { get; set; } = [];
}