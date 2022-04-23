using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using GCLocalServerRewrite.common;
using GCLocalServerRewrite.models;
using SharedProject.enums;
using SharedProject.models;
using SQLite.Net2;

namespace GCLocalServerRewrite.controllers;

public class ApiController : WebApiController
{
    private readonly SQLiteConnection cardSqLiteConnection;

    public ApiController()
    {
        cardSqLiteConnection = DatabaseHelper.ConnectDatabase(Configs.CARD_DB_NAME);
    }
    
    [Route(HttpVerbs.Get, "/PlayOption")]
    public PlayOption? GetPlayOption([QueryField] long cardId)
    {
        var result = cardSqLiteConnection.Table<CardDetail>()
            .Where(detail => detail.CardId == cardId &&
                             detail.Pcol1 == 0 && detail.Pcol2 == 0 && detail.Pcol3 == 0);

        if (!result.Any())
        {
            return null;
        }

        var cardDetail = result.First();
        var fastSlow = (int)cardDetail.ScoreUi1;
        var feverTrance = (int)cardDetail.ScoreUi2;

        if (!Enum.IsDefined(typeof(PlayOptions.FastSlowIndicator), fastSlow))
        {
            fastSlow = (int)PlayOptions.FastSlowIndicator.NotUsed;
        }

        if (!Enum.IsDefined(typeof(PlayOptions.FeverTranceShow), feverTrance))
        {
            feverTrance = (int)PlayOptions.FeverTranceShow.NotUsed;
        }

        return new PlayOption
        {
            CardId = cardId,
            FastSlowIndicator = (PlayOptions.FastSlowIndicator)fastSlow,
            FeverTrance = (PlayOptions.FeverTranceShow)feverTrance
        };
    } 
}