namespace MediaInfo.Streams
{
    public interface IAudioStream : IStreamBase
    {
        int Channels { get; }

        string MPlayerID { get; }

        int SamplingRate { get; }
    }
}