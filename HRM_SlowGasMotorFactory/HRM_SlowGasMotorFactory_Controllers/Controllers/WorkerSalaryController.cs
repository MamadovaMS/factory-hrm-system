using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM_SlowGasMotorFactory_Controllers.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WorkerSalaryController : ControllerBase
{
    private readonly IWorkerSalaryBusinessLogic _businessLogic;
    private readonly ILogger<WorkerSalaryController> _logger;

    public WorkerSalaryController(IWorkerSalaryBusinessLogic businessLogic, ILogger<WorkerSalaryController> logger)
    {
        _businessLogic = businessLogic;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get(Guid? workerId, DateTime? calculateDateStart, DateTime? calculateDateFinish)
    {
        try
        {
            return Ok(_businessLogic.Get(workerId, calculateDateStart, calculateDateFinish));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Try get all");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public IActionResult Calculate([FromQuery] DateTime date)
    {
        try
        {
            if (date == default)
                return BadRequest("Date is required");

            _businessLogic.CalculateSalary(date);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Try calculate salary");
            return StatusCode(500, "Internal server error");
        }
    }
}