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
        public async Task<ActionResult<UserResult>> CreateUser([FromBody] UserDto userDto)
        {
            return Ok(await userService.CreateAsync(userDto));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResult>>> Find([FromQuery] FindUsersQuery query)
        {
            return Ok(await userService.FindAsync(query));
        }

        [HttpGet("{code}")]
        public async Task<ActionResult<UserResult>> FindOne(string code)
        {
            UserResult? user = await userService.FindOneAsync(code);
            return user is null ? NotFound() : Ok(user);
        }
    }
}
