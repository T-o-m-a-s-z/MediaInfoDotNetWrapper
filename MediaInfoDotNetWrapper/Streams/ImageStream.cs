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

    class ImageStream : StreamBase, IImageStream
    {

        public override string StreamType
        {
            get { return "Image"; }
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

        public override string Description
        {
            get
            {
                var sb = new StringBuilder();

                if (!string.IsNullOrEmpty(this.Format))
                    sb.Append(string.Format(", {0}", this.Format));
                else if (!string.IsNullOrEmpty(CodecId))
                    sb.Append(string.Format(", {0}", this.CodecId));

                if (!string.IsNullOrEmpty(this.PixelFormat))
                    sb.Append(string.Format(", {0}", this.PixelFormat));

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
