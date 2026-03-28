using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace MainServer.Results;

public class GameProtocolResult : IActionResult
{
    private readonly ServiceResult<string> _result;
    private readonly string? _successSubHeader;

    public GameProtocolResult(ServiceResult<string> result, string? successSubHeader = null)
    {
        _result = result;
        _successSubHeader = successSubHeader;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.ContentType = "text/plain";

        string body;
        if (_result.Succeeded)
        {
            body = _successSubHeader is not null
                ? $"1\n{_successSubHeader}\n{_result.Data}"
                : $"1\n{_result.Data}";
        }
        else
        {
            body = $"{_result.Error!.Code}\n{_result.Error!.Message}";
        }

        await response.WriteAsync(body);
    }
}
