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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MediaInfo
{
    public enum StreamKind
    {
        General,
        Visual,
        Audio,
        Text,
        Chapters,
        Image,
        Menu,
        Max
    }

    public enum InfoKind
    {
        Name,
        Text,
        Measure,
        Options,
        NameText,
        MeasureText,
        Info,
        HowTo,
        Max
    }

    public enum InfoOptions
    {
        ShowInInform,
        Reserved,
        ShowInSupported,
        TypeOfValue,
        Max
    }

    class MediaInfo : IDisposable
    {
        [DllImport("MediaInfo.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr MediaInfo_New();

        [DllImport("MediaInfo.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern void MediaInfo_Delete(IntPtr Handle);

        [DllImport("MediaInfo.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern UIntPtr MediaInfo_Open(IntPtr Handle, string FileName);

        [DllImport("MediaInfo.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern void MediaInfo_Close(IntPtr Handle);

        [DllImport("MediaInfo.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr MediaInfo_Inform(IntPtr Handle, UIntPtr Reserved);

        [DllImport("MediaInfo.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr MediaInfo_GetI(IntPtr Handle, UIntPtr StreamKind, UIntPtr StreamNumber, UIntPtr Parameter, UIntPtr KindOfInfo);

        [DllImport("MediaInfo.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)] //See MediaInfoDLL.h for enumeration values
        private static extern IntPtr MediaInfo_Get(IntPtr Handle, UIntPtr StreamKind, UIntPtr StreamNumber, string Parameter, UIntPtr KindOfInfo, UIntPtr KindOfSearch);

        [DllImport("MediaInfo.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr MediaInfo_Option(IntPtr Handle, string Option_, string Value);

        [DllImport("MediaInfo.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern UIntPtr MediaInfo_State_Get(IntPtr Handle);

        [DllImport("MediaInfo.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)] //see MediaInfo.h for details
        private static extern UIntPtr MediaInfo_Count_Get(IntPtr Handle, UIntPtr StreamKind, IntPtr StreamNumber); //see MediaInfoDLL.h for enumeration values

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        IntPtr _handle;
        bool _disposed = false;

        public MediaInfo()
        {
            // Set proper directory for x86 / x64 version
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Environment.Is64BitProcess ? "x64" : "x86");
            SetDllDirectory(path);

            _handle = MediaInfo_New();
        }

        ~MediaInfo()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                //if (disposing) { /* Free other state (managed objects). */ }

                // Free your own state (unmanaged objects). Set large fields to null.
                MediaInfo_Delete(_handle);
                _disposed = true;
            }
        }

        public UIntPtr Open(string fileName)
        {
            return MediaInfo_Open(_handle, fileName);
        }

        public void Close()
        {
            MediaInfo_Close(_handle);
        }

        public string Inform()
        {
            return Marshal.PtrToStringUni(MediaInfo_Inform(_handle, (UIntPtr)0));
        }

        public string Get_(StreamKind streamKind, int streamNumber, int parameter, InfoKind kindOfInfo = InfoKind.Text)
        {
            return Marshal.PtrToStringUni(MediaInfo_GetI(_handle, (UIntPtr)streamKind, (UIntPtr)streamNumber, (UIntPtr)parameter, (UIntPtr)kindOfInfo));
        }

        public string Get_(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo = InfoKind.Text, InfoKind kindOfSearch = InfoKind.Name)
        {
            return Marshal.PtrToStringUni(MediaInfo_Get(_handle, (UIntPtr)streamKind, (UIntPtr)streamNumber, parameter, (UIntPtr)kindOfInfo, (UIntPtr)kindOfSearch));
        }

        public string Option_(string option__, string value = "")
        {
            return Marshal.PtrToStringUni(MediaInfo_Option(_handle, option__, value));
        }
        public int State_Get()
        {
            return Convert.ToInt32(MediaInfo_State_Get(_handle));
        }

        public int Count_Get(StreamKind streamKind, uint streamNumber = uint.MaxValue)
        {
            if (streamNumber == uint.MaxValue)
            {
                return Convert.ToInt32(MediaInfo_Count_Get(_handle, (UIntPtr)streamKind, (IntPtr)(-1)));
            }
            else
            {
                return Convert.ToInt32(MediaInfo_Count_Get(_handle, (UIntPtr)streamKind, (IntPtr)streamNumber));
            }
        }

        #region Static methods

        public const long KB = 1024;
        public const long MB = 1024 * 1024;
        public const long GB = 1024 * 1024 * 1024;
        public const int MIN = 60;
        public const int HR = 60 * 60;

        public static string FormatFileSize(long size)
        {
            var s = string.Empty;

            if (size < 0)
                s = "Invalid size";
            else if (size < KB)
                s = size.ToString() + " Bytes";
            //else if (size < 100 * KB)
            //    s = (size / KB).ToString("#,###") + " KB";
            else if (size < MB)
                s = (size / KB).ToString("#,###") + " KB";
            //else if (size < 100 * MB)
            //    s = (size / MB).ToString("#,###") + " MB";
            else if (size < GB)
                s = (size / MB).ToString("#,###") + " MB";
            //else if (size < 100 * GB)
            //    s = (size / GB).ToString("#,###.#") + " GB";
            else
                s = (size / GB).ToString("#,###") + " GB";

            return s;
        }

        public static long ParseTimeSpan(string timeString)
        {
            long millis = 0;

            if (timeString.Contains(":"))
            {
                var time = new TimeSpan();
                if (TimeSpan.TryParse(timeString, out time))
                {
                    millis = (long)Math.Floor(time.TotalMilliseconds);
                }
            }
            else
            {
                var time = 0d;
                if (double.TryParse(timeString, out time))
                {
                    var k = (long)Math.Floor(time * 1000);
                    millis = (long)Math.Floor(time * 1000);
                }
            }

            return millis;
        }

        public static long GetMillisFromString(string timeString)
        {
            long millis = 0;

            if (timeString.Contains(":"))
            {
                var time = new TimeSpan();
                if (TimeSpan.TryParse(timeString, out time))
                {
                    millis = (long)Math.Floor(time.TotalMilliseconds);
                }
            }
            else
            {
                var time = 0d;
                if (double.TryParse(timeString, out time))
                {
                    var k = (long)Math.Floor(time * 1000);
                    millis = (long)Math.Floor(time * 1000);
                }
            }

            return millis;
        }

        #endregion Static methods
    }
}