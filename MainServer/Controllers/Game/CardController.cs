using System.Net;
using Application.Common.Models;
using Application.Game.Card;
using Application.Game.Card.Management;
using Application.Game.Card.Read;
using Application.Game.Card.Session;
using Application.Game.Card.Write;
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
                        result = await Mediator.Send(new ReadCardQuery(request.CardId));
                        break;
                    case CardRequestType.ReadCardDetail:
                        result = await Mediator.Send(new ReadCardDetailQuery(request.CardId, request.Data));
                        break;
                    case CardRequestType.ReadCardDetails:
                        result = await Mediator.Send(new ReadAllCardDetailsQuery(request.CardId));
                        break;
                    case CardRequestType.ReadCardBData:
                        break;
                    case CardRequestType.ReadAvatar:
                        result = await Mediator.Send(new ReadAvatarQuery(request.CardId));
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
                        result = await Mediator.Send(new WriteAvatarCommand(request.CardId, request.Data));
                        break;
                    case CardRequestType.WriteItem:
                        result = await Mediator.Send(new WriteItemCommand(request.CardId, request.Data));
                        break;
                    case CardRequestType.WriteTitle:
                        result = await Mediator.Send(new WriteTitleCommand(request.CardId, request.Data));
                        break;
                    case CardRequestType.WriteMusicDetail:
                        result = await Mediator.Send(new WriteMusicDetailCommand(request.CardId, request.Data));
                        break;
                    case CardRequestType.WriteNavigator:
                        result = await Mediator.Send(new WriteNavigatorCommand(request.CardId, request.Data));
                        break;
                    case CardRequestType.WriteCoin:
                        result = await Mediator.Send(new WriteCoinCommand(request.CardId, request.Data));
                        break;
                    case CardRequestType.WriteSkin:
                        result = await Mediator.Send(new WriteSkinCommand(request.CardId, request.Data));
                        break;
                    case CardRequestType.WriteUnlockKeynum:
                        result = await Mediator.Send(new WriteUnlockKeynumCommand(request.CardId, request.Data));
                        break;
                    case CardRequestType.WriteSoundEffect:
                        result = await Mediator.Send(new WriteSoundEffectCommand(request.CardId, request.Data));
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
            var normalResult = "1\n" +
                               "1,1\n" +
                               $"{result.Data}";
            return Ok(normalResult);
        }

        // Here error is not null since Succeeded => Error is null; 
        var errorMessage = $"{result.Error!.Code}\n" +
                           $"{result.Error!.Message}";
        return Ok(errorMessage);
    }
}