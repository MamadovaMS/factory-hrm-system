using System.ComponentModel.DataAnnotations;

namespace HRM_SlowGasMotorFactory_DataModels.CustomValidationAttributes;

[AttributeUsage(AttributeTargets.Property)]
public class NoFutureDateAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is DateTime date)
        {
            return date <= DateTime.Today;
        }
        return false;
    }
}