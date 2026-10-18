using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;
using HRM_SlowGasMotorFactory_DataModels.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM_SlowGasMotorFactory_Controllers.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PassMarksController(IPassMarksBusinessLogic businessLogic, ILogger<PassMarksController> logger) : ControllerBase
{
    private readonly IPassMarksBusinessLogic _businessLogic = businessLogic;
    private readonly ILogger<PassMarksController> _logger = logger;

    [HttpGet]
    public IActionResult Get(Guid? workerId, string? passNumber, DateTime? dateFrom, DateTime? dateTo)
    {
        try
        {
            return Ok(_businessLogic.Get(workerId, passNumber, dateFrom, dateTo));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Try get all");
            throw;
        }
    }

    [HttpPost("in")]
    public IActionResult WorkerIn(PassMarks passMark)
    {
        try
        {
            return _businessLogic.WorkerIn(passMark) ? NoContent() :
                BadRequest(_businessLogic.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Try worker in {passMark}", passMark);
            throw;
        }
    }

    [HttpPost("out")]
    public IActionResult WorkerOut(PassMarks passMark)
    {
        try
        {
            return _businessLogic.WorkerOut(passMark) ? NoContent() :
                StatusCode(StatusCodes.Status500InternalServerError, _businessLogic.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Try worker out {passMark}", passMark);
            throw;
        }
    }
}