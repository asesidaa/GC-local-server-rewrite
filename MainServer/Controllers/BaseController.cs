using MainServer.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace MainServer.Controllers;

public abstract class BaseController<T> : ControllerBase where T : BaseController<T>
{
    private ILogger<T>? logger;

    private ISender? mediator;

    protected ISender Mediator => (mediator ??= HttpContext.RequestServices.GetService<ISender>()) ?? throw new InvalidOperationException();

    protected ILogger<T> Logger => (logger ??= HttpContext.RequestServices.GetService<ILogger<T>>()) ?? throw new InvalidOperationException();

    protected IActionResult GameResult(ServiceResult<string> result) =>
        new GameProtocolResult(result);

    protected IActionResult CardGameResult(ServiceResult<string> result) =>
        new GameProtocolResult(result, "1,1");
}