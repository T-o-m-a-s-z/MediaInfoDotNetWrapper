/*

Wrapper for MediaInfo library by MediaArea.net SARL (http://mediainfo.sourceforge.net)

Partially based on VB.NET wrapper by Tony George <teejee2008@gmail.com>

This is free software; you can redistribute it and/or modify it under the terms of either:

a) The Lesser General Public License version 2.1 (see license.txt)
b) The BSD License (see BSDL.txt)

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.

*/
using System;
using System.Collections.Generic;
using System.Text;

namespace MediaInfo
{
    public class MediaFile : FileBase
    {
        #region Properties

        public string Description { get; private set; }

        public string InfoText { get; private set; }

        public string InfoHtml { get; private set; }

        public string MediaInfoText { get; private set; }

        public string MediaInfoHtml { get; private set; }

        public bool IsMediaInfoAvailable { get; private set; }
        
        //List containing references for all video, audio, image, text, menu and chapter streams in file
        public List<Streams.IStreamBase> AllStreams { get; private set; }
        
        //Not added to AllStreams
        public Streams.IGeneralStream GeneralStream { get; private set; }

        public List<Streams.IAudioStream> AudioStreams { get; private set; }

        public List<Streams.IVideoStream> VideoStreams { get; private set; }

        public List<Streams.ITextStream> TextStreams { get; private set; }

        public List<Streams.IImageStream> ImageStreams { get; private set; }

        public List<Streams.IMenuStream> MenuStreams { get; private set; }

        public List<Streams.IChaptersStream> ChaptersStreams { get; private set; }

        public List<Streams.IDataStream> DataStreams { get; private set; }

        public int FrameCount
        {
            get
            {
                if (this.VideoStreams.Count > 0)
                {
                    if (this.VideoStreams[0].FrameRate != 0 && this.GeneralStream.DurationMillis != 0)
                        return (int)Math.Ceiling(this.VideoStreams[0].FrameRate * this.GeneralStream.DurationMillis / 1000d);
                }

                return 0;
            }
        }

        public int StreamCount
        {
            get { return this.AudioStreams.Count + this.VideoStreams.Count + this.TextStreams.Count; }
        }

        public bool InfoAvailable
        {
            get { return this.AudioStreams.Count + this.VideoStreams.Count > 0; }
        }

        public long FileSize
        {
            get
            {
                if (System.IO.File.Exists(this.File))
                    return new System.IO.FileInfo(this.File).Length;

                return 0;
            }
        }

        #endregion Properties

        public MediaFile(string sourceFile) : base(sourceFile)
        {
            this.AllStreams = new List<Streams.IStreamBase>();
            this.GeneralStream = new Streams.GeneralStream();
            this.AudioStreams = new List<Streams.IAudioStream>();
            this.VideoStreams = new List<Streams.IVideoStream>();
            this.TextStreams = new List<Streams.ITextStream>();
            this.ImageStreams = new List<Streams.IImageStream>();
            this.MenuStreams = new List<Streams.IMenuStream>();
            this.ChaptersStreams = new List<Streams.IChaptersStream>();
            this.DataStreams = new List<Streams.IDataStream>();

            if (string.IsNullOrEmpty(sourceFile))
                return;

            PrepareMediaInfo(true);

            if (!this.InfoAvailable)
                return;

            this.InfoHtml = GetInfoHtml(this.GeneralStream, this.AllStreams);
            this.InfoText = GetInfoText();
            this.Description = GetDescription();
        }

        private void PrepareMediaInfo(bool appendInfo)
        {
            if (this.IsMediaInfoAvailable)
                return;

            if (System.IO.File.Exists(this.File) == false)
                return;

            using (var objMediaInfo = new MediaInfo())
            {
                objMediaInfo.Open(this.File);

                try
                {
                    this.MediaInfoText = objMediaInfo.Inform().Trim();

                    var generalStream = new Streams.GeneralStream();
                    var allStreams = new List<Streams.IStreamBase>();

                    IEnumerable<string> streams = null;
                    string[] lines = null;

                    streams = this.MediaInfoText.Split(Environment.NewLine + Environment.NewLine);

                    var streamIndex = -1;

                    foreach (var stream in streams)
                    {
                        Streams.StreamBase currentStream = null;
                        lines = stream.Split(Environment.NewLine);
                        var firstLine = lines[0];

                        if (!string.IsNullOrEmpty(firstLine))
                        {
                            if (firstLine.Contains("General"))
                            {
                                currentStream = generalStream = new Streams.GeneralStream();
                                //MediaInfo cannot read Avisynth files, so add format manually
                                if (Extension == ".avs")
                                    currentStream.Properties.Add("Format", "Avisynth Script");
                            }
                            else if (firstLine.Contains("Audio"))
                            {
                                currentStream = new Streams.AudioStream();
                                allStreams.Add(currentStream);
                                currentStream.StreamIndex = ++streamIndex;

                            }
                            else if (firstLine.Contains("Video"))
                            {
                                currentStream = new Streams.VideoStream();
                                allStreams.Add(currentStream);
                                currentStream.StreamIndex = ++streamIndex;

                            }
                            else if (firstLine.Contains("Text"))
                            {
                                currentStream = new Streams.TextStream();
                                allStreams.Add(currentStream);
                                currentStream.StreamIndex = ++streamIndex;
                            }
                            else if (firstLine.Contains("Image"))
                            {
                                currentStream = new Streams.ImageStream();
                                allStreams.Add(currentStream);
                                currentStream.StreamIndex = ++streamIndex;
                            }
                            else if (firstLine.Contains("Menu"))
                            {
                                currentStream = new Streams.MenuStream();
                                allStreams.Add(currentStream);
                                currentStream.StreamIndex = ++streamIndex;
                            }
                            else if (firstLine.Contains("Chapters"))
                            {
                                currentStream = new Streams.ChaptersStream();
                                allStreams.Add(currentStream);
                                currentStream.StreamIndex = ++streamIndex;
                            }
                        }

                        if (currentStream != null)
                        {
                            //get properties
                            foreach (var line in lines)
                            {
                                var property = line.Split(" : ");
                                if (property.Length > 1)
                                {
                                    var propertyName = property[0].Trim();
                                    var propertyValue = property[1].Trim();
                                    if (!currentStream.Properties.ContainsKey(propertyName))
                                        currentStream.Properties.Add(propertyName, propertyValue);
                                }
                            }
                        }
                    }

                    foreach (var stream in allStreams)
                        stream.SourceFile = this;

                    //fix stream order using ID property ------------------------------------

                    var flag = true;
                    Streams.IStreamBase tempStream;

                    //bubble-sort the streams using the ID number

                    for (var pass = 1; (pass <= allStreams.Count) && flag; pass++) // N passes
                    {
                        flag = false;

                        for (var comp = 0; comp <= allStreams.Count - 2; comp++) // N-1 comparisons
                        {
                            if (allStreams[comp].Id != -1 & allStreams[comp + 1].Id != -1)
                            {
                                if (allStreams[comp].Id > allStreams[comp + 1].Id)
                                {
                                    tempStream = allStreams[comp];
                                    allStreams[comp] = allStreams[comp + 1];
                                    allStreams[comp + 1] = tempStream;
                                    flag = true;
                                }
                            }
                        }
                    }

                    foreach (var stream in allStreams)
                        stream.StreamIndex = AllStreams.IndexOf(stream);

                    SetStreamTypeIndices(allStreams);

                    //--finished fixing stream order ---------------------------------------

                    this.IsMediaInfoAvailable = true;
                    this.MediaInfoHtml = GetInfoHtml(generalStream, allStreams);

                    if (appendInfo)
                        AddProperties(generalStream, allStreams, true);
                }
                finally
                {
                    objMediaInfo.Close();
                }
            }
        }

        private void AddProperties(Streams.IGeneralStream generalStream, List<Streams.IStreamBase> allStreams, bool isMediaInfo = false)
        {
            Dictionary<string, string> dict;
            var index = 0;

            if (this.GeneralStream == null)
                this.GeneralStream = generalStream;
            else
            {
                dict = generalStream.Properties;
                foreach (var key in dict.Keys)
                {
                    if (!this.GeneralStream.Properties.ContainsKey(key))
                        this.GeneralStream.Properties.Add(key, dict[key]);
                }
            }

            index = -1;
            //simply add streams one by one
            if (this.AllStreams.Count == 0)
            {
                foreach (var stream in allStreams)
                {
                    index++;

                    this.AllStreams.Add(stream);
                    switch (stream.StreamType.ToString())
                    {
                        case "Data":
                            this.DataStreams.Add(stream as Streams.DataStream);
                            stream.StreamTypeIndex = this.DataStreams.IndexOf(stream as Streams.DataStream);
                            break;
                        case "Audio":
                            this.AudioStreams.Add(stream as Streams.AudioStream);
                            stream.StreamTypeIndex = this.AudioStreams.IndexOf(stream as Streams.AudioStream);
                            break;
                        case "Video":
                            this.VideoStreams.Add(stream as Streams.VideoStream);
                            stream.StreamTypeIndex = this.VideoStreams.IndexOf(stream as Streams.VideoStream);
                            break;
                        case "Text":
                            this.TextStreams.Add(stream as Streams.TextStream);
                            stream.StreamTypeIndex = this.TextStreams.IndexOf(stream as Streams.TextStream);
                            break;
                        case "Image":
                            this.ImageStreams.Add(stream as Streams.ImageStream);
                            stream.StreamTypeIndex = this.ImageStreams.IndexOf(stream as Streams.ImageStream);
                            break;
                        case "Menu":
                            this.MenuStreams.Add(stream as Streams.MenuStream);
                            stream.StreamTypeIndex = this.MenuStreams.IndexOf(stream as Streams.MenuStream);
                            break;
                        case "Chapters":
                            this.ChaptersStreams.Add(stream as Streams.ChaptersStream);
                            stream.StreamTypeIndex = this.ChaptersStreams.IndexOf(stream as Streams.ChaptersStream);
                            break;
                    }
                }
            }
            else
            {
                //match the streams and append additional properties for each stream
                foreach (var s1 in allStreams)
                {
                    foreach (var s2 in this.AllStreams)
                    {
                        if (s2.StreamType.Equals(s1.StreamType) & (s2.StreamTypeIndex == s1.StreamTypeIndex) & ((s2.FormatId == s1.FormatId) | string.IsNullOrEmpty(s2.FormatId)))
                        {
                            dict = s1.Properties;

                            if (isMediaInfo)
                            {
                                //overwrite ffmpeg's CodecID and Format values with MediaInfo's values
                                if (s2.Properties.ContainsKey("Codec ID") & s1.Properties.ContainsKey("Codec ID"))
                                    s2.Properties["Codec ID"] = s1.Properties["Codec ID"];

                                if (s2.Properties.ContainsKey("Format") & s1.Properties.ContainsKey("Format"))
                                    s2.Properties["Format"] = s1.Properties["Format"];
                            }

                            foreach (var key in dict.Keys)
                            {
                                if (!s2.Properties.ContainsKey(key))
                                    s2.Properties.Add(key, dict[key]);
                            }
                        }
                    }
                }
            }
        }

        private void SetStreamTypeIndices(List<Streams.IStreamBase> streamList)
        {
            var audioCount = -1;
            var videoCount = -1;
            var imageCount = -1;
            var textCount = -1;
            var chaptersCount = -1;
            var menuCount = -1;

            foreach (var stream in streamList)
            {
                stream.StreamIndex = streamList.IndexOf(stream);
                if (stream is Streams.VideoStream)
                    stream.StreamTypeIndex = ++videoCount;
                else if (stream is Streams.AudioStream)
                    stream.StreamTypeIndex = ++audioCount;
                else if (stream is Streams.ImageStream)
                    stream.StreamTypeIndex = ++imageCount;
                else if (stream is Streams.TextStream)
                    stream.StreamTypeIndex = ++textCount;
                else if (stream is Streams.ChaptersStream)
                    stream.StreamTypeIndex = ++chaptersCount;
                else if (stream is Streams.MenuStream)
                    stream.StreamTypeIndex = ++menuCount;
            }
        }

        private void DeleteProperties()
        {
            this.GeneralStream = null;
            this.AllStreams.Clear();
            this.AudioStreams.Clear();
            this.VideoStreams.Clear();
            this.TextStreams.Clear();
            this.ImageStreams.Clear();
            this.MenuStreams.Clear();
            this.ChaptersStreams.Clear();
            this.InfoText = string.Empty;
            this.InfoHtml = string.Empty;
        }

        private string GetDescription()
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(this.GeneralStream.FormatId))
                sb.Append(string.Format(", {0} File", this.GeneralStream.Extension));

            if (this.FileSize != 0)
                sb.Append(string.Format(", {0}", MediaInfo.FormatFileSize(this.FileSize)));

            if (this.StreamCount >= 0)
                sb.Append(string.Format(", {0} streams", this.StreamCount));

            if (this.GeneralStream.Bitrate != 0)
                sb.Append(string.Format(", {0} kbps", this.GeneralStream.Bitrate));

            if (!string.IsNullOrEmpty(this.GeneralStream.DurationString))
                sb.Append(string.Format(", {0}", this.GeneralStream.DurationString));

            var description = sb.ToString();
            if (!string.IsNullOrEmpty(description.Trim()))
                description = description.Trim().Remove(0, 1).Trim();

            sb = new StringBuilder(description);
            sb.AppendLine();
            sb.AppendLine();

            foreach (var stream in this.AllStreams)
            {
                if (stream.StreamType == "Audio" || stream.StreamType == "Video" || stream.StreamType == "Text")
                    sb.AppendLine(string.Format("#{0} | {1} [ {2} ]", stream.StreamIndex, stream.StreamType, stream));
            }

            description = sb.ToString().Trim();

            if (description.EndsWith(Environment.NewLine))
                description = description.Remove(description.Length, 1);

            return description;
        }

        private string GetInfoHtml(Streams.IGeneralStream generalStream, List<Streams.IStreamBase> allStreams)
        {
            var sb = new StringBuilder();
            var odd = false;
            var dict = generalStream.Properties;

            sb.AppendLine("<table width='100%'>");
            sb.AppendLine("<thead><tr>");
            sb.AppendLine("<th colspan=2>&nbsp;General</th>");
            sb.AppendLine("</tr></thead>");

            sb.AppendLine("<tbody>");
            foreach (var key in dict.Keys)
            {
                odd = !odd;
                sb.Append(odd ? "<tr class='odd'>" : "<tr>");
                sb.AppendLine(string.Format("<td nowrap>{0}</td><td>{1}</td>", key, dict[key]));
                sb.Append("</tr>");
            }
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine();

            foreach (var stream in allStreams)
            {
                if (stream.Properties.Count == 0)
                    continue;

                dict = stream.Properties;
                sb.AppendLine("<table width='100%'>");

                sb.AppendLine("<thead><tr>");
                sb.AppendLine(string.Format("<th colspan=2>&nbsp;{0} #{1}</th>", stream.StreamType, stream.StreamTypeIndex));
                sb.AppendLine("</tr></thead>");

                sb.AppendLine("<tbody>");
                foreach (var key in dict.Keys)
                {
                    odd = !odd;
                    sb.Append(odd ? "<tr class='odd'>" : "<tr>");
                    sb.AppendLine(string.Format("<td nowrap>{0}</td><td>{1}</td>", key, dict[key]));
                    sb.Append("</tr>");
                }
                sb.AppendLine("</tbody>");
                sb.AppendLine("</table>");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string GetInfoText()
        {
            var sb = new StringBuilder();
            var dict = GeneralStream.Properties;

            sb.AppendLine("General");
            foreach (var key in dict.Keys)
                sb.AppendLine(string.Format("{0} : {1}", key, dict[key]));

            sb.AppendLine();
            foreach (var stream in AllStreams)
            {
                dict = stream.Properties;

                sb.AppendLine(string.Format("{0} #{1}", stream.StreamType, stream.StreamTypeIndex));
                foreach (var key in dict.Keys)
                    sb.AppendLine(string.Format("{0} : {1}", key, dict[key]));

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

    public static class StringExtensions
    {
        public static string[] Split(this string @string, string separator)
        {
            return @string.Split(new string[] { separator }, StringSplitOptions.None);
        }
    }
}