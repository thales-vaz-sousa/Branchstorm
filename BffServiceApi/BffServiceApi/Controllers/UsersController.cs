using System.ComponentModel.DataAnnotations;
using BffApiClassLibrary.Clients.User;
using BffApiClassLibrary.Common;
using Microsoft.AspNetCore.Mvc;

namespace BffServiceApi.Controllers;

public record CreateUserRequest
{
    [Required(ErrorMessage = "Name é obrigatório")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; init; } = string.Empty;
}

/// <summary>Cadastro e busca de pessoas (RF01), usado nos seletores de autor/membro.</summary>
[ApiController]
[Route("bff/users")]
public class UsersController(IUserApiClient users) : ControllerBase
{
    private readonly IUserApiClient users = users;

    [HttpPost]
    public async Task<ActionResult<UserResult>> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        UserResult user = await users.CreateAsync(new UserCreateRequest(request.Name.Trim(), request.Email.Trim()), ct);
        return Created($"/bff/users/{user.Code}", user);
    }

    /// <summary>Busca por nome e/ou e-mail. Com "codes", resolve varios usuários de uma vez (sem limite de 100).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResult>>> Find(
        [FromQuery] string? name,
        [FromQuery] string? email,
        [FromQuery] string[]? codes,
        CancellationToken ct)
    {
        if (codes is { Length: > 0 })
        {
            return Ok(await users.FindByCodesAsync(codes, ct));
        }

        return Ok(await users.SearchAsync(name, email, ct));
    }

    [HttpGet("{code}")]
    public async Task<ActionResult<UserResult>> FindOne([FromRoute] string code, CancellationToken ct)
    {
        return Ok(await users.FindOneAsync(code, ct) ?? throw BffException.NotFound("Usuário", code));
    }
}
