using System.ComponentModel.DataAnnotations;

namespace HRM_SlowGasMotorFactory_DataModels.CustomValidationAttributes;

[AttributeUsage(AttributeTargets.Property)]
public class CurrentDateAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null) return true;

        if (value is DateTime date)
        {
            return date.Date == DateTime.Today.Date;
        }
        return false;
    }
}