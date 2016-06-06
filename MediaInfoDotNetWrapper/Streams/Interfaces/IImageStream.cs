namespace MediaInfo.Streams
{
    public interface IImageStream : IStreamBase
    {
        string FrameSize { get; }

        int Height { get; }

        string PixelFormat { get; }

        long Resolution { get; }

        int Width { get; }
    }
}