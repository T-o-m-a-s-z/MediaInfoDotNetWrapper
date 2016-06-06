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
namespace MediaInfo.Streams
{
    class ChaptersStream : StreamBase, IChaptersStream
    {
        public override string StreamType
        {
            get { return "Chapters"; }
        }

        public override string Description
        {
            get { return this.Format ?? string.Empty; }
        }
    }
}
