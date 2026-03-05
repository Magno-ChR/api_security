using api_security.application.Authentication.Commands;
using api_security.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api_security.Controllers;

[Route("api/security/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthenticationsController : ControllerBase
{
    private readonly IMediator mediator;

    public AuthenticationsController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginCommand request)
        => this.HandleResult(await mediator.Send(request));

}
