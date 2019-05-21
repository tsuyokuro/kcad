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

            get => mStrCursorPos;
        }

        private string mStrCursorPos2 = "";

        public string StrCursorPos2
        {
            set
            {
                mStrCursorPos2 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StrCursorPos2)));
            }

            get => mStrCursorPos2;
        }

        private CadVertex mCursorPos;

        public CadVertex CursorPos
        {
            set
            {
                if (!String.IsNullOrEmpty(mStrCursorPos) && mCursorPos.Equals(value))
                {
                    return;
                }

                mCursorPos = value;

                String s = string.Format("({0:0.00}, {1:0.00}, {2:0.00})",
                    mCursorPos.X, mCursorPos.Y, mCursorPos.Z);

                StrCursorPos = s;
            }

            get => mCursorPos;
        }

        private CadVertex mCursorPos2;

        public CadVertex CursorPos2
        {
            set
            {
                if (!String.IsNullOrEmpty(mStrCursorPos2) && mCursorPos2.Equals(value))
                {
                    return;
                }

                mCursorPos2 = value;

                String s = string.Format("({0:0.00}, {1:0.00}, {2:0.00})",
                    mCursorPos2.X, mCursorPos2.Y, mCursorPos2.Z);

                StrCursorPos2 = s;
            }

            get => mCursorPos2;
        }
    }
}
