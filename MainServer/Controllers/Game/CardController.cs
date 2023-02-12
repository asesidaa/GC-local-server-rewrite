using System.Net;
using Application.Common.Models;
using Application.Game.Card;
using Application.Game.Card.Management;
using Application.Game.Card.Read;
using Application.Game.Card.Session;
using Application.Game.Card.Write;
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
                        result = await Mediator.Send(new ReadCardBDataQuery(request.CardId));
                        break;
                    case CardRequestType.ReadAvatar:
                        result = await Mediator.Send(new ReadAvatarQuery(request.CardId));
                        break;
                    case CardRequestType.ReadItem:
                        result = await Mediator.Send(new ReadItemQuery(request.CardId));
                        break;
                    case CardRequestType.ReadSkin:
                        result = await Mediator.Send(new ReadSkinQuery(request.CardId));
                        break;
                    case CardRequestType.ReadTitle:
                        result = await Mediator.Send(new ReadTitleQuery(request.CardId));
                        break;
                    case CardRequestType.ReadNavigator:
                        result = await Mediator.Send(new ReadNavigatorQuery(request.CardId));
                        break;
                    case CardRequestType.ReadSoundEffect:
                        result = await Mediator.Send(new ReadSoundEffectQuery(request.CardId));
                        break;
                    case CardRequestType.ReadMusic:
                        result = await Mediator.Send(new ReadMusicQuery(request.CardId));
                        break;
                    case CardRequestType.ReadMusicAou:
                        result = await Mediator.Send(new ReadMusicAouQuery(request.CardId));
                        break;
                    case CardRequestType.ReadMusicExtra:
                        result = await Mediator.Send(new ReadMusicExtraQuery(request.CardId));
                        break;
                    case CardRequestType.ReadEventReward:
                        result = await Mediator.Send(new ReadEventRewardQuery(request.CardId));
                        break;
                    case CardRequestType.ReadCoin:
                        result = await Mediator.Send(new ReadCoinQuery(request.CardId));
                        break;
                    case CardRequestType.ReadUnlockReward:
                        result = await Mediator.Send(new ReadUnlockRewardQuery(request.CardId));
                        break;
                    case CardRequestType.ReadUnlockKeynum:
                        result = await Mediator.Send(new ReadUnlockKeynumQuery(request.CardId));
                        break;
                    case CardRequestType.ReadGetMessage:
                        result = await Mediator.Send(new ReadGetMessageQuery(request.CardId));
                        break;
                    case CardRequestType.ReadCond:
                        result = await Mediator.Send(new ReadCondQuery(request.CardId));
                        break;
                    case CardRequestType.ReadTotalTrophy:
                        result = await Mediator.Send(new ReadTotalTrophyQuery(request.CardId));
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