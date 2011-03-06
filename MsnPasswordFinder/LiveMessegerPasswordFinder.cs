using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

/*
Copyright 2009 Oguz Kartal

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/


namespace MsnPasswordFinder
{

    public struct LiveIdInformation
    {
        public string LiveId;
        public string Password;
    }

    class NativeWin32API
    {

        [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]
        public struct CREDENTIAL
        {
            public UInt32                                               Flags;
            public UInt32                                               Type;
            public string                                               TargetName;
            public string                                               Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME     LastWritten;
            public UInt32                                               CredentialBlobSize;
            public IntPtr                                               CredentialBlob;
            public UInt32                                               Persist;
            public UInt32                                               AttributeCount;
            public IntPtr                                               CredAttributes;
            public string                                               TargetAlias;
            public string                                               UserName;
        }
        

        [DllImport("advapi32.dll", EntryPoint = "CredEnumerateW", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CredEnumerate(string Filter, UInt32 Flags, out UInt32 Count, out IntPtr Credentials);


        [DllImport("advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
        public static extern void CredFree(IntPtr Buffer);
        
    }

    class LiveMessegerPasswordFinder
    {
        private LanguageSupport                 LangMgr;
        private List<LiveIdInformation>         LiveAccounts; 
        private IntPtr                          CredentialPtr;
        private int                             ReaderIndex;
        private bool                            IsSuccess;

        public LiveMessegerPasswordFinder()
        {
            CredentialPtr = IntPtr.Zero;
            LiveAccounts = null;
            IsSuccess = false;
            ReaderIndex = 0;
        }

        public bool GetAccountInformations()
        {
            bool Result; 
            UInt32 Count=0;
            LiveIdInformation Info = new LiveIdInformation();
            LangMgr = new LanguageSupport();
            IntPtr BufPtr = IntPtr.Zero; 
            NativeWin32API.CREDENTIAL CurrentCredential;

            if (IsSuccess)
                return true;

            
            Result = NativeWin32API.CredEnumerate("WindowsLive:name=*",0,out Count,out CredentialPtr);

            if (Result)
            {
                LiveAccounts = new List<LiveIdInformation>((int)Count);

                BufPtr = CredentialPtr;

                for (int i = 0; i < Count; i++)
                {
       
                    BufPtr = new IntPtr(BufPtr.ToInt32() + ((i == 0) ? 0  : Marshal.SizeOf(CredentialPtr)));

     
                    CurrentCredential = (NativeWin32API.CREDENTIAL)Marshal.PtrToStructure(Marshal.ReadIntPtr(BufPtr),typeof(NativeWin32API.CREDENTIAL));
 
                    Info.LiveId = CurrentCredential.UserName; 

          
                    Info.Password = Marshal.PtrToStringUni(CurrentCredential.CredentialBlob);

                    int LngIndex = System.Globalization.CultureInfo.CurrentCulture.Parent.NativeName == "English"? 1 : 0;

                    if (Info.Password == null)
                        Info.Password = LangMgr["PwdNotSet"];
                    LiveAccounts.Add(Info); 
                }

                IsSuccess = true; 
            }

            return Result;
        }

        public bool Read(out LiveIdInformation Account)
        {
            Account = new LiveIdInformation();
            
            if (!IsSuccess)
                return false; 

            if (ReaderIndex != LiveAccounts.Count) 
                Account = LiveAccounts[ReaderIndex++]; 
            else
                return false; 

            return true;
        }

        public void ResetReaderIndex()
        {
            ReaderIndex = 0;
        }

        public void Release()
        {
            NativeWin32API.CredFree(CredentialPtr); 
            CredentialPtr = IntPtr.Zero;
            LiveAccounts.Clear();
            LiveAccounts = null;
            IsSuccess = false;
        }

    }
}
