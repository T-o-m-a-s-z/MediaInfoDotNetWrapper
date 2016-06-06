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

namespace MediaInfo
{
    public abstract class FileBase
    {
        public string File { get; private set; }

        public string Name { get; private set; }

        public string ParentFolder { get; private set; }

        public string Title { get; private set; }

        public string Extension { get; private set; }

        public FileBase(string sourceFile)
        {
            if (string.IsNullOrEmpty(sourceFile))
                return;

            this.File = sourceFile;
            this.Name = Path.GetFileName(sourceFile);
            this.Title = Path.GetFileNameWithoutExtension(sourceFile);
            this.Extension = Path.GetExtension(sourceFile).ToLowerInvariant();
            this.ParentFolder = Path.GetDirectoryName(sourceFile);
        }
    }
}