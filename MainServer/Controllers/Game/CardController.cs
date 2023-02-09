using System.Net;
using Application.Common.Models;
using Application.Game.Card;
using Application.Game.Card.Management;
using Application.Game.Card.Session;
using Domain;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Throw;

namespace MainServer.Controllers.Game;

[ApiController]
[Route("service/card")]
public class CardController : BaseController<CardController>
{
    [HttpPost("cardn.cgi")]
    public async Task<ActionResult<string>> CardService([FromForm]CardRequest request)
    {
        var cardRequestType = (CardRequestType)request.CardRequestType;
        var cardCommandType = (CardCommandType)request.CardCommandType;
        
        cardCommandType.Throw().IfOutOfRange();
        if (cardCommandType is CardCommandType.CardReadRequest or CardCommandType.CardWriteRequest)
        {
            cardRequestType.Throw().IfOutOfRange();
        }

        request.Data = WebUtility.UrlDecode(request.Data);
        var result = ServiceResult.Failed<string>(ServiceError.DefaultError);
        switch (cardCommandType)
        {
            case CardCommandType.CardReadRequest:
            case CardCommandType.CardWriteRequest:
            {
                switch (cardRequestType)
                {
                    case CardRequestType.ReadCard:
                        break;
                    case CardRequestType.ReadCardDetail:
                        break;
                    case CardRequestType.ReadCardDetails:
                        break;
                    case CardRequestType.ReadCardBData:
                        break;
                    case CardRequestType.ReadAvatar:
                        break;
                    case CardRequestType.ReadItem:
                        break;
                    case CardRequestType.ReadSkin:
                        break;
                    case CardRequestType.ReadTitle:
                        break;
                    case CardRequestType.ReadMusic:
                        break;
                    case CardRequestType.ReadEventReward:
                        break;
                    case CardRequestType.ReadNavigator:
                        break;
                    case CardRequestType.ReadMusicExtra:
                        break;
                    case CardRequestType.ReadMusicAou:
                        break;
                    case CardRequestType.ReadCoin:
                        break;
                    case CardRequestType.ReadUnlockReward:
                        break;
                    case CardRequestType.ReadUnlockKeynum:
                        break;
                    case CardRequestType.ReadSoundEffect:
                        break;
                    case CardRequestType.ReadGetMessage:
                        break;
                    case CardRequestType.ReadCond:
                        break;
                    case CardRequestType.ReadTotalTrophy:
                        break;
                    case CardRequestType.GetSession:
                    case CardRequestType.StartSession:
                        result = await Mediator.Send(new GetSessionCommand(request.CardId, request.Mac));
                        break;
                    case CardRequestType.WriteCard:
                        
                        break;
                    case CardRequestType.WriteCardDetail:
                        break;
                    case CardRequestType.WriteCardBData:
                        break;
                    case CardRequestType.WriteAvatar:
                        break;
                    case CardRequestType.WriteItem:
                        break;
                    case CardRequestType.WriteTitle:
                        break;
                    case CardRequestType.WriteMusicDetail:
                        break;
                    case CardRequestType.WriteNavigator:
                        break;
                    case CardRequestType.WriteCoin:
                        break;
                    case CardRequestType.WriteSkin:
                        break;
                    case CardRequestType.WriteUnlockKeynum:
                        break;
                    case CardRequestType.WriteSoundEffect:
                        break;
                    case CardRequestType.StartOnlineMatching:
                        break;
                    case CardRequestType.UpdateOnlineMatching:
                        break;
                    case CardRequestType.UploadOnlineMatchingResult:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(message: "Should not happen", paramName:null);
                }
                break;
            }
            case CardCommandType.RegisterRequest:
                result = await Mediator.Send(new CardRegisterCommand(request.CardId, request.Data));
                break;
            case CardCommandType.ReissueRequest:
                result = await Mediator.Send(new CardReissueCommand(request.CardId));
                break;
            default:
                throw new ArgumentOutOfRangeException(message: "Should not happen", paramName:null);
        }

        if (result.Succeeded)
        {
            return Ok(result.Data);
        }

        var errorMessage = $"{(int)CardReturnCode.Unknown}\n" +
                           $"{result.Error!.Message}";
        return Ok(errorMessage);
    }
}