﻿using Config.Net;

namespace GCLocalServerRewrite.common;

public interface IAppSettings
{
    [Option(DefaultValue = Configs.DEFAULT_AVATAR_COUNT)]
    int AvatarCount { get; }
    
    [Option(DefaultValue = Configs.DEFAULT_NAVIGATOR_COUNT)]
    int NavigatorCount { get; }
    
    [Option(DefaultValue = Configs.DEFAULT_ITEM_COUNT)]
    int ItemCount { get; }
    
    [Option(DefaultValue = Configs.DEFAULT_TITLE_COUNT)]
    int TitleCount { get; }
    
    [Option(DefaultValue = Configs.DEFAULT_SKIN_COUNT)]
    int SkinCount { get; }
    
    [Option(DefaultValue = Configs.DEFAULT_SE_COUNT)]
    int SeCount { get; }
    
    [Option(DefaultValue = Configs.DEFAULT_MUSIC_DB_NAME)]
    string MusicDbName { get; }
    
    [Option(DefaultValue = Configs.DEFAULT_CARD_DB_NAME)]
    string CardDbName { get; }
    
    [Option(DefaultValue = Configs.DEFAULT_SERVER_IP)]
    string ServerIp { get; }

    IEnumerable<IDataResponse> ResponseData { get; }
}