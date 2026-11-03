using Microsoft.AspNetCore.Mvc;
using TeamApiClassLibrary.Team.Entities.Dtos;

namespace TeamServiceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamController : ControllerBase
    {

        [HttpPost]
        public IActionResult CreateTeam([FromBody] TeamDto createTeamDto)
        {
            
            return Ok();
        }

        [HttpGet("{code}")]
        public IActionResult FindTeam ([FromRoute] string code)
        {
            return Ok();
        }

        [HttpPatch("{code}")]
        public IActionResult UpdateTeamMembers([FromBody] TeamDto teamDto)
        {
            return Ok();
        }
    }
}
