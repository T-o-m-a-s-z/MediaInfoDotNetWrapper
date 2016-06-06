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
    //Any unknown stream type
    class DataStream : StreamBase, IDataStream
    {

        public override string StreamType
        {
            get { return "Data"; }
        }

        public override string FormatId
        {
            get
            {
                var value = GetProperty("Format");

                if (!string.IsNullOrEmpty(value))
                {
                    switch (value)
                    {
                        case "AVC":
                            return "mpeg4avc";
                        case "MPEG-4 Visual":
                        case "MS-MPEG4 v1":
                        case "MS-MPEG4 v2":
                        case "MS-MPEG4 v3":
                        case "S-Mpeg 4 v2":
                        case "S-Mpeg 4 v3":
                            return "mpeg4asp";
                        case "RealVideo 1":
                        case "RealVideo 2":
                        case "RealVideo 3":
                        case "RealVideo 4":
                            return "rv";
                        case "MPEG Video":
                            return "mpeg";
                        default:
                            return value.ToLowerInvariant();
                    }
                }
                else
                {
                    value = GetProperty("Codec ID");
                    switch (value)
                    {
                        case "rawvideo":
                            return "raw";
                        case "dvvideo":
                            return "dv";
                        default:
                            return value.ToLowerInvariant();
                    }
                }
            }
        }

        public override string Description
        {
            get { return this.FormatId; }
        }
    }
}
