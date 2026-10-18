using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM_SlowGasMotorFactory_Controllers.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WorkerPostHistoryController(IWorkerBusinessLogic businessLogic, ILogger<WorkerPostHistoryController> logger) : ControllerBase
{
    private readonly IWorkerBusinessLogic _businessLogic = businessLogic;
    private readonly ILogger<WorkerPostHistoryController> _logger = logger;

    [HttpGet("{workerId}")]
    public IActionResult Get(Guid workerId)
    {
        try
        {
            var posts = _businessLogic.GetWorkerPreviousPosts(workerId);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Try get post history for worker {workerId}", workerId);
            throw;
        }
    }
}