using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;
using HRM_SlowGasMotorFactory_DataModels.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM_SlowGasMotorFactory_Controllers.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WorkerController(IWorkerBusinessLogic businessLogic, ILogger<WorkerController> logger) : ControllerBase
{
    private readonly IWorkerBusinessLogic _businessLogic = businessLogic;
    private readonly ILogger<WorkerController> _logger = logger;

    [HttpGet]
    public IActionResult Get(string? personalNumber, string? fio, Guid? postId, DateTime? birthDateStart,
        DateTime? birthDateFinish, DateTime? employmentDateStart, DateTime? employmentDateFinish, bool isDelete = false)
    {
        try
        {
            return Ok(_businessLogic.Get(personalNumber, fio, postId, birthDateStart, birthDateFinish,
                employmentDateStart, employmentDateFinish, isDelete));
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
    public IActionResult Post(Worker worker)
    {
        try
        {
            return _businessLogic.Add(worker) ? NoContent() :
                StatusCode(StatusCodes.Status500InternalServerError, _businessLogic.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Try add worker {worker}", worker);
            throw;
        }
    }

    [HttpPut]
    public IActionResult Put(Worker worker)
    {
        try
        {
            return _businessLogic.Update(worker) ? NoContent() :
                StatusCode(StatusCodes.Status500InternalServerError, _businessLogic.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Try update worker {worker}", worker);
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