using System.ComponentModel.DataAnnotations;

namespace HRM_SlowGasMotorFactory_DataModels.CustomValidationAttributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class GuidNoEmptyAttribute : ValidationAttribute
{
    public GuidNoEmptyAttribute()
    {
        ErrorMessage = $"Значение не должно быть {Guid.Empty}";
    }

    public override bool IsValid(object? value)
    {
        return value != null && Guid.TryParse(value.ToString(), out var guidValue) && guidValue != Guid.Empty;
    }
}