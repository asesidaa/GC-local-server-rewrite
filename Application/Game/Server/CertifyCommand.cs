using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Domain.Config;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.Game.Server;

public record CertifyCommand(string? Gid, string? Mac, string? Random, string? Md5, string Host) : IRequest<string>;

public partial class CertifyCommandHandler : IRequestHandler<CertifyCommand, string>
{
    private readonly RelayConfig relayConfig;

    private readonly AuthConfig authConfig;

    public CertifyCommandHandler(IOptions<RelayConfig> relayOptions, IOptions<AuthConfig> authOptions)
    {
        relayConfig = relayOptions.Value;
        authConfig = authOptions.Value;
    }

    public Task<string> Handle(CertifyCommand request, CancellationToken cancellationToken)
    {
        if (request.Gid == null)
        {
            return Task.FromResult(QuitWithError(ErrorCode.ErrorNoGid));
        }

        if (request.Mac == null)
        {
            return Task.FromResult(QuitWithError(ErrorCode.ErrorNoMac));
        }

        if (request.Random == null)
        {
            return Task.FromResult(QuitWithError(ErrorCode.ErrorNoRandom));
        }

        if (request.Md5 == null)
        {
            return Task.FromResult(QuitWithError(ErrorCode.ErrorNoHash));
        }

        if (!MacValid(request.Mac) )
        {
            return Task.FromResult(QuitWithError(ErrorCode.ErrorInvalidMac));
        }

        if (!Md5Valid(request.Md5))
        {
            return Task.FromResult(QuitWithError(ErrorCode.ErrorInvalidHash));
        }
        var machine = new Machine
        {
            TenpoId = "1337",
            TenpoName = "GCLocalServer",
            Pref = "nesys",
            Location = "Local",
            Mac = request.Mac
        };
        if (authConfig.Enabled)
        {
            machine = authConfig.Machines.FirstOrDefault(m => m.Mac == request.Mac);
            if (machine is null)
            {
                return Task.FromResult(QuitWithError(ErrorCode.ErrorInvalidMac));
            }
        }
        
        var ticket = string.Join(string.Empty, 
            MD5.HashData(Encoding.UTF8.GetBytes(request.Gid)).Select(b => b.ToString("x2")));
        
        var response = $"host=card_id=7020392000147361,relay_addr={relayConfig.RelayServer},relay_port={relayConfig.RelayPort}\n" +
                       $"no={machine.TenpoId}\n" +
                       $"name={machine.TenpoName}\n" +
                       $"pref={machine.Pref}\n" +
                       $"addr={machine.Location}\n" +
                       "x-next-time=15\n" +
                       $"x-img=http://{request.Host}/news.png\n" +
                       $"x-ranking=http://{request.Host}/ranking/ranking.php\n" +
                       $"ticket={ticket}";

        return Task.FromResult(response);
    }

    private static bool MacValid(string mac)
    {
        return MacRegex().IsMatch(mac);
    }

    private static bool Md5Valid(string md5)
    {
        return Md5Regex().IsMatch(md5);
    }
    
    private static string QuitWithError(ErrorCode errorCode)
    {
        return $"error={(int)errorCode}";
    }

    private enum ErrorCode
    {
        ErrorNoGid,
        ErrorNoMac,
        ErrorNoRandom,
        ErrorNoHash,
        ErrorInvalidMac,
        ErrorInvalidHash
    }

    [GeneratedRegex("^[a-fA-F0-9]{12}$")]
    private static partial Regex MacRegex();
    [GeneratedRegex("^[a-fA-F0-9]{32}$")]
    private static partial Regex Md5Regex();
}
