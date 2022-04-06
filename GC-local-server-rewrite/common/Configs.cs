using System.Configuration;
using Swan;
using Swan.Logging;

namespace GCLocalServerRewrite.common;

public static class Configs
{
    public const bool USE_FILE_CACHE = true;

    public const string ROOT_CA_CN = "Taito Arcade Machine CA";

    public const string CERT_CN = "GC local server";

    public const string DB_FOLDER = "db";

    public const string LOG_FOLDER = "log";

    public const string LOG_BASE_NAME = "log";

    public const string STATIC_FOLDER = "static";

    public const string CONFIG_FILE_NAME = "App.config";

    public const string CARD_SERVICE_BASE_ROUTE = "/service/card";

    public const string UPLOAD_SERVICE_BASE_ROUTE = "/service/upload";

    public const string RESPONE_SERVICE_BASE_ROUTE = "/service/respone";

    public const string INCOM_SERVICE_BASE_ROUTE = "/service/incom";

    public const string RANK_BASE_ROUTE = "/ranking";

    public const string ALIVE_BASE_ROUTE = "/alive";

    public const string SERVER_BASE_ROUTE = "/server";

    public const string STATIC_BASE_ROUTE = "/static";

    public const string CARD_DB_NAME = "card.db3";

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
    
    public const int GC4_EX_GID = 303801;
    
    public static readonly int AVATAR_COUNT;

    public static readonly int NAVIGATOR_COUNT;

    public static readonly int ITEM_COUNT;

    public static readonly int TITLE_COUNT;

    public static readonly int SKIN_COUNT;

    public static readonly int SE_COUNT;

    public static readonly string MUSIC_DB_NAME;

    public static readonly string SERVER_ADDR;

    public static readonly List<string> DOMAINS;

private static readonly Configuration CONFIG = ConfigurationManager.OpenExeConfiguration(PathHelper.ConfigFilePath);

    private const string AVATAR_COUNT_CONFIG_NAME = "AvatarCount";
    private const string NAVIGATOR_COUNT_CONFIG_NAME = "NavigatorCount";
    private const string ITEM_COUNT_CONFIG_NAME = "ItemCount";
    private const string TITLE_COUNT_CONFIG_NAME = "TitleCount";
    private const string SKIN_COUNT_CONFIG_NAME = "SkinCount";
    private const string SE_COUNT_CONFIG_NAME = "SeCount";
    private const string MUSIC_DB_CONFIG_NAME = "MusicDBName";
    private const string SERVER_IP_CONFIG_NAME = "ServerIp";

    private const int DEFAULT_AVATAR_COUNT = 294;
    private const int DEFAULT_NAVIGATOR_COUNT = 71;
    private const int DEFAULT_ITEM_COUNT = 21;
    private const int DEFAULT_TITLE_COUNT = 4942;
    private const int DEFAULT_SKIN_COUNT = 21;
    private const int DEFAULT_SE_COUNT = 25;
    private const string DEFAULT_MUSIC_DB_NAME = "music.db3";
    private const string DEFAULT_SERVER_IP = "127.0.0.1";


    static Configs()
    {
        static void GetIntValueFromConfig(string configFieldName, int defaultValue, out int field )
        {
            var success = int.TryParse(CONFIG.AppSettings.Settings[configFieldName].Value, out var count);

            field = success ? count:defaultValue;
        }
        
        GetIntValueFromConfig(AVATAR_COUNT_CONFIG_NAME, DEFAULT_AVATAR_COUNT, out AVATAR_COUNT);
        GetIntValueFromConfig(NAVIGATOR_COUNT_CONFIG_NAME, DEFAULT_NAVIGATOR_COUNT, out NAVIGATOR_COUNT);
        GetIntValueFromConfig(ITEM_COUNT_CONFIG_NAME, DEFAULT_ITEM_COUNT, out ITEM_COUNT);
        GetIntValueFromConfig(TITLE_COUNT_CONFIG_NAME, DEFAULT_TITLE_COUNT, out TITLE_COUNT);
        GetIntValueFromConfig(SKIN_COUNT_CONFIG_NAME, DEFAULT_SKIN_COUNT, out SKIN_COUNT);
        GetIntValueFromConfig(SE_COUNT_CONFIG_NAME, DEFAULT_SE_COUNT, out SE_COUNT);

        MUSIC_DB_NAME = CONFIG.AppSettings.Settings[MUSIC_DB_CONFIG_NAME].Value;

        if (string.IsNullOrWhiteSpace(MUSIC_DB_NAME))
        {
            MUSIC_DB_NAME = DEFAULT_MUSIC_DB_NAME;
        }

        SERVER_ADDR = CONFIG.AppSettings.Settings[SERVER_IP_CONFIG_NAME].Value;

        if (string.IsNullOrWhiteSpace(SERVER_ADDR))
        {
            SERVER_ADDR = DEFAULT_SERVER_IP;
        }

        DOMAINS = new()
        {
            "localhost",
            "cert.nesys.jp",
            "nesys.taito.co.jp",
            "fjm170920zero.nesica.net"
        };
    }
}