using Microsoft.AspNetCore.Mvc;
using UserApiClassLibrary.User.Entities.Dtos;
using UserApiClassLibrary.User.Entities.Queries;
using UserApiClassLibrary.User.Entities.Results;
using UserApiClassLibrary.User.Services;

namespace UserServiceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(
        IUserService userService
        ): ControllerBase
    {
        private readonly IUserService userService = userService;

        [HttpPost]
        public IActionResult CreateUser([FromBody] UserDto userDto)
        {
            return Ok(userService.CreateAsync(userDto));
        }

        [HttpGet]
        public ActionResult<IEnumerable<UserResult>> Find()
        {
            return Ok(userService.FindAsync());
        }

        [HttpGet("search")]
        public IActionResult FindOne([FromQuery] FindUserQuery query)
        {
            return Ok(userService.FindOneAsync(query));
        }
    }
}
