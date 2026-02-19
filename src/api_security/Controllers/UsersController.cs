using api_security.application.Users.AddRole;
using api_security.application.Users.Create;
using api_security.application.Users.Get;
using api_security.application.Users.GetList;
using api_security.application.Users.RemoveRole;
using api_security.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace api_security.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = "admin")]
public class UsersController : ControllerBase
{
    private readonly IMediator mediator;

    public UsersController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserList([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        => this.HandleResult(await mediator.Send(new GetUserListQuery(page, pageSize, search)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
        => this.HandleResult(await mediator.Send(new GetUserQuery(id)));

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand request)
        => this.HandleResult(await mediator.Send(request));

    [HttpPost("{userId:guid}/roles")]
    public async Task<IActionResult> AddRole(Guid userId, [FromBody] AddRoleRequest request)
        => this.HandleResult(await mediator.Send(new AddRoleCommand(userId, request.RoleName)));

    [HttpDelete("{userId:guid}/roles")]
    public async Task<IActionResult> RemoveRole(Guid userId, [FromQuery] string roleName)
        => this.HandleResult(await mediator.Send(new RemoveRoleCommand(userId, roleName)));
}

public record AddRoleRequest(string RoleName);
