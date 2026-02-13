using api_security.application.Users.Create;
using api_security.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api_security.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "admin")]
public class UsersController : ControllerBase
{
    private readonly IMediator mediator;

    public UsersController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand request)
        => this.HandleResult(await mediator.Send(request));

}
