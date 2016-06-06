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
using System.Text.RegularExpressions;

namespace MediaInfo.Streams
{
    abstract class StreamBase : IStreamBase
    {
        protected Regex _exp;
        protected MatchCollection _expMatches;

        #region Properties

        public MediaFile SourceFile { get; set; }

        public int StreamIndex { get; set; }

        public int StreamTypeIndex { get; set; }

        public Dictionary<string, string> Properties { get; set; }

        public virtual string StreamType
        {
            get { return string.Empty; }
        }

        public virtual int Bitrate
        {
            get
            {
                string value;
                if (Properties.TryGetValue("Bit rate", out value))
                {

                    if (value != null)
                    {
                        var i = 0;

                        _exp = new Regex("([ 0-9.,]+)[Kbps]*");
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

        public virtual string Format
        {
            get { return GetProperty("Format"); }
        }

        public virtual string FormatId
        {
            get { return GetProperty("Format"); }
        }

        public virtual string CodecId
        {
            get { return GetProperty("Codec ID"); }
        }

        public long StreamSize
        {
            get
            {
                string value;
                if (Properties.TryGetValue("StreamSize", out value))
                {
                    long i = 0;

                    if (value.Contains(" KiB"))
                        i = (long)(Convert.ToDouble(value.Replace(" KiB", "").Replace(" ", "").Replace(",", "").Trim()) * MediaInfo.KB);
                    else if (value.Contains(" MiB"))
                        i = (long)(Convert.ToDouble(value.Replace(" MiB", "").Replace(" ", "").Replace(",", "").Trim()) * MediaInfo.MB);
                    else if (value.Contains(" GiB"))
                        i = (long)(Convert.ToDouble(value.Replace(" GiB", "").Replace(" ", "").Replace(",", "").Trim()) * MediaInfo.GB);

                    return i;
                }
                else
                    return 0;
            }
        }

        public int Id
        {
            get
            {
                string value;
                int sid;

                if (Properties.TryGetValue("ID", out value))
                {
                    value = value.Trim();
                    if (value.Contains(" "))
                        value = value.Split(' ')[0];
                    if (int.TryParse(value, out sid))
                        return sid;
                }

                return -1;
            }
        }

        public long DurationMillis
        {
            get
            {
                string value;

                if (Properties.TryGetValue("Duration", out value))
                {

                    if (value != null)
                    {
                        if (value.Contains(":"))
                        {
                            //Example: 02:15:30.004
                            return MediaInfo.GetMillisFromString(value);
                        }
                        else
                        {
                            //Example: 2h 15mn 30s 4ms
                            long x = 0;
                            var p = value.Split();

                            foreach (var s in p)
                            {
                                if (s.Contains("ms"))
                                    x += Convert.ToInt64(s.Replace("ms", "").Trim());
                                else if (s.Contains("s"))
                                    x += Convert.ToInt64(s.Replace("s", "").Trim()) * 1000;
                                else if (s.Contains("mn"))
                                    x += Convert.ToInt64(s.Replace("mn", "").Trim()) * MediaInfo.MIN * 1000;
                                else if (s.Contains("h"))
                                    x += Convert.ToInt64(s.Replace("h", "").Trim()) * MediaInfo.HR * 1000;
                            }

                            return x;
                        }
                    }
                }

                return 0;
            }
        }

        public string DurationString
        {
            get
            {
                if (this.DurationMillis > 0)
                    return TimeSpan.FromSeconds(Math.Floor(this.DurationMillis / 1000d)).ToString();
                else
                    return "00:00:00";
            }
        }

        public string DurationStringAccurate
        {
            get
            {
                if (this.DurationMillis > 0)
                    return (TimeSpan.FromSeconds(Math.Floor(this.DurationMillis / 1000d)).ToString() + "." + (this.DurationMillis % 1000).ToString("000"));
                else
                    return "00:00:00.000";
            }
        }

        public virtual string Description
        {
            get { return string.Empty; }
        }

        #endregion Properties

        public StreamBase()
        {
            this.Properties = new Dictionary<string, string>();
        }

        public string GetProperty(string Name)
        {
            if (Properties.ContainsKey(Name))
                return Properties[Name];
            else
                return string.Empty;
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
