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
using System.IO;
using System.Text.RegularExpressions;

namespace MediaInfo.Streams
{
    class GeneralStream : StreamBase, IGeneralStream
    {
        public override string StreamType
        {
            get { return "General"; }
        }

        public override int Bitrate
        {
            get
            {
                string value;
                if (Properties.TryGetValue("Overall bit rate", out value))
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

        public string Extension
        {
            get
            {
                if (!string.IsNullOrEmpty(GetProperty("Complete name")))
                    return Path.GetExtension(GetProperty("Complete name")).Replace(".", "").Trim().ToUpper();
                else
                    return string.Empty;
            }
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
