using System.Collections.Concurrent;
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
using Swan;
using Swan.Logging;

namespace GCLocalServerRewrite.controllers;

public class CardServiceController : WebApiController
{
    private readonly SQLiteConnection cardSqLiteConnection;
    private readonly SQLiteConnection musicSqLiteConnection;

    private static readonly ConcurrentDictionary<long, OnlineMatchingEntry> OnlineMatchingEntries = new();

    public CardServiceController()
    {
        cardSqLiteConnection = DatabaseHelper.ConnectDatabase(Configs.SETTINGS.CardDbName);
        musicSqLiteConnection = DatabaseHelper.ConnectDatabase(Configs.SETTINGS.MusicDbName);
    }

    [Route(HttpVerbs.Post, "/cardn.cgi")]
    // ReSharper disable once UnusedMember.Global
    public string CardService([FormField] int gid, [FormField("mac_addr")] string mac, [FormField] int type,
        [FormField("card_no")] long cardId, [FormField("data")] string xmlData, [FormField("cmd_str")] int cmdType)
    {
        HttpContext.Response.ContentType = MediaTypeNames.Application.Octet;
        HttpContext.Response.ContentEncoding = new UTF8Encoding(false);
        HttpContext.Response.KeepAlive = true;

        return ProcessCommand(cmdType, mac, cardId, xmlData, type);
    }

    private string ProcessCommand(int cmdType, string mac, long cardId, string xmlData, int type)
    {
        if (!Enum.IsDefined(typeof(Command), cmdType))
        {
            throw new ArgumentOutOfRangeException(nameof(cmdType), cmdType, $"Cmd type is unknown!\n Data is {xmlData}");
        }

        var command = (Command)cmdType;

        return command switch
        {
            Command.CardReadRequest or Command.CardWriteRequest => ProcessCardRequest(mac, cardId, xmlData, type),
            Command.ReissueRequest => ProcessReissueRequest(),
            Command.RegisterRequest => ProcessRegisterRequest(cardId, xmlData),
            _ => throw new ArgumentOutOfRangeException(nameof(command), command, "Command unknown, should never happen!")
        };
    }

    private string ProcessRegisterRequest(long cardId, string xmlData)
    {
        $"Get card register request, data is \n{xmlData}".Info();
        Write<Card>(cardId, xmlData);
        return ConstructResponse(xmlData);
    }

    private static string ProcessReissueRequest()
    {
        "Get reissue request, returning not reissue".Info();
        return ConstructResponse("", ReturnCode.NotReissue);
    }

    private string ProcessCardRequest(string mac, long cardId, string xmlData, int type)
    {
        if (!Enum.IsDefined(typeof(CardRequestType), type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "Card request type is unknown!");
        }

        var requestType = (CardRequestType)type;

        $"Getting card request, type is {requestType}".Info();

        switch (requestType)
        {
            #region ReadRequests

            case CardRequestType.ReadCard:
            {
                var response = Card(cardId, out var returnCode);

                return ConstructResponse(response, returnCode);
            }
            case CardRequestType.ReadCardDetail:
            {
                var cardDetail = CardDetail(cardId, xmlData);
                return ConstructResponse(cardDetail);
            }
            case CardRequestType.ReadCardDetails:
            {
                return ConstructResponse(CardDetails(cardId));
            }
            case CardRequestType.ReadCardBData:
            {
                return ConstructResponse(CardBData(cardId));
            }
            case CardRequestType.ReadAvatar:
            {
                return ConstructResponse(
                    GetStaticCount<Avatar>(cardId, Configs.SETTINGS.AvatarCount, Configs.AVATAR_XPATH));
            }
            case CardRequestType.ReadItem:
            {
                return ConstructResponse(
                    GetStaticCount<Item>(cardId, Configs.SETTINGS.ItemCount, Configs.ITEM_XPATH));
            }
            case CardRequestType.ReadSkin:
            {
                return ConstructResponse(
                    GetStaticCount<Skin>(cardId, Configs.SETTINGS.SkinCount, Configs.SKIN_XPATH));
            }
            case CardRequestType.ReadTitle:
            {
                return ConstructResponse(
                    GetStaticCount<Title>(cardId, Configs.SETTINGS.TitleCount, Configs.TITLE_XPATH));
            }
            case CardRequestType.ReadMusic:
            {
                return ConstructResponse(MusicUnlock());
            }
            case CardRequestType.ReadEventReward:
            {
                return ConstructResponse(
                    GenerateEmptyXml(Configs.EVENT_REWARD));
            }
            case CardRequestType.ReadNavigator:
            {
                return ConstructResponse(
                    GetStaticCount<Navigator>(cardId, Configs.SETTINGS.NavigatorCount, Configs.NAVIGATOR_XPATH));
            }
            case CardRequestType.ReadMusicExtra:
            {
                return ConstructResponse(MusicExtra());
            }
            case CardRequestType.ReadMusicAou:
            {
                return ConstructResponse(MusicAouUnlock());
            }
            case CardRequestType.ReadCoin:
            {
                return ConstructResponse(Coin(cardId));
            }
            case CardRequestType.ReadUnlockReward:
            {
                return ConstructResponse(UnlockReward(cardId));
            }
            case CardRequestType.ReadUnlockKeynum:
            {
                return ConstructResponse(UnlockKeynum(cardId));
            }
            case CardRequestType.ReadSoundEffect:
            {
                return ConstructResponse(
                    GetStaticCount<SoundEffect>(cardId, Configs.SETTINGS.SeCount, Configs.SE_XPATH));
            }
            case CardRequestType.ReadGetMessage:
            {
                return ConstructResponse(GenerateEmptyXml(Configs.GET_MESSAGE));
            }
            case CardRequestType.ReadCond:
            {
                return ConstructResponse(GenerateEmptyXml(Configs.COND));
            }
            case CardRequestType.ReadTotalTrophy:
            {
                return ConstructResponse(TotalTrophy(cardId));
            }
            case CardRequestType.StartSession:
            case CardRequestType.GetSession:
            {
                return ConstructResponse(GetSession(cardId, mac));
            }

            #endregion

            #region WriteRequests

            case CardRequestType.WriteCard:
            {
                $"Card Write data is {xmlData}".Info();
                Write<Card>(cardId, xmlData);
                return ConstructResponse(xmlData);
            }
            case CardRequestType.WriteCardDetail:
            {
                $"Card Detail Write data is {xmlData}".Info();
                WriteCardDetail(cardId, xmlData);
                return ConstructResponse(xmlData);
            }
            case CardRequestType.WriteCardBData:
            {
                $"Card BData Write data is {xmlData}".Info();
                Write<CardBData>(cardId, xmlData);
                WriteCardPlayCount(cardId);
                return ConstructResponse(xmlData);
            }
            // TODO: Maybe one day implement these
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
                $"Card Write data is {xmlData}".Info();
                return ConstructResponse(xmlData);
            }

            #endregion
            #region OnlineMatching

            case CardRequestType.StartOnlineMatching:
            {
                $"Start Online Matching, data is {xmlData}".Info();
                return ConstructResponse(StartOnlineMatching(cardId, xmlData));
            }
            
            case CardRequestType.UpdateOnlineMatching:
            {
                $"Update Online Matching, data is {xmlData}".Info();
                return ConstructResponse(UpdateOnlineMatching(cardId, xmlData));
            }

            case CardRequestType.UploadOnlineMatchingResult:
            {
                $"Get Online Matching result, data is {xmlData}".Info();
                return ConstructResponse(UploadOnlineMatchingResult(cardId, xmlData));
            }
            #endregion

            default:
                throw new ArgumentOutOfRangeException(nameof(requestType), requestType, "Request type not captured, should never happen!");
        }
    }
 
    #region ReadImplementation

    private string Card(long cardId, out ReturnCode returnCode)
    {
        var result = cardSqLiteConnection.Table<Card>().Where(card => card.CardId == cardId);

        if (!result.Any())
        {
            returnCode = ReturnCode.CardNotRegistered;
            return string.Empty;
        }

        var card = result.First();

        returnCode = ReturnCode.Ok;
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
            return GenerateEmptyXml(Configs.CARD_DETAIL);
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
            return GenerateEmptyXml(Configs.CARD_DETAIL);
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
            return GenerateEmptyXml(Configs.CARD_BDATA);
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

    private string MusicAouUnlock()
    {
        var result = musicSqLiteConnection.Table<MusicAou>().ToList();

        return !result.Any() ? GenerateEmptyXml(Configs.MUSIC_AOU) : GenerateRecordsXml(result, Configs.MUSIC_AOU_XPATH);
    }

    private string MusicExtra()
    {
        var result = musicSqLiteConnection.Table<MusicExtra>().ToList();

        return GenerateRecordsXml(result, Configs.MUSIC_EXTRA_XPATH);
    }

    #endregion

    #region HelperMethods

    private static string ConstructResponse(string xml, ReturnCode returnCode = ReturnCode.Ok)
    {
        var returnCodeInt = (int)returnCode;
        if (returnCodeInt == 1)
        {
            return $"{returnCodeInt}\n" +
                   "1,1\n" +
                   xml;
        }
        return $"{returnCodeInt}";
    }

    private static string GenerateEmptyXml(string fieldName)
    {
        var xml = new XDocument(new XElement("root",
                                             new XElement(fieldName)));
        xml.Declaration = new XDeclaration("1.0", "UTF-8", null);

        return xml.ToString();
    }

    private static string GenerateSingleXml<T>(T obj, string xpath) where T : class
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

    private void WriteCardPlayCount(long cardId)
    {
        var record = cardSqLiteConnection.Table<CardPlayCount>().Where(count => count.CardId == cardId);

        if (!record.Any())
        {
            $"Created new play count data for card {cardId}".Info();
            var playCount = new CardPlayCount
            {
                CardId = cardId,
                PlayCount = 1,
                LastPlayed = DateTime.Now
            };
            cardSqLiteConnection.InsertOrReplace(playCount);
            return;
        }

        var data = record.First();
        var now = DateTime.Now;
        var lastPlayedTime = data.LastPlayed;

        if (now <= lastPlayedTime)
        {
            $"Current time {now} is less than or equal to last played time! Clock skew detected!".Warn();
            data.PlayCount = 0;
            data.LastPlayed = DateTime.Now;
            cardSqLiteConnection.InsertOrReplace(data);
            return;
        }

        DateTime start;
        DateTime end;

        if (now.Hour >= 8)
        {
            start = DateTime.Today.AddHours(8);
            end = start.AddHours(24);
        }
        else
        {
            end = DateTime.Today.AddHours(8);
            start = end.AddHours(-24);
        }

        data.PlayCount = lastPlayedTime.IsBetween(start, end) ? data.PlayCount + 1 : 0;
        cardSqLiteConnection.InsertOrReplace(data);
        $"Updated card play count, current count is {data.PlayCount}".Info();
    }

    private void WriteCardDetail(long cardId, string xmlData)
    {
        var result = cardSqLiteConnection.Table<CardDetail>()
            .Where(detail => detail.CardId == cardId);

        // Unlock all unlockable songs in card details table when write card detail for the first time
        if (!result.Any())
        {
            UnlockSongs(cardId);
        }

        var reader = new ChoXmlReader<CardDetail>(new StringReader(xmlData)).WithXPath(Configs.DATA_XPATH);
        var cardDetail = reader.Read();

        if (cardDetail is null)
        {
            throw new HttpRequestException("Write object is null");
        }

        cardDetail.SetCardId(cardId);
        cardDetail.LastPlayTime = DateTime.Now;
        var rowsAffected = cardSqLiteConnection.InsertOrReplace(cardDetail);
        if (rowsAffected == 0)
        {
            throw new ApplicationException("Update database failed!");
        }

        "Updated card detail".Info();
    }
    private void UnlockSongs(long cardId)
    {
        var unlockableSongIds = Configs.SETTINGS.UnlockableSongIds;

        if (unlockableSongIds is null)
        {
            unlockableSongIds = Configs.DEFAULT_UNLOCKABLE_SONGS;
        }
        var detailList = unlockableSongIds.Select(id => new CardDetail
            {
                CardId = cardId,
                Pcol1 = 10,
                Pcol2 = id,
                Pcol3 = 0,
                ScoreUi2 = 1,
                ScoreUi6 = 1,
                LastPlayTime = DateTime.Now
            })
            .ToList();

        cardSqLiteConnection.InsertOrIgnoreAll(detailList);
    }

    #endregion

    #region OnlineMatchingImplementation

    private static string StartOnlineMatching(long cardId, string xmlData)
    {
        var reader = new ChoXmlReader<OnlineMatchingEntry>(new StringReader(xmlData)).WithXPath(Configs.DATA_XPATH);
        var entry = reader.Read();

        entry.CardId = cardId;
        entry.MatchingId = 0xDEADBEEF;
        entry.EntryNo = OnlineMatchingEntries.Count;
        entry.EntryStart = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        entry.MatchingTimeout = 10;
        entry.MatchingRemainingTime = 3;
        entry.MatchingWaitTime = 3;
        entry.Status = 1;
        entry.Dump().Info();
        OnlineMatchingEntries[cardId] = entry;
        
        return GenerateRecordsXml(OnlineMatchingEntries.Values.ToList(), Configs.ONLINE_MATCHING_XPATH);
    }

    private static string UpdateOnlineMatching(long cardId, string xmlData)
    {
        var reader = new ChoXmlReader<OnlineMatchingUpdateData>(new StringReader(xmlData));
        var data = reader.Read();
        var entry = OnlineMatchingEntries.GetValueOrDefault(cardId);
        if (entry is null)
        {
            throw new HttpException(400,"Entry for this card id does not exist!");
        }
        if (data.Action != 0)
        {
            $"Action is {data.Action}".Info();
        }

        entry.MessageId = data.MessageId;
        if (entry.MatchingRemainingTime <= 0)
        {
            entry.Status = 3;
            entry.Dump().Info();
            return GenerateRecordsXml(OnlineMatchingEntries.Values.ToList(), Configs.ONLINE_MATCHING_XPATH);
        }
        
        entry.MatchingRemainingTime--;
        entry.Dump().Info();

        return GenerateRecordsXml(OnlineMatchingEntries.Values.ToList(), Configs.ONLINE_MATCHING_XPATH); 
    }

    private static string UploadOnlineMatchingResult(long cardId, string xmlData)
    {
        var reader = new ChoXmlReader<OnlineMatchingResultData>(new StringReader(xmlData));
        var data = reader.Read();
        
        var entry = OnlineMatchingEntries.GetValueOrDefault(cardId);
        if (entry is null)
        {
            throw new HttpException(400,"Entry for this card id does not exist!");
        }

        if (entry.MatchingId != data.MatchingId)
        {
            throw new HttpException(400,"Matching Id mismatch!");
        }

        // Only one matching at a time
        OnlineMatchingEntries.Clear();
        var result = new OnlineMatchingResult
        {
            Status = 1
        };
        return GenerateSingleXml(result, Configs.ONLINE_BATTLE_RESULT_XPATH);
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
        GetSession = 401,
        StartSession = 402,

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
        WriteSoundEffect = 8969,
        
        // Online matching
        StartOnlineMatching = 8705,
        UpdateOnlineMatching = 8961,
        UploadOnlineMatchingResult = 8709
    }

    private enum Command
    {
        CardReadRequest = 256,
        CardWriteRequest = 768,
        RegisterRequest = 512,
        ReissueRequest = 1536
    }

    private enum ReturnCode
    {
        /// <summary>
        /// 処理は正常に完了しました in debug string
        /// </summary>
        Ok = 1,

        /// <summary>
        /// 未登録のカードです in debug string
        /// </summary>
        CardNotRegistered = 23,

        /// <summary>
        /// 再発行予約がありません in debug string
        /// </summary>
        NotReissue = 27
    }
}