using Newtonsoft.Json.Linq;
using Plotter.Serializer;
using System.ComponentModel;
using System.IO;

namespace Plotter
{
    public partial class PlotterViewModel : INotifyPropertyChanged
    {
        private string mCurrentFileName = null;

        public string CurrentFileName
        {
            get => mCurrentFileName;

            private set
            {
                mCurrentFileName = value;

                if (mCurrentFileName != null)
                {
                    mMainWindow.Title = "KCad " + mCurrentFileName;
                }
                else
                {
                    mMainWindow.Title = "KCad";
                }
            }
        }

        private void SaveFile(string fname)
        {
            if (fname.EndsWith(".kjs") || fname.EndsWith(".txt"))
            {
                SaveToMsgPackJsonFile(fname);
            }
            else
            {
                SaveToMsgPackFile(fname);
            }
        }

        private void LoadFile(string fname)
        {
            if (fname.EndsWith(".kjs") || fname.EndsWith(".txt"))
            {
                LoadFromMsgPackJsonFile(fname);
            }
            else
            {
                LoadFromMsgPackFile(fname);
            }
        }

        #region "MessagePack file access"

        private void SaveToMsgPackFile(string fname)
        {
            CadData cd = new CadData(
                                mController.DB,
                                mController.CurrentDC.WorldScale,
                                mController.PageSize
                                );

            MpCadFile.Save(fname, cd);

            CurrentFileName = fname;
        }

        private void LoadFromMsgPackFile(string fname)
        {
            CadData? cd = MpCadFile.Load(fname);

            if (cd == null)
            {
                return;
            }

            CadData rcd = cd.Value;


            SetWorldScale(rcd.WorldScale);

            mController.PageSize = rcd.PageSize;

            mController.SetDB(rcd.DB);

            CurrentFileName = fname;
        }


        private void SaveToMsgPackJsonFile(string fname)
        {
            CadData cd = new CadData(
                mController.DB,
                mController.CurrentDC.WorldScale,
                mController.PageSize);


            MpCadFile.SaveAsJson(fname, cd);

            CurrentFileName = fname;
        }

        private void LoadFromMsgPackJsonFile(string fname)
        {
            CadData? cd = MpCadFile.LoadJson(fname);

            if (cd == null)
            {
                return;
            }

            CadData rcd = cd.Value;

            SetWorldScale(rcd.WorldScale);

            mController.PageSize = rcd.PageSize;

            mController.SetDB(rcd.DB);

            CurrentFileName = fname;
        }
        #endregion
    }
}