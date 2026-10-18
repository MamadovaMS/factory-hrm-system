using System.ComponentModel.DataAnnotations;

namespace HRM_SlowGasMotorFactory_DataModels.CustomValidationAttributes;

[AttributeUsage(AttributeTargets.Property)]
public class BirthDateAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is DateTime birthDate)
        {
            var age = DateTime.Today.Year - birthDate.Year;
            if (birthDate > DateTime.Today.AddYears(-age)) age--;
            return age >= 16;
        }
        return false;
    }
}