using HRM_SlowGasMotorFactory_DataModels.DataModels;
using HRM_SlowGasMotorFactory_DataModels.Enums;

namespace HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;

public interface IPostBusinessLogic
{
    string ErrorMessage { get; }

    List<Post> Get(string? postName = null, PostType? postType = null, double? salaryMin = null, double? salaryMax = null);
    Post? Get(Guid id);
    bool Add(Post post);
    bool Update(Post post);
    bool Delete(Guid id);
}