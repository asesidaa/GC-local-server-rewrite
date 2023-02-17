using System.Net;
using Application.Game.Card;
using Application.Game.Card.Management;
using Application.Game.Card.OnlineMatching;
using Application.Game.Card.Read;
using Application.Game.Card.Session;
using Application.Game.Card.Write;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
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
        ServiceResult<string> result;
        switch (cardCommandType)
        {
            case CardCommandType.CardReadRequest:
            case CardCommandType.CardWriteRequest:
            {
                result = cardRequestType switch
                {
                    CardRequestType.ReadCard => await Mediator.Send(new ReadCardQuery(request.CardId)),
                    CardRequestType.ReadCardDetail => await Mediator.Send(
                        new ReadCardDetailQuery(request.CardId, request.Data)),
                    CardRequestType.ReadCardDetails => await Mediator.Send(new ReadAllCardDetailsQuery(request.CardId)),
                    CardRequestType.ReadCardBData => await Mediator.Send(new ReadCardBDataQuery(request.CardId)),
                    CardRequestType.ReadAvatar => await Mediator.Send(new ReadAvatarQuery(request.CardId)),
                    CardRequestType.ReadItem => await Mediator.Send(new ReadItemQuery(request.CardId)),
                    CardRequestType.ReadSkin => await Mediator.Send(new ReadSkinQuery(request.CardId)),
                    CardRequestType.ReadTitle => await Mediator.Send(new ReadTitleQuery(request.CardId)),
                    CardRequestType.ReadMusic => await Mediator.Send(new ReadMusicQuery(request.CardId)),
                    CardRequestType.ReadEventReward => await Mediator.Send(new ReadEventRewardQuery(request.CardId)),
                    CardRequestType.ReadNavigator => await Mediator.Send(new ReadNavigatorQuery(request.CardId)),
                    CardRequestType.ReadMusicExtra => await Mediator.Send(new ReadMusicExtraQuery(request.CardId)),
                    CardRequestType.ReadMusicAou => await Mediator.Send(new ReadMusicAouQuery(request.CardId)),
                    CardRequestType.ReadCoin => await Mediator.Send(new ReadCoinQuery(request.CardId)),
                    CardRequestType.ReadUnlockReward => await Mediator.Send(new ReadUnlockRewardQuery(request.CardId)),
                    CardRequestType.ReadUnlockKeynum => await Mediator.Send(new ReadUnlockKeynumQuery(request.CardId)),
                    CardRequestType.ReadSoundEffect => await Mediator.Send(new ReadSoundEffectQuery(request.CardId)),
                    CardRequestType.ReadGetMessage => await Mediator.Send(new ReadGetMessageQuery(request.CardId)),
                    CardRequestType.ReadCond => await Mediator.Send(new ReadCondQuery(request.CardId)),
                    CardRequestType.ReadTotalTrophy => await Mediator.Send(new ReadTotalTrophyQuery(request.CardId)),
                    CardRequestType.GetSession or CardRequestType.StartSession => await Mediator.Send(
                        new GetSessionCommand(request.CardId, request.Mac)),
                    CardRequestType.WriteCard =>
                        await Mediator.Send(new WriteCardCommand(request.CardId, request.Data)),
                    CardRequestType.WriteCardDetail => await Mediator.Send(
                        new WriteCardDetailCommand(request.CardId, request.Data)),
                    CardRequestType.WriteCardBData => await Mediator.Send(
                        new WriteCardBDataCommand(request.CardId, request.Data)),
                    CardRequestType.WriteAvatar => await Mediator.Send(new WriteAvatarCommand(request.CardId,
                        request.Data)),
                    CardRequestType.WriteItem =>
                        await Mediator.Send(new WriteItemCommand(request.CardId, request.Data)),
                    CardRequestType.WriteTitle => await Mediator.Send(new WriteTitleCommand(request.CardId,
                        request.Data)),
                    CardRequestType.WriteMusicDetail => await Mediator.Send(
                        new WriteMusicDetailCommand(request.CardId, request.Data)),
                    CardRequestType.WriteNavigator => await Mediator.Send(
                        new WriteNavigatorCommand(request.CardId, request.Data)),
                    CardRequestType.WriteCoin =>
                        await Mediator.Send(new WriteCoinCommand(request.CardId, request.Data)),
                    CardRequestType.WriteSkin =>
                        await Mediator.Send(new WriteSkinCommand(request.CardId, request.Data)),
                    CardRequestType.WriteUnlockKeynum => await Mediator.Send(
                        new WriteUnlockKeynumCommand(request.CardId, request.Data)),
                    CardRequestType.WriteSoundEffect => await Mediator.Send(
                        new WriteSoundEffectCommand(request.CardId, request.Data)),
                    CardRequestType.StartOnlineMatching => await Mediator.Send(
                        new StartOnlineMatchingCommand(request.CardId, request.Data)),
                    CardRequestType.UpdateOnlineMatching => await Mediator.Send(
                        new UpdateOnlineMatchingCommand(request.CardId, request.Data)),
                    CardRequestType.UploadOnlineMatchingResult => await Mediator.Send(
                        new UploadOnlineMatchingResultCommand(request.CardId, request.Data)),
                    _ => throw new ArgumentOutOfRangeException(nameof(cardRequestType), cardRequestType, "Should not happen")
                };
                break;
            }
            case CardCommandType.RegisterRequest:
                result = await Mediator.Send(new CardRegisterCommand(request.CardId, request.Data));
                break;
            case CardCommandType.ReissueRequest:
                result = await Mediator.Send(new CardReissueCommand(request.CardId));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(cardCommandType), cardCommandType, "Should not happen");
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