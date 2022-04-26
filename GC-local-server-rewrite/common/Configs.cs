using Config.Net;

namespace GCLocalServerRewrite.common;

public static class Configs
{
    public const bool USE_FILE_CACHE = true;

    public const string ROOT_CA_CN = "Taito Arcade Machine CA";

    public const string CERT_CN = "GC local server";

    public const string DB_FOLDER = "db";

    public const string LOG_FOLDER = "log";

    public const string CERT_FOLDER = "certs";

    public const string LOG_BASE_NAME = "log";

    public const string CONFIG_FILE_NAME = "config.json";

    public const string STATIC_FOLDER = "static";

    public const string API_BASE_ROUTE = "/api";
    
    public const string OPTION_SERVICE_BASE_ROUTE = "/service/option";

    public const string CARD_SERVICE_BASE_ROUTE = "/service/card";

    public const string UPLOAD_SERVICE_BASE_ROUTE = "/service/upload";

    public const string RESPONE_SERVICE_BASE_ROUTE = "/service/respone";

    public const string INCOM_SERVICE_BASE_ROUTE = "/service/incom";

    public const string RANK_BASE_ROUTE = "/ranking";

    public const string ALIVE_BASE_ROUTE = "/alive";

    public const string SERVER_BASE_ROUTE = "/server";

    public const string STATIC_BASE_ROUTE = "/static";

    public const string ROOT_XPATH = "/root";

    public const string DATA_XPATH = $"{ROOT_XPATH}/data";

    public const string CARD = "card";

    public const string CARD_XPATH = $"{ROOT_XPATH}/{CARD}";

    public const string CARD_DETAIL = "card_detail";

    public const string CARD_DETAIL_SINGLE_XPATH = $"{ROOT_XPATH}/{CARD_DETAIL}";

    public const string CARD_DETAIL_RECORD_XPATH = $"{CARD_DETAIL_SINGLE_XPATH}/record";

    public const string CARD_BDATA = "card_bdata";

    public const string CARD_BDATA_XPATH = $"{ROOT_XPATH}/{CARD_BDATA}";

    public const string MUSIC = "music";

    public const string MUSIC_XPATH = $"{ROOT_XPATH}/{MUSIC}/record";

    public const string MUSIC_EXTRA = "music_extra";

    public const string MUSIC_EXTRA_XPATH = $"{ROOT_XPATH}/{MUSIC_EXTRA}/record";

    public const string MUSIC_AOU = "music_aou";

    public const string MUSIC_AOU_XPATH = $"{ROOT_XPATH}/{MUSIC_AOU}";

    public const string ITEM = "item";

    public const string ITEM_XPATH = $"{ROOT_XPATH}/{ITEM}/record";

    public const string AVATAR = "avatar";

    public const string AVATAR_XPATH = $"{ROOT_XPATH}/{AVATAR}/record";

    public const string SKIN = "skin";

    public const string SKIN_XPATH = $"{ROOT_XPATH}/{SKIN}/record";

    public const string TITLE = "title";

    public const string TITLE_XPATH = $"{ROOT_XPATH}/{TITLE}/record";

    public const string NAVIGATOR = "navigator";

    public const string NAVIGATOR_XPATH = $"{ROOT_XPATH}/{NAVIGATOR}/record";

    public const string COIN = "coin";

    public const string COIN_XPATH = $"{ROOT_XPATH}/{COIN}";

    public const string UNLOCK_REWARD = "unlock_reward";

    public const string UNLOCK_REWARD_XPATH = $"{ROOT_XPATH}/{UNLOCK_REWARD}/record";

    public const string UNLOCK_KEYNUM = "unlock_keynum";

    public const string UNLOCK_KEYNUM_XPATH = $"{ROOT_XPATH}/{UNLOCK_KEYNUM}/record";

    public const string SOUND_EFFECT = "sound_effect";

    public const string SE_XPATH = $"{ROOT_XPATH}/{SOUND_EFFECT}/record";

    public const string GET_MESSAGE = "get_message";

    public const string TOTAL_TROPHY = "total_trophy";

    public const string TOTAL_TROPHY_XPATH = $"{ROOT_XPATH}/{TOTAL_TROPHY}";

    public const string EVENT_REWARD = "event_reward";

    public const string COND = "cond";

    public const string SESSION_XPATH = $"{ROOT_XPATH}/session";

    public const string RANK_STATUS_XPATH = $"{ROOT_XPATH}/ranking_status";

    public static readonly List<string> DOMAINS = new()
    {
        "localhost",
        "cert.nesys.jp",
        "nesys.taito.co.jp",
        "fjm170920zero.nesica.net"
    };

    public static readonly IAppSettings SETTINGS = 
        new ConfigurationBuilder<IAppSettings>().UseJsonFile(PathHelper.ConfigFilePath).Build();

    public const int DEFAULT_AVATAR_COUNT = 323;
    public const int DEFAULT_NAVIGATOR_COUNT = 94;
    public const int DEFAULT_ITEM_COUNT = 21;
    public const int DEFAULT_TITLE_COUNT = 5273;
    public const int DEFAULT_SKIN_COUNT = 21;
    public const int DEFAULT_SE_COUNT = 26;
    public const string DEFAULT_CARD_DB_NAME = "card.db3";
    public const string DEFAULT_MUSIC_DB_NAME = "music4MAX465.db3";
    public const string DEFAULT_SERVER_IP = "127.0.0.1";
}