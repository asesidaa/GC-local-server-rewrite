namespace GCLocalServerRewrite.common;

public interface IOptionServiceResponse
{
    string FileName { get; }
    
    long NotBeforeUnixTime { get; }
    
    long NotAfterUnixTime { get; }
    
    string Md5 { get; }

    int Index { get; }
}