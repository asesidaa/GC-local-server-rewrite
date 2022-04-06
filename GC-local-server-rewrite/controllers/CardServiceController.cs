using System.Data;
using System.Net.Mime;
using System.Text;
using System.Xml.Linq;
using ChoETL;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using GCLocalServerRewrite.common;
using GCLocalServerRewrite.models;
using SQLite.Net2;
using Swan.Logging;

namespace GCLocalServerRewrite.controllers;

public class CardServiceController : WebApiController
{
    private readonly SQLiteConnection cardSqLiteConnection;
    private readonly SQLiteConnection musicSqLiteConnection;

    public CardServiceController()
    {
        cardSqLiteConnection = DatabaseHelper.ConnectDatabase(Configs.CARD_DB_NAME);
        musicSqLiteConnection = DatabaseHelper.ConnectDatabase(Configs.MUSIC_DB_NAME);
    }

    [Route(HttpVerbs.Post, "/cardn.cgi")]
    // ReSharper disable once UnusedMember.Global
    public string CardService([FormField] int gid, [FormField("mac_addr")] string mac, [FormField] int type,
        [FormField("card_no")] long cardId, [FormField("data")] string xmlData)
    {
        HttpContext.Response.ContentType = MediaTypeNames.Application.Octet;
        HttpContext.Response.ContentEncoding = Encoding.Default;
        HttpContext.Response.KeepAlive = true;

        /*if (gid != Configs.GC4_EX_GID)
        {
            throw new ArgumentOutOfRangeException(nameof(gid));
        }*/

        if (!Enum.IsDefined(typeof(CardRequestType), type))
        {
            throw new ArgumentOutOfRangeException(nameof(type));
        }

        var requestType = (CardRequestType)type;

        switch (requestType)
        {
            #region ReadRequests

            case CardRequestType.ReadCard:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(Card(cardId));
            }
            case CardRequestType.ReadCardDetail:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(CardDetail(cardId, xmlData));
            }
            case CardRequestType.ReadCardDetails:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(CardDetails(cardId));
            }
            case CardRequestType.ReadCardBData:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(CardBData(cardId));
            }
            case CardRequestType.ReadAvatar:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(
                    GetStaticCount<Avatar>(cardId, Configs.AVATAR_COUNT, Configs.AVATAR_XPATH));
            }
            case CardRequestType.ReadItem:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(
                    GetStaticCount<Item>(cardId, Configs.ITEM_COUNT, Configs.ITEM_XPATH));
            }
            case CardRequestType.ReadSkin:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(
                    GetStaticCount<Skin>(cardId, Configs.SKIN_COUNT, Configs.SKIN_XPATH));
            }
            case CardRequestType.ReadTitle:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(
                    GetStaticCount<Title>(cardId, Configs.TITLE_COUNT, Configs.TITLE_XPATH));
            }
            case CardRequestType.ReadMusic:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(MusicUnlock());
            }
            case CardRequestType.ReadEventReward:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(
                    GenerateEmptyXML(Configs.EVENT_REWARD));
            }
            case CardRequestType.ReadNavigator:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(
                    GetStaticCount<Navigator>(cardId, Configs.NAVIGATOR_COUNT, Configs.NAVIGATOR_XPATH));
            }
            case CardRequestType.ReadMusicExtra:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(MusicExtra());
            }
            case CardRequestType.ReadMusicAou:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(GenerateEmptyXML(Configs.MUSIC_AOU));
            }
            case CardRequestType.ReadCoin:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(Coin(cardId));
            }
            case CardRequestType.ReadUnlockReward:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(UnlockReward(cardId));
            }
            case CardRequestType.ReadUnlockKeynum:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(UnlockKeynum(cardId));
            }
            case CardRequestType.ReadSoundEffect:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(
                    GetStaticCount<SoundEffect>(cardId, Configs.SE_COUNT, Configs.SE_XPATH));
            }
            case CardRequestType.ReadGetMessage:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(GenerateEmptyXML(Configs.GET_MESSAGE));
            }
            case CardRequestType.ReadCond:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(GenerateEmptyXML(Configs.COND));
            }
            case CardRequestType.ReadTotalTrophy:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(TotalTrophy(cardId));
            }
            case CardRequestType.SessionStart:
            case CardRequestType.SessionGet:
            {
                $"Getting read request, type is {requestType}".Info();

                return ConstructResponse(GetSession(cardId, mac));
            }

            #endregion

            #region WriteRequests

            case CardRequestType.WriteCardDetail:
            {
                $"Getting write request, type is {requestType}\n Data is {xmlData}".Info();
                Write<CardDetail>(cardId, xmlData);

                return ConstructResponse(xmlData);
            }
            case CardRequestType.WriteCardBData:
            {
                $"Getting write request, type is {requestType}\n Data is {xmlData}".Info();
                Write<CardBData>(cardId, xmlData);

                return ConstructResponse(xmlData);
            }
            case CardRequestType.WriteCard:
            {
                $"Getting write request, type is {requestType}\n Data is {xmlData}".Info();
                Write<Card>(cardId, xmlData);

                return ConstructResponse(xmlData);
            }
            case CardRequestType.WriteAvatar:
            case CardRequestType.WriteItem:
            case CardRequestType.WriteTitle:
            case CardRequestType.WriteMusicDetail:
            case CardRequestType.WriteNavigator:
            case CardRequestType.WriteCoin:
            case CardRequestType.WriteSkin:
            case CardRequestType.WriteUnlockKeynum:
            case CardRequestType.WriteSoundEffect:
            {
                $"Getting write request, type is {requestType}\n Data is {xmlData}".Info();

                return ConstructResponse(xmlData);
            }

            #endregion

            default:
#pragma warning disable CA2208
                throw new ArgumentOutOfRangeException(nameof(requestType));
#pragma warning restore CA2208
        }
    }

    #region ReadImplementation

    private string Card(long cardId)
    {
        var result = cardSqLiteConnection.Table<Card>().Where(card => card.CardId == cardId);

        if (!result.Any())
        {
            return GenerateEmptyXML(Configs.CARD);
        }

        var card = result.First();

        return GenerateSingleXml(card, Configs.CARD_XPATH);
    }

    private string CardDetail(long cardId, string xmlData)
    {
        var reader = new ChoXmlReader<CardDetailReadData>(new StringReader(xmlData));
        var data = reader.Read();

        var result = cardSqLiteConnection.Table<CardDetail>()
            .Where(detail => detail.CardId == cardId &&
                             detail.Pcol1 == data.Pcol1 && detail.Pcol2 == data.Pcol2 &&
                             detail.Pcol3 == data.Pcol3);

        if (!result.Any())
        {
            return GenerateEmptyXML(Configs.CARD_DETAIL);
        }

        var cardDetail = result.First();

        return GenerateSingleXml(cardDetail, Configs.CARD_DETAIL_SINGLE_XPATH);
    }

    private string CardDetails(long cardId)
    {
        var result = cardSqLiteConnection.Table<CardDetail>()
            .Where(detail => detail.CardId == cardId);

        if (!result.Any())
        {
            return GenerateEmptyXML(Configs.CARD_DETAIL);
        }

        var cardDetails = result.ToList();

        return GenerateRecordsXml(cardDetails, Configs.CARD_DETAIL_RECORD_XPATH);
    }

    private string CardBData(long cardId)
    {
        var result = cardSqLiteConnection.Table<CardBData>()
            .Where(detail => detail.CardId == cardId);

        if (!result.Any())
        {
            return GenerateEmptyXML(Configs.CARD_BDATA);
        }
        var cardBData = result.First();

        return GenerateSingleXml(cardBData, Configs.CARD_BDATA_XPATH);
    }

    private static string GetStaticCount<T>(long cardId, int count, string xpath)
        where T : Record, IIdModel, ICardIdModel, new()
    {
        var models = new List<T>();

        for (var id = 1; id <= count; id++)
        {
            var model = new T();
            model.SetId(id);
            model.SetCardId(cardId);
            models.Add(model);
        }

        return GenerateRecordsXml(models, xpath);
    }

    private static string GetSession(long cardId, string mac)
    {
        var session = new Session
        {
            CardId = cardId,
            Mac = mac,
            SessionId = "12345678901234567890123456789012",
            Expires = 9999,
            PlayerId = 1
        };

        return GenerateSingleXml(session, Configs.SESSION_XPATH);
    }

    private static string TotalTrophy(long cardId)
    {
        var trophy = new TotalTrophy
        {
            CardId = cardId,
            TrophyNum = 9
        };

        return GenerateSingleXml(trophy, Configs.TOTAL_TROPHY_XPATH);
    }

    private static string Coin(long cardId)
    {
        var coin = new Coin
        {
            CardId = cardId,
            CurrentCoins = 999999,
            TotalCoins = 999999,
            MonthlyCoins = 999999
        };

        return GenerateSingleXml(coin, Configs.COIN_XPATH);
    }

    private static string UnlockReward(long cardId)
    {
        var unlockRewards = new List<UnlockReward>
        {
            new()
            {
                CardId = cardId,
                RewardType = 1,
                RewardId = 1,
                TargetId = 1,
                TargetNum = 1,
                KeyNum = 3
            }
        };

        return GenerateRecordsXml(unlockRewards, Configs.UNLOCK_REWARD_XPATH);
    }

    private static string UnlockKeynum(long cardId)
    {
        var unlockKeynums = new List<UnlockKeynum>
        {
            new()
            {
                CardId = cardId,
                RewardId = 1,
                KeyNum = 0,
                RewardCount = 1
            }
        };

        return GenerateRecordsXml(unlockKeynums, Configs.UNLOCK_KEYNUM_XPATH);
    }

    private string MusicUnlock()
    {
        var result = musicSqLiteConnection.Table<Music>().ToList();

        return GenerateRecordsXml(result, Configs.MUSIC_XPATH);
    }

    private string MusicExtra()
    {
        var result = musicSqLiteConnection.Table<MusicExtra>().ToList();

        return GenerateRecordsXml(result, Configs.MUSIC_EXTRA_XPATH);
    }

    #endregion

    #region HelperMethods

    private static string ConstructResponse(string xml)
    {
        return "1\n" +
               "10,10\n" +
               xml;
    }

    private static string GenerateEmptyXML(string fieldName)
    {
        var xml = new XDocument(new XElement("root",
            new XElement(fieldName)));
        xml.Declaration = new XDeclaration("1.0", "UTF-8", null);

        return xml.ToString();
    }

    private static string GenerateSingleXml<T>(T obj, string xpath) where T: class
    {
        var sb = new StringBuilder();

        using (var writer = new ChoXmlWriter<T>(sb))
        {
            writer.Configuration.OmitXmlDeclaration = false;
            writer.Configuration.UseXmlSerialization = true;
            writer.WithXPath(xpath);

            writer.Write(obj);
        }

        return sb.ToString();
    }
    
    private static string GenerateRecordsXml<T>(IReadOnlyList<T> list, string xpath) where T : Record
    {
        var stringBuilder = new StringBuilder();

        for (var i = 0; i < list.Count; i++)
        {
            var obj = list[i];
            obj.RecordId = i + 1;
        }

        using (var writer = new ChoXmlWriter<T>(stringBuilder))
        {
            writer.Configuration.OmitXmlDeclaration = false;
            writer.Configuration.UseXmlSerialization = true;
            writer.WithXPath(xpath);
            writer.Write(list);
        }

        stringBuilder.Insert(stringBuilder.Length - 5, "/");
        return stringBuilder.ToString();
    }

    #endregion

    #region WriteImplementation

    private void Write<T>(long cardId, string xmlData) where T : class, ICardIdModel
    {
        var reader = new ChoXmlReader<T>(new StringReader(xmlData)).WithXPath(Configs.DATA_XPATH);
        var writeObject = reader.Read();

        if (writeObject == null)
        {
            throw new HttpRequestException();
        }

        writeObject.SetCardId(cardId);
        var rowsAffected = cardSqLiteConnection.InsertOrReplace(writeObject, typeof(T));

        if (rowsAffected == 0)
        {
            throw new ApplicationException("Update database failed!");
        }

        $"Updated {typeof(T)}".Info();
    }

    #endregion

    private enum CardRequestType
    {
        // Read data
        ReadCard = 259,
        ReadCardDetail = 260,
        ReadCardDetails = 261,
        ReadCardBData = 264,
        ReadAvatar = 418,
        ReadItem = 420,
        ReadSkin = 422,
        ReadTitle = 424,
        ReadMusic = 428,
        ReadEventReward = 441,
        ReadNavigator = 443,
        ReadMusicExtra = 465,
        ReadMusicAou = 467,
        ReadCoin = 468,
        ReadUnlockReward = 507,
        ReadUnlockKeynum = 509,
        ReadSoundEffect = 8458,
        ReadGetMessage = 8461,
        ReadCond = 8465,
        ReadTotalTrophy = 8468,

        // Sessions
        SessionGet = 401,
        SessionStart = 402,

        // Write data
        WriteCard = 771,
        WriteCardDetail = 772,
        WriteCardBData = 776,
        WriteAvatar = 929,
        WriteItem = 931,
        WriteTitle = 935,
        WriteMusicDetail = 941,
        WriteNavigator = 954,
        WriteCoin = 980,
        WriteSkin = 933,
        WriteUnlockKeynum = 1020,
        WriteSoundEffect = 8969
    }
}