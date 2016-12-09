using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Plotter
{
    using static CadFigure;

    [Serializable]
    public abstract class CadFigureBehavior
    {
        protected CadFigure Fig;

        public CadFigureBehavior(CadFigure fig)
        {
            Fig = fig;
        }

        public abstract States State { get; }
        public abstract void addPoint(CadPoint p);
        public abstract void setPointAt(int index, CadPoint pt);
        public abstract void removeSelected();
        public abstract void draw(DrawContext dc, Pen pen);
        public abstract void drawSeg(DrawContext dc, Pen pen, int idxA, int idxB);
        public abstract void drawSelected(DrawContext dc, Pen pen);
        public abstract void drawTemp(DrawContext dc, CadPoint tp, Pen pen);
        public abstract void startCreate();
        public abstract Types endCreate();

        public virtual void moveSelectedPoint(CadPoint delta)
        {
            for (int i = 0; i < Fig.StoreList.Count; i++)
            {
                CadPoint op = Fig.StoreList[i];

                if (!op.Selected)
                {
                    continue;
                }

                if (i < Fig.PointList.Count)
                {
                    Fig.PointList[i] = op + delta;
                }
            }
        }

        public virtual void moveAllPoints(CadPoint delta)
        {
            CadUtil.movePoints(Fig.PointList, delta);
        }

        public virtual CadRect getContainsRect()
        {
            return CadUtil.getContainsRect(Fig.PointList);
        }

        public virtual IReadOnlyList<CadPoint> getPoints(int curveSplitNum)
        {
            return Fig.PointList;
        }
    }

    #region Nop Behavior
    public class CadNopBehavior : CadFigureBehavior
    {
        CadFigure.Types Type;

        public CadNopBehavior(CadFigure fig, Types t) : base(fig)
        {
            Type = t;
        }

        public override States State
        {
            get
            {
                return States.NONE;
            }
        }

        public override void addPoint(CadPoint p)
        {
        }

        public override void draw(DrawContext dc, Pen pen)
        {
        }

        public override void drawSeg(DrawContext dc, Pen pen, int idxA, int idxB)
        {
        }

        public override void drawSelected(DrawContext dc, Pen pen)
        {
        }

        public override void drawTemp(DrawContext dc, CadPoint tp, Pen pen)
        {
        }

        public override Types endCreate()
        {
            return Type;
        }

        public override void removeSelected()
        {
        }

        public override void setPointAt(int index, CadPoint pt)
        {
        }

        public override void startCreate()
        {
        }
    }
    #endregion
}
