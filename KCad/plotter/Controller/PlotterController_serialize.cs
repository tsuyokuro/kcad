using KCad;
using MessagePack;
using Newtonsoft.Json.Linq;
using Plotter.Serializer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CadDataTypes;

namespace Plotter.Controller
{
    public partial class PlotterController
    {
        public void Copy()
        {
            CopyFiguresAsBin();
        }

        public void Paste()
        {
            PasteFiguresAsBin();
        }



        public void CopyFiguresAsBin()
        {
            var temp = GetSelectedFigureList();

            var figList = CadFigure.Util.GetRootFigList(temp);

            if (figList.Count == 0)
            {
                return;
            }

            List<MpFigure> mpfigList = MpUtil.FigureListToMp(figList, true);

            byte[] bin = MessagePackSerializer.Serialize(mpfigList);

            Clipboard.SetData(CadClipBoard.TypeNameBin, bin);

            // For debug
            //string js = MessagePackSerializer.ToJson(bin);
            //Clipboard.SetText(js);
        }

        public void PasteFiguresAsBin()
        {
            if (!Clipboard.ContainsData(CadClipBoard.TypeNameBin))
            {
                return;
            }
            byte[] bin = (byte[])Clipboard.GetData(CadClipBoard.TypeNameBin);

            List<MpFigure> mpfigList = MessagePackSerializer.Deserialize<List<MpFigure>>(bin);

            List<CadFigure> figList = MpUtil.FigureListFromMp(mpfigList);


            // Pase figures in fig list
            CadVector pp = LastDownPoint;

            MinMax3D mm3d = CadUtil.GetFigureMinMaxIncludeChild(figList);

            CadVector d = pp - mm3d.GetMinAsVector();

            CadOpeList opeRoot = CadOpe.CreateListOpe();

            foreach (CadFigure fig in figList)
            {
                PasteFigure(fig, d);
                CurrentLayer.AddFigure(fig);    // 子ObjectはLayerに追加しない

                CadOpe ope = CadOpe.CreateAddFigureOpe(CurrentLayer.ID, fig.ID);
                opeRoot.OpeList.Add(ope);
            }

            HistoryMan.foward(opeRoot);

            UpdateTreeView(true);
        }

        private void PasteFigure(CadFigure fig, CadVector delta)
        {
            fig.MoveAllPoints(CurrentDC, delta);
            mDB.AddFigure(fig);

            if (fig.ChildList != null)
            {
                foreach (CadFigure child in fig.ChildList)
                {
                    PasteFigure(child, delta);
                }
            }
        }
    }

}