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
using System.Text;
using System.Text.RegularExpressions;

namespace MediaInfo.Streams
{
    class VideoStream : StreamBase, IVideoStream
    {
        public override string StreamType
        {
            get { return "Video"; }
        }

        public override string FormatId
        {
            get
            {
                var value = GetProperty("Format").Trim();

                if (!string.IsNullOrEmpty(value))
                {
                    switch (value.ToLowerInvariant())
                    {
                        case "avc":
                        case "h264":
                            return "H264";
                        case "mpeg-4":
                        case "mpeg-4 visual":
                        case "ms-mpeg v1":
                        case "ms-mpeg v2":
                        case "ms-mpeg v3":
                        case "s-mpeg 4 v2":
                        case "s-mpeg 4 v3":
                            return "MPEG4";
                        case "realvideo 1":
                        case "realvideo 2":
                        case "realvideo 3":
                        case "realvideo 4":
                        case "rv10":
                        case "rv20":
                        case "rv30":
                        case "rv40":
                            return "RV";
                        case "mpeg video":
                        case "mpeg2video":
                            return "MPEG";
                        case "wmv2":
                            return "WMV";
                        default:
                            return value.ToUpperInvariant();
                    }
                }
                else
                {
                    value = GetProperty("Codec ID");
                    switch (value.ToLowerInvariant())
                    {
                        case "rawvideo":
                            return "RAW";
                        case "dvvideo":
                            return "DV";
                        default:
                            return value.ToUpperInvariant();
                    }
                }
            }
        }

        public string MPlayerID
        {
            get
            {
                string value;

                if (Properties.TryGetValue("MPlayer -vid", out value))
                    return value;
                else
                    return "";
            }
        }

        public string PixelFormat
        {
            get { return GetProperty("Pixel format"); }
        }

        public int Width
        {
            get
            {
                string value;

                if (Properties.TryGetValue("Width", out value))
                {

                    if (value != null)
                    {
                        var i = 0;

                        _exp = new Regex("([ 0-9,]+)[pixels]*");
                        _expMatches = _exp.Matches(value);

                        if (_expMatches.Count > 0)
                        {
                            value = _exp.Replace(_expMatches[0].Value, "$1").Replace(" ", "").Replace(",", "").Trim();

                            if (int.TryParse(value, out i))
                                return i;
                        }

                    }
                }

                return 0;
            }
        }

        public int Height
        {
            get
            {
                string value;

                if (Properties.TryGetValue("Height", out value))
                {
                    if (value != null)
                    {
                        var i = 0;

                        _exp = new Regex("([ 0-9,]+)[pixels]*");
                        _expMatches = _exp.Matches(value);

                        if (_expMatches.Count > 0)
                        {
                            value = _exp.Replace(_expMatches[0].Value, "$1").Replace(" ", "").Replace(",", "").Trim();

                            if (int.TryParse(value, out i))
                                return i;
                        }
                    }
                }

                return 0;
            }
        }

        public long Resolution
        {
            get { return this.Width * this.Height; }
        }

        public string FrameSize
        {
            get { return string.Format("{0}x{1}", this.Width, this.Height); }
        }

        public double FrameRate
        {
            get
            {
                string value;

                if (Properties.TryGetValue("Frame rate", out value))
                {

                    if (value != null)
                    {
                        var i = 0d;

                        _exp = new Regex("([ 0-9.,]+)[fps]*");
                        _expMatches = _exp.Matches(value);

                        if (_expMatches.Count > 0)
                        {
                            value = _exp.Replace(_expMatches[0].Value, "$1").Replace(" ", "").Replace(",", "").Trim();

                            if (double.TryParse(value, out i))
                            {
                                if (i >= 24.97 & i <= 25.2)
                                    i = 25;

                                return i;
                            }
                        }
                    }
                }

                return 0;
            }
        }

        public bool IsInterlaced
        {
            get
            {
                string value;

                if (Properties.TryGetValue("Scan type", out value))
                {
                    if (value == "Interlaced")
                        return true;
                }

                return false;
            }
        }

        public override string Description
        {
            get
            {
                var sb = new StringBuilder();

                if (!string.IsNullOrEmpty(this.FormatId))
                    sb.Append(string.Format(", {0}", this.FormatId));

                //if (!string.IsNullOrEmpty(this.PixelFormat))
                //    sb.Append(string.Format(", {0}", this.PixelFormat));

                if (this.Bitrate != 0)
                    sb.Append(string.Format(", {0} kbps", this.Bitrate));

                if (this.FrameRate != 0)
                    sb.Append(string.Format(", {0} fps", this.FrameRate));

                if (!string.IsNullOrEmpty(this.FrameSize))
                    sb.Append(string.Format(", {0}", this.FrameSize));

                var description = sb.ToString();

                if (!string.IsNullOrEmpty(description.Trim()))
                    description = description.Trim().Remove(0, 1).Trim();

                return description;
            }
        }
    }
}
