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
using System.Text;
using System.Text.RegularExpressions;

namespace MediaInfo.Streams
{
    class AudioStream : StreamBase, IAudioStream
    {
        public override string StreamType
        {
            get { return "Audio"; }
        }

        public override string FormatId
        {
            get
            {
                var value = GetProperty("Format");

                if (!string.IsNullOrEmpty(value))
                {
                    switch (value.ToLowerInvariant())
                    {
                        case "mpeg audio":
                            switch (GetProperty("Format profile").ToLowerInvariant())
                            {
                                case "layer 2":
                                    return "MP2";
                                case "layer 3":
                                    return "MP3";
                            }

                            break;
                        case "2048":
                            return "SONIC";
                        case "ac-3":
                            return "AC3";
                        case "wma1":
                        case "wma2":
                        case "wmav1":
                        case "wmav2":
                            return "WMA";
                        default:
                            //PCM, FLAC, AAC, Vorbis
                            return value.ToUpperInvariant();
                    }
                }
                else
                {
                    value = GetProperty("Codec ID");

                    if (value.Contains("pcm"))
                        return "PCM";
                    else
                        return value.ToUpperInvariant();
                }

                return "";
            }
        }

        public string MPlayerID
        {
            get
            {
                string value;

                if (Properties.TryGetValue("MPlayer -aid", out value))
                    return value;
                else
                    return "";
            }
        }

        public int SamplingRate
        {
            get
            {
                string value;
                if (Properties.TryGetValue("Sampling rate", out value))
                {

                    if (value != null)
                    {
                        double r = 0;

                        _exp = new Regex("([ 0-9.,]+)KHz*");
                        _expMatches = _exp.Matches(value);

                        if (_expMatches.Count > 0)
                        {
                            value = _exp.Replace(_expMatches[0].Value, "$1").Replace(" ", "").Replace(",", "").Trim();

                            if (double.TryParse(value, out r))
                                return Convert.ToInt32(r * 1000);
                        }

                        _exp = new Regex("([ 0-9.,]+)Hz*");
                        _expMatches = _exp.Matches(value);

                        if (_expMatches.Count > 0)
                        {
                            value = _exp.Replace(_expMatches[0].Value, "$1").Replace(" ", "").Replace(",", "").Trim();

                            if (double.TryParse(value, out r))
                                return Convert.ToInt32(r);
                        }
                    }
                }

                return 0;
            }
        }

        public int Channels
        {
            get
            {
                string value;

                if (Properties.TryGetValue("Channel(s)", out value))
                {
                    if (value != null)
                    {
                        var r = 0d;

                        _exp = new Regex("([ 0-9.]+)[channels]*");
                        _expMatches = _exp.Matches(value);

                        if (_expMatches.Count > 0)
                        {
                            value = _exp.Replace(_expMatches[0].Value, "$1").Replace(" ", "").Replace(",", "").Trim();

                            if (double.TryParse(value, out r))
                                return (int)Math.Ceiling(r);
                        }

                    }
                }

                return 0;
            }
        }

        public override string Description
        {
            get
            {
                var sb = new StringBuilder();

                if (!string.IsNullOrEmpty(this.FormatId))
                    sb.Append(string.Format(", {0}", this.FormatId));

                if (this.Bitrate != 0)
                    sb.Append(string.Format(", {0} kbps", this.Bitrate));

                if (this.Channels != 0)
                    sb.Append(string.Format(", {0} ch", this.Channels));

                if (this.SamplingRate != 0)
                    sb.Append(string.Format(", {0} hz", this.SamplingRate));

                var description = sb.ToString();

                if (!string.IsNullOrEmpty(description.Trim()))
                    description = description.Trim().Remove(0, 1).Trim();

                return description;
            }
        }
    }
}
