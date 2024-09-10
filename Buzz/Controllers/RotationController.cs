using Microsoft.AspNetCore.Mvc;

namespace Buzz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RotationController : ControllerBase
    {
        private readonly RotationService _rotationService;

        public RotationController(RotationService rotationService)
        {
            _rotationService = rotationService;
        }

        [HttpGet]
        public ActionResult<List<TaskAssignment>> GetRotationList()
        {
            var rotationList = _rotationService.GetRotationList();
            return Ok(rotationList); 
        }
    }
}