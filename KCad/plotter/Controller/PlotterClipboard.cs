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

            List<MpFigure_Latest> mpfigList = MpUtil.FigureListToMp_Latest(figList, true);

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

            List<MpFigure_Latest> mpfigList = MessagePackSerializer.Deserialize<List<MpFigure_Latest>>(bin);

            List<CadFigure> figList = MpUtil.FigureListFromMp_Latest(mpfigList);


            // Pase figures in fig list
            CadVertex pp = controller.LastDownPoint;

            MinMax3D mm3d = CadUtil.GetFigureMinMaxIncludeChild(figList);

            CadVertex d = pp - mm3d.GetMinAsVector();

            CadOpeList opeRoot = new CadOpeList();

            foreach (CadFigure fig in figList)
            {
                PasteFigure(controller, fig, d);
                controller.CurrentLayer.AddFigure(fig);    // 子ObjectはLayerに追加しない

                CadOpe ope = new CadOpeAddFigure(controller.CurrentLayer.ID, fig.ID);
                opeRoot.OpeList.Add(ope);
            }

            controller.HistoryMan.foward(opeRoot);
        }

        private static void PasteFigure(PlotterController controller, CadFigure fig, CadVertex delta)
        {
            fig.MoveAllPoints(delta);
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
            List<MpFigure_Latest> mpfigList = MpUtil.FigureListToMp_Latest(figList, true);
            byte[] bin = MessagePackSerializer.Serialize(mpfigList);

            return bin;
        }

        public static List<CadFigure> FigureListFromBin(byte[] bin)
        {
            List<MpFigure_Latest> mpfigList = MessagePackSerializer.Deserialize<List<MpFigure_Latest>>(bin);
            List<CadFigure> figList = MpUtil.FigureListFromMp_Latest(mpfigList);

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

            CadOpeList opeRoot = new CadOpeList();

            foreach (CadFigure fig in dest)
            {
                PasteFigure(controller, fig);
                controller.CurrentLayer.AddFigure(fig);    // 子ObjectはLayerに追加しない

                CadOpe ope = new CadOpeAddFigure(controller.CurrentLayer.ID, fig.ID);
                opeRoot.OpeList.Add(ope);
            }

            controller.HistoryMan.foward(opeRoot);

            return dest;
        }
    }

}