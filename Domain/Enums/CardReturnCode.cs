namespace Domain;

public enum CardReturnCode
{
    /// <summary>
    /// Normal
    /// 処理は正常に完了しました in debug string
    /// </summary>
    Ok = 1,

    /// <summary>
    /// New card
    /// 未登録のカードです in debug string
    /// </summary>
    CardNotRegistered = 23,

    /// <summary>
    /// Not reissue, to determine whether it is a new card or reissued card
    /// 再発行予約がありません in debug string
    /// </summary>
    NotReissue = 27,
    
    /// <summary>
    /// Server side validation error
    /// </summary>
    Unknown = 999
}