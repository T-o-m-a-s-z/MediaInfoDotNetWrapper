using System.Collections.Generic;

namespace MediaInfo.Streams
{
    public interface IStreamBase
    {
        int Bitrate { get; }

        string CodecId { get; }

        string Description { get; }

        long DurationMillis { get; }

        string DurationString { get; }

        string DurationStringAccurate { get; }

        string Format { get; }

        string FormatId { get; }

        int Id { get; }

        Dictionary<string, string> Properties { get; set; }

        MediaFile SourceFile { get; set; }

        int StreamIndex { get; set; }

        long StreamSize { get; }

        string StreamType { get; }

        int StreamTypeIndex { get; set; }

        string GetProperty(string Name);

        string ToString();
    }
}