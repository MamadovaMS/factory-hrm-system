using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;
using HRM_SlowGasMotorFactory_DataModels.DataModels;
using HRM_SlowGasMotorFactory_DataModels.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM_SlowGasMotorFactory_Controllers.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PostController(IPostBusinessLogic businessLogic, ILogger<PostController> logger) : ControllerBase
{
    private readonly IPostBusinessLogic _businessLogic = businessLogic;
    private readonly ILogger<PostController> _logger = logger;

    [HttpGet]
    public IActionResult Get(string? postName, PostType? postType, double? salaryMin, double? salaryMax)
    {
        try
        {
            return Ok(_businessLogic.Get(postName, postType, salaryMin, salaryMax));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Try get all");
            throw;
        }
    }

    [HttpGet("{id}")]
    public IActionResult Get(Guid id)
    {
        try
        {
            var record = _businessLogic.Get(id);
            return record == null ? BadRequest(_businessLogic.ErrorMessage) : Ok(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Try get by id {id}", id);
            throw;
        }
    }

    [HttpPost]
    public IActionResult Post(Post post)
    {
        try
        {
            return _businessLogic.Add(post) ? NoContent() :
                StatusCode(StatusCodes.Status500InternalServerError, _businessLogic.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Try add post {post}", post);
            throw;
        }
    }

    [HttpPut]
    public IActionResult Put(Post post)
    {
        try
        {
            return _businessLogic.Update(post) ? NoContent() :
                StatusCode(StatusCodes.Status500InternalServerError, _businessLogic.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Try update post {post}", post);
            throw;
        }
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        try
        {
            return _businessLogic.Delete(id) ? NoContent() :
                StatusCode(StatusCodes.Status500InternalServerError, _businessLogic.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Try delete by id {id}", id);
            throw;
        }
    }
}