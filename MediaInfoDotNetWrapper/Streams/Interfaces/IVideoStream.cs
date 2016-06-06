namespace MediaInfo.Streams
{
    public interface IVideoStream : IStreamBase
    {
        double FrameRate { get; }

        string FrameSize { get; }

        int Height { get; }

        bool IsInterlaced { get; }

        string MPlayerID { get; }

        string PixelFormat { get; }

        long Resolution { get; }

        int Width { get; }
    }
}