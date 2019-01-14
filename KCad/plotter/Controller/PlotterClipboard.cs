using MessagePack;
using Plotter.Serializer;
using System.Collections.Generic;
using System.Windows;
using CadDataTypes;

namespace Plotter.Controller
{
    public class PlotterClipboard
    {
        public static bool HasCopyData()
        {
            return Clipboard.ContainsData(CadClipBoard.TypeNameBin);
        }

        public static void CopyFiguresAsBin(PlotterController controller)
        {
            var figList = controller.GetSelectedRootFigureList();

            if (figList.Count == 0)
            {
                return;
            }

            List<MpFigure> mpfigList = MpUtil.FigureListToMp(figList, true);

            byte[] bin = MessagePackSerializer.Serialize(mpfigList);

            Clipboard.SetData(CadClipBoard.TypeNameBin, bin);
        }

        public static void PasteFiguresAsBin(PlotterController controller)
        {
            if (!Clipboard.ContainsData(CadClipBoard.TypeNameBin))
            {
                return;
            }
            byte[] bin = (byte[])Clipboard.GetData(CadClipBoard.TypeNameBin);

            List<MpFigure> mpfigList = MessagePackSerializer.Deserialize<List<MpFigure>>(bin);

            List<CadFigure> figList = MpUtil.FigureListFromMp(mpfigList);


            // Pase figures in fig list
            CadVector pp = controller.LastDownPoint;

            MinMax3D mm3d = CadUtil.GetFigureMinMaxIncludeChild(figList);

            CadVector d = pp - mm3d.GetMinAsVector();

            CadOpeList opeRoot = CadOpe.CreateListOpe();

            foreach (CadFigure fig in figList)
            {
                PasteFigure(controller, fig, d);
                controller.CurrentLayer.AddFigure(fig);    // 子ObjectはLayerに追加しない

                CadOpe ope = CadOpe.CreateAddFigureOpe(controller.CurrentLayer.ID, fig.ID);
                opeRoot.OpeList.Add(ope);
            }

            controller.HistoryMan.foward(opeRoot);
        }

        private static void PasteFigure(PlotterController controller, CadFigure fig, CadVector delta)
        {
            fig.MoveAllPoints(controller.CurrentDC, delta);
            controller.DB.AddFigure(fig);

            if (fig.ChildList != null)
            {
                foreach (CadFigure child in fig.ChildList)
                {
                    PasteFigure(controller, child, delta);
                }
            }
        }

        private static void PasteFigure(PlotterController controller, CadFigure fig)
        {
            controller.DB.AddFigure(fig);

            if (fig.ChildList != null)
            {
                foreach (CadFigure child in fig.ChildList)
                {
                    PasteFigure(controller, child);
                }
            }
        }

        public static byte[] FigureListToBin(List<CadFigure> figList)
        {
            List<MpFigure> mpfigList = MpUtil.FigureListToMp(figList, true);
            byte[] bin = MessagePackSerializer.Serialize(mpfigList);

            return bin;
        }

        public static List<CadFigure> FigureListFromBin(byte[] bin)
        {
            List<MpFigure> mpfigList = MessagePackSerializer.Deserialize<List<MpFigure>>(bin);
            List<CadFigure> figList = MpUtil.FigureListFromMp(mpfigList);

            return figList;
        }

        public static List<CadFigure> CopyFigures(List<CadFigure> src)
        {
            byte[] bin = FigureListToBin(src);
            List<CadFigure> dest = FigureListFromBin(bin);
            return dest;
        }

        public static List<CadFigure> CopyFigures(PlotterController controller, List<CadFigure> src)
        {
            byte[] bin = FigureListToBin(src);
            List<CadFigure> dest = FigureListFromBin(bin);

            CadOpeList opeRoot = CadOpe.CreateListOpe();

            foreach (CadFigure fig in dest)
            {
                PasteFigure(controller, fig);
                controller.CurrentLayer.AddFigure(fig);    // 子ObjectはLayerに追加しない

                CadOpe ope = CadOpe.CreateAddFigureOpe(controller.CurrentLayer.ID, fig.ID);
                opeRoot.OpeList.Add(ope);
            }

            controller.HistoryMan.foward(opeRoot);

            return dest;
        }
    }

}