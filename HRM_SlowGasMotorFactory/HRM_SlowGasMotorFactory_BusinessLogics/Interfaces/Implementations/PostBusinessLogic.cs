using HRM_SlowGasMotorFactory_DataModels.DataModels;
using HRM_SlowGasMotorFactory_DataModels.Enums;
using System.ComponentModel.DataAnnotations;

namespace HRM_SlowGasMotorFactory_BusinessLogics.Interfaces.Implementations;

public class PostBusinessLogic(IPostDatabase database) : IPostBusinessLogic
{
    private readonly IPostDatabase _database = database;
    private string _error = string.Empty;

    public string ErrorMessage => _error;

    public bool Add(Post post)
    {
        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(post, new ValidationContext(post), results, true);

        if (!validate)
        {
            _error = string.Join(", ", results.Select(x => x.ErrorMessage));
            return false;
        }

        var checkPostName = _database.Read().Where(x => x.PostName == post.PostName);
        if (checkPostName.Any())
        {
            _error = "Есть должность с таким названием";
            return false;
        }

        return _database.Create(post);
    }

    public bool Delete(Guid id)
    {
        var checkPost = _database.Read(id);
        if (checkPost == null)
        {
            _error = "Должность не найдена";
            return false;
        }

        return _database.Delete(id);
    }

    public Post? Get(Guid id)
    {
        var post = _database.Read(id);
        if (post == null)
        {
            _error = "Должность не найдена";
            return null;
        }
        return post;
    }

    public List<Post> Get(string? postName = null, PostType? postType = null,
                         double? salaryMin = null, double? salaryMax = null)
    {
        return [.. _database.Read().Where(x =>
                (string.IsNullOrEmpty(postName) || x.PostName == postName) &&
                (!postType.HasValue || x.PostType == postType) &&
                (!salaryMin.HasValue || x.SalaryPerHour >= salaryMin) &&
                (!salaryMax.HasValue || x.SalaryPerHour <= salaryMax)
            )];
    }

    public bool Update(Post post)
    {
        var results = new List<ValidationResult>();
        var validate = Validator.TryValidateObject(post, new ValidationContext(post), results, true);

        if (!validate)
        {
            _error = string.Join(", ", results.Select(x => x.ErrorMessage));
            return false;
        }

        var checkPost = _database.Read(post.Id);
        if (checkPost == null)
        {
            _error = "Должность не найдена";
            return false;
        }

        var checkPostName = _database.Read().Where(x => x.PostName == post.PostName && x.Id != post.Id);
        if (checkPostName.Any())
        {
            _error = "Есть должность с таким названием";
            return false;
        }

        return _database.Update(post);
    }
}