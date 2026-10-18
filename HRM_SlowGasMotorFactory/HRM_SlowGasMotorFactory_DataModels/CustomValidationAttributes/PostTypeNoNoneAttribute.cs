using HRM_SlowGasMotorFactory_DataModels.Enums;
using System.ComponentModel.DataAnnotations;

namespace HRM_SlowGasMotorFactory_DataModels.CustomValidationAttributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class PostTypeNoNoneAttribute : ValidationAttribute
{
    public PostTypeNoNoneAttribute()
    {
        ErrorMessage = "Тип должности не может быть None";
    }

    public override bool IsValid(object? value)
    {
        if (value is PostType postType)
        {
            return postType != PostType.None;
        }
        return false;
    }
}