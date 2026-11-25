using BranchstormApiClientClassLibrary.User.Entities.Dtos;
using BranchstormApiClientClassLibrary.User.Entities.Queries;
using BranchstormApiClientClassLibrary.User.Entities.Results;
using BranchstormApiClientClassLibrary.User.Services;
using Microsoft.AspNetCore.Mvc;

namespace BranchstormApiClient.Controllers;

[ApiController]
[Route("api/user")]
public class UserController(
    IUserService userService
    ) : ControllerBase
{
    private readonly IUserService userService = userService;

    [HttpPost]
    public async Task<ActionResult<UserResult>> Create([FromBody] UserDto userDto)
    {
        UserResult? user = await userService.CreateAsync(userDto);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResult>>> Find([FromQuery] FindUsersQuery query)
    {
        return Ok(await userService.FindAsync(query));
    }

    [HttpGet("{code}")]
    public async Task<ActionResult<UserResult>> FindOne([FromRoute] string code)
    {
        UserResult? user = await userService.FindOneAsync(code);
        return user is null ? NotFound() : Ok(user);
    }
}
