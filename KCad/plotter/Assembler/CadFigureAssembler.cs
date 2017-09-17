using Plotter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    class CadFigureAssembler
    {
        protected CadObjectDB DB;
        protected Result ProcResult = new Result();

        public class ResultItem
        {
            public uint LayerID { set; get; }=0;

            private CadFigure mFigure;

            public CadFigure Figure
            {
                set
                {
                    mFigure = value;
                }

                get
                {
                    return mFigure;
                }
            }

            public uint FigureID
            {
                get
                {
                    if (mFigure == null)
                    {
                        return 0;
                    }

                    return mFigure.ID;
                }
            }

            public ResultItem()
            {
            }

            public ResultItem(uint layerID, CadFigure fig)
            {
                LayerID = layerID;
                Figure = fig;
            }
        }

        public class Result
        {
            public List<ResultItem> AddList = new List<ResultItem>();
            public List<ResultItem> RemoveList = new List<ResultItem>();

            public bool isValid()
            {
                return AddList.Count > 0 || RemoveList.Count > 0;
            }

            public void clear()
            {
                AddList.Clear();
                RemoveList.Clear();
            }
        }

        public CadFigureAssembler(CadObjectDB db)
        {
            DB = db;
        }
    }

    class CadFigureCutter : CadFigureAssembler
    {
        public CadFigureCutter(CadObjectDB db) : base(db)
        {
        }

        public Result Cut(List<SelectItem> selList)
        {
            var sels = (
                from a in selList
                orderby a.FigureID, a.PointIndex ascending
                select a);

            uint figId = 0;
            CadFigure fig = null;
            int pcnt = 0;
            int sp = -1;
            int cp = -1;
            int num = 0;

            List<SelectItem> figSet = new List<SelectItem>();

            Action<CadFigure> endFig = (f) =>
            {
                num = pcnt - sp;

                CadFigure nfig = null;

                if (f.IsLoop)
                {
                    if (num >= 1)
                    {
                        nfig = DB.NewFigure(CadFigure.Types.POLY_LINES);
                        nfig.AddPoints(fig.PointList, sp, num);

                        CadVector t = fig.GetPointAt(0);
                        nfig.AddPoint(t);
                    }
                }
                else
                {
                    if (num >= 2)
                    {
                        nfig = DB.NewFigure(CadFigure.Types.POLY_LINES);
                        nfig.AddPoints(fig.PointList, sp, num);
                    }
                }

                if (nfig != null)
                {
                    ProcResult.AddList.Add(new ResultItem(f.LayerID, nfig));
                }
            };

            foreach (SelectItem si in sels)
            {
                if (si.FigureID != figId)
                {
                    if (sp != -1)
                    {
                        endFig(fig);
                    }

                    figId = si.FigureID;
                    fig = DB.GetFigure(figId);
                    pcnt = fig.PointCount;
                    sp = -1;
                }

                cp = si.PointIndex;

                if (cp == 0)
                {
                    continue;
                }

                if (sp == -1)
                {
                    figSet.Add(si);
                    sp = 0;
                }

                num = cp - sp + 1;

                if (sp + num <= pcnt)
                {
                    CadFigure nfig = DB.NewFigure(CadFigure.Types.POLY_LINES);
                    nfig.AddPoints(fig.PointList, sp, num);

                    ProcResult.AddList.Add(new ResultItem(si.LayerID, nfig));
                }

                sp = cp;
            }

            if (sp != -1)
            {
                endFig(fig);
            }

            foreach (SelectItem si in figSet)
            {
                CadFigure removefig = DB.GetFigure(si.FigureID);

                if (removefig != null)
                {
                    ProcResult.RemoveList.Add(new ResultItem(si.LayerID, removefig));
                }
            }

            return ProcResult;
        }

    }

    class CadSegmentCutter : CadFigureAssembler
    {
        public CadSegmentCutter(CadObjectDB db) : base(db)
        {
        }

        public Result CutSegment(MarkSeg seg, CadVector p)
        {
            ProcResult.clear();

            var ci = CadUtil.PerpendicularCrossSeg(seg.pA, seg.pB, p);

            if (!ci.IsCross)
            {
                return ProcResult;
            }

            CadFigure org = DB.GetFigure(seg.FigureID);

            int a = Math.Min(seg.PtIndexA, seg.PtIndexB);
            int b = Math.Max(seg.PtIndexA, seg.PtIndexB);


            CadFigure fa = DB.NewFigure(CadFigure.Types.POLY_LINES);
            CadFigure fb = DB.NewFigure(CadFigure.Types.POLY_LINES);

            fa.AddPoints(org.PointList, 0, a + 1);
            fa.AddPoint(ci.CrossPoint);

            fb.AddPoint(ci.CrossPoint);
            fb.AddPoints(org.PointList, b);

            if (org.IsLoop)
            {
                fb.AddPoint(fa.GetPointAt(0));
            }

            ProcResult.AddList.Add(new ResultItem(seg.LayerID, fa));
            ProcResult.AddList.Add(new ResultItem(seg.LayerID, fb));
            ProcResult.RemoveList.Add(new ResultItem(org.LayerID, org));

            return ProcResult;
        }
    }


    class AreaCollecter
    {
        public CadObjectDB DB;

        public AreaCollecter(CadObjectDB db)
        {
            DB = db;
        }

        public List<CadFigure> Collect(List<SelectItem> selList)
        {
            List<CadFigure> res = new List<CadFigure>();

            CadFigureBonder bonder = new CadFigureBonder(DB, null);

            var bondRes = bonder.Bond(selList);

            foreach (SelectItem si in selList)
            {
                if (bondRes.RemoveList.Find(a => a.FigureID == si.Figure.ID) != null)
                {
                    continue;
                }

                if (res.Find(a => a.ID == si.Figure.ID) != null)
                {
                    continue;
                }

                if (si.Figure.PointCount < 3)
                {
                    continue;
                }

                res.Add(si.Figure);
            }

            foreach (var item in bondRes.AddList)
            {
                if (item.Figure.PointCount < 3)
                {
                    continue;
                }

                res.Add(item.Figure);
            }

            foreach (CadFigure fig in res)
            {
                if (fig.Type != CadFigure.Types.POLY_LINES)
                {
                    continue;
                }

                if (fig.PointCount < 3)
                {
                    continue;
                }

                fig.Normal = CadMath.Normal(fig.PointList);
            }

            return res;
        }
    }
}
