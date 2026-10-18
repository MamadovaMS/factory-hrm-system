using HRM_SlowGasMotorFactory_DataModels.CustomValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace HRM_SlowGasMotorFactory_DataModels.DataModels;

public class Worker
{
    [GuidNoEmpty]
    public Guid Id { get; set; }

    [Required]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Персональный номер должен быть 10 символов")]
    public string PersonalNumber { get; set; } = string.Empty;

    [GuidNoEmpty]
    public Guid PostId { get; set; }

    public Post? Post { get; set; }

    [Required]
    [RegularExpression(@"^[А-ЯЁ][а-яё]+\s[А-ЯЁ][а-яё]+(\s[А-ЯЁ][а-яё]+)?$",
        ErrorMessage = "ФИО должно быть в формате 'Фамилия Имя [Отчество]'")]
    public string FIO { get; set; } = string.Empty;

    [BirthDate(ErrorMessage = "Сотрудник должен быть старше 16 лет")]
    public DateTime BirthDate { get; set; }

    [DataType(DataType.Date)]
    [NoFutureDate(ErrorMessage = "Дата трудоустройства не может быть в будущем")]
    public DateTime EmploymentDate { get; set; }

    public bool IsDeleted { get; set; }

    public List<PassMarks> PassMarks { get; set; } = [];
}