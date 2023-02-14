using System.Diagnostics;
using Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MainServer.Filters;

/// <summary>
/// Exception filters
/// </summary>
public class ApiExceptionFilterService : ExceptionFilterAttribute
{
    private readonly IDictionary<Type, Action<ExceptionContext>> exceptionHandlers;

    private readonly ILogger<ApiExceptionFilterService> logger;

    /// <summary>
    /// Constructor
    /// </summary>
    public ApiExceptionFilterService(ILogger<ApiExceptionFilterService> logger)
    {
        this.logger = logger;
        // Register known exception types and handlers.
        exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
        {
            { typeof(ArgumentOutOfRangeException), HandleArgumentOutOfRangeException }
        };
    }

    /// <summary>
    /// On exception event
    /// </summary>
    /// <param name="context"></param>
    public override void OnException(ExceptionContext context)
    {
        HandleException(context);

        base.OnException(context);
    }

    private void HandleException(ExceptionContext context)
    {
        var type = context.Exception.GetType();
        if (exceptionHandlers.ContainsKey(type))
        {
            exceptionHandlers[type].Invoke(context);
            return;
        }

        HandleUnknownException(context);
    }

    private static void HandleUnknownException(ExceptionContext context)
    {
        var details = ServiceResult.Failed(ServiceError.DefaultError);

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };

        context.ExceptionHandled = true;
    }

    private void HandleArgumentOutOfRangeException(ExceptionContext context)
    {
        logger.LogError(context.Exception, "Get an argument out of bound exception");
        var exception = context.Exception as ArgumentOutOfRangeException;
        Debug.Assert(exception != null, nameof(exception) + " != null");

        var variable = exception.ParamName ?? "Unknown";
        var details = ServiceResult.Failed(ServiceError.CustomMessage($"Argument {variable} out of bounds!"));

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status400BadRequest
        };

        context.ExceptionHandled = true;
    }
}