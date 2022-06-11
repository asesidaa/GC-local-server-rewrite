namespace GCLocalServerRewrite.common;

public interface IDataResponse
{
    string FileName { get; }
    
    long NotBeforeUnixTime { get; }
    
    long NotAfterUnixTime { get; }
    
    string Md5 { get; }

    int Index { get; }
}