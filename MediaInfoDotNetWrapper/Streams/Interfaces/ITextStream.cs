namespace MediaInfo.Streams
{
    public interface ITextStream : IStreamBase
    {
        string Language { get; }

        string MPlayerID { get; }
    }
}