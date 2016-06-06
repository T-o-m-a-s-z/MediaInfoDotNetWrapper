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

namespace MediaInfo.Streams
{
    class TextStream : StreamBase, ITextStream
    {
        public override string StreamType
        {
            get { return "Text"; }
        }

        public string Language
        {
            get { return GetProperty("Language"); }
        }

        public string MPlayerID
        {
            get
            {
                string value;
                if (Properties.TryGetValue("MPlayer -sid", out value))
                    return value;
                else
                    return string.Empty;
            }
        }

        public override string FormatId
        {
            get
            {
                var value = GetProperty("Format");

                if (!string.IsNullOrEmpty(value))
                    return value.ToUpperInvariant();

                value = GetProperty("Codec ID");
                if (!string.IsNullOrEmpty(value))
                    return value.ToUpperInvariant();

                return string.Empty;
            }
        }

        public override string Description
        {
            get
            {
                var sb = new StringBuilder();

                if (!string.IsNullOrEmpty(this.FormatId))
                    sb.Append(string.Format(", {0}", this.FormatId));

                if (!string.IsNullOrEmpty(this.Language))
                    sb.Append(string.Format(", {0}", this.Language));

                var description = sb.ToString();

                if (!string.IsNullOrEmpty(description.Trim()))
                    description = description.Trim().Remove(0, 1).Trim();

                return description;
            }
        }

    }
}
