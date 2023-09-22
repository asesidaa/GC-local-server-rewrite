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
    private readonly MachineConfig machineConfig;


    public CertifyCommandHandler(IOptions<RelayConfig> relayOptions,IOptions<MachineConfig> machineOptions)
    {
        relayConfig = relayOptions.Value;
        machineConfig = machineOptions.Value;
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

        if (!MacValid(request.Mac))
        {
            return Task.FromResult(QuitWithError(ErrorCode.ErrorInvalidMac));
        }

        if (!Md5Valid(request.Md5))
        {
            return Task.FromResult(QuitWithError(ErrorCode.ErrorInvalidHash));
        }

        string ticket;
        string response;
       
        if (machineConfig.MachineCheckEnable)
        {
            if (CheckMac(request.Mac))
            {
                var TInfo = GetTenpoInfo(request.Mac);
                ticket = string.Join(string.Empty,
                   MD5.HashData(Encoding.UTF8.GetBytes(request.Gid)).Select(b => b.ToString("x2")));

                response = $"host=card_id=7020392000147361,relay_addr={relayConfig.RelayServer},relay_port={relayConfig.RelayPort}\n" +
                              $"no={TInfo[0]}\n" + //店舗ID
                              $"name={TInfo[1]}\n" +//店舗
                              $"pref={TInfo[2]}\n" + //県
                              $"addr={TInfo[3]}\n" + //住所
                              "x-next-time=15\n" +
                              $"x-img=http://{request.Host}/news.png\n" +
                              $"x-ranking=http://{request.Host}/ranking/ranking.php\n" +
                              $"ticket={ticket}";

                return Task.FromResult(response);

            }
            else
            {
                return Task.FromResult(QuitWithError(ErrorCode.ErrorInvalidMac));
            }
        }

        ticket = string.Join(string.Empty,
           MD5.HashData(Encoding.UTF8.GetBytes(request.Gid)).Select(b => b.ToString("x2")));

        response = $"host=card_id=7020392000147361,relay_addr={relayConfig.RelayServer},relay_port={relayConfig.RelayPort}\n" +
                      "no=1337\n" +
                      "name=GCLocalServer\n" +
                      "pref=nesys\n" +
                      "addr=Local\n" +
                      "x-next-time=15\n" +
                      $"x-img=http://{request.Host}/news.png\n" +
                      $"x-ranking=http://{request.Host}/ranking/ranking.php\n" +
                      $"ticket={ticket}";

        return Task.FromResult(response);
    }
    private  bool CheckMac(string mac)
    {
        foreach (var t in machineConfig.Machines)
        {
            if (t.Mac == mac) { return true; }
        }

        return false;
    }
    private  List<string> GetTenpoInfo(string mac)
    {
        var list = new List<string>();
        foreach (var t in machineConfig.Machines)
        {
            if (t.Mac == mac)
            {
                list.Add(t.TenpoID);
                list.Add(t.Name);
                list.Add(t.Pref);
                list.Add(t.Location);
                break;
            }
        }
        return list;
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
