using System;
using System.ComponentModel;
using CadDataTypes;

namespace Plotter
{
    public class CursorPosViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string mStrCursorPos = "";

        public string StrCursorPos
        {
            set
            {
                mStrCursorPos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StrCursorPos)));
            }

            get
            {
                return mStrCursorPos;
            }
        }

        private string mStrCursorPos2 = "";

        public string StrCursorPos2
        {
            set
            {
                mStrCursorPos2 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StrCursorPos2)));
            }

            get
            {
                return mStrCursorPos2;
            }
        }

        private CadVector mCursorPos;

        public CadVector CursorPos
        {
            set
            {
                if (!String.IsNullOrEmpty(mStrCursorPos) && mCursorPos.Equals(value))
                {
                    return;
                }

                mCursorPos = value;

                String s = string.Format("({0:0.00}, {1:0.00}, {2:0.00})",
                    mCursorPos.x, mCursorPos.y, mCursorPos.z);

                StrCursorPos = s;
            }

            get
            {
                return mCursorPos;
            }
        }

        private CadVector mCursorPos2;

        public CadVector CursorPos2
        {
            set
            {
                if (!String.IsNullOrEmpty(mStrCursorPos2) && mCursorPos2.Equals(value))
                {
                    return;
                }

                mCursorPos2 = value;

                String s = string.Format("({0:0.00}, {1:0.00}, {2:0.00})",
                    mCursorPos2.x, mCursorPos2.y, mCursorPos2.z);

                StrCursorPos2 = s;
            }

            get
            {
                return mCursorPos2;
            }
        }
    }
}
