using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    /**
    * Item for history of user operation
    * 
    */
    public abstract class CadOpe
    {
        //protected uint LayerID;

        protected CadOpe()
        {
            //LayerID = 0;
        }

        /*
        protected CadOpe(uint layerID)
        {
            LayerID = layerID;
        }
        */

        public static CadOpeList getListOpe()
        {
            CadOpeList ope = new CadOpeList();
            return ope;
        }

        public static CadOpeList getListOpe(List<CadOpe> list)
        {
            CadOpeList ope = new CadOpeList(list);
            return ope;
        }

        public static CadOpe getAddPointOpe(uint layerID, uint figureID, int pointIndex, ref CadPoint pt)
        {
            CadPoint t = pt;
            CadOpe ope = new CadOpeAddPoint(layerID, figureID, pointIndex, ref t);
            return ope;
        }

        public static CadOpe getInsertPointsOpe(uint layerID, uint figureID, int startIndex, int insertNum)
        {
            CadOpe ope = new CadOpeInsertPoints(layerID, figureID, startIndex, insertNum);
            return ope;
        }

        public static CadOpe getSetCloseOpe(uint layerID, uint figureID, bool on)
        {
            CadOpe ope = new CadOpeSetClose(layerID, figureID, on);
            return ope;
        }

        public static CadOpe getAddFigureOpe(uint layerID, uint figureID)
        {
            CadOpe ope = new CadOpeAddFigure(layerID, figureID);
            return ope;
        }

        public static CadOpe getRemoveFigureOpe(CadLayer layer, uint figureID)
        {
            CadOpe ope = new CadOpeRemoveFigure(layer, figureID);
            return ope;
        }

        public static CadOpe getDiffOpe(DiffDataList diffList)
        {
            CadOpe ope = new CadOpeDiff(diffList);
            return ope;
        }

        public static CadOpe getRemoveRelPointOpe(CadLayer layer, CadRelativePoint rp)
        {
            CadOpe ope = new CadOpeRemoveRelPoint(layer.ID, rp);
            return ope;
        }

        public virtual string toString()
        {
            return GetType().FullName;
        }

        public abstract void undo(CadObjectDB db);
        public abstract void redo(CadObjectDB db);

        public virtual void releaseResource(CadObjectDB db)
        {
        }
    }

    public class CadOpeDiff : CadOpe
    {
        DiffDataList Diffs;

        public CadOpeDiff(DiffDataList diffs)
        {
            Diffs = diffs;
        }

        public override string toString()
        {
            return GetType().FullName;
        }

        public override void undo(CadObjectDB db)
        {
            Diffs.undo(db);
        }

        public override void redo(CadObjectDB db)
        {
            Diffs.redo(db);
        }
    }


    public class CadOpeList : CadOpe
    {
        public List<CadOpe> OpeList { get; protected set; } = null;

        public CadOpeList()
        {
            OpeList = new List<CadOpe>();
        }

        public CadOpeList(List<CadOpe> list)
        {
            OpeList = new List<CadOpe>(list);
        }

        public override string toString()
        {
            return GetType().FullName;
        }

        public override void undo(CadObjectDB db)
        {
            foreach (CadOpe ope in OpeList.Reverse<CadOpe>())
            {
                ope.undo(db);
            }
        }

        public override void redo(CadObjectDB db)
        {
            foreach (CadOpe ope in OpeList)
            {
                ope.redo(db);
            }
        }
    }

    #region point base
    public abstract class CadOpePointBase : CadOpe
    {
        protected uint LayerID;
        protected uint FigureID;
        protected int PointIndex;

        public CadOpePointBase(
            uint layerID,
            uint figureID,
            int pointIndex)
        {
            LayerID = layerID;
            FigureID = figureID;
            PointIndex = pointIndex;
        }
    }

    public class CadOpeAddPoint : CadOpePointBase
    {
        private CadPoint Point;

        public CadOpeAddPoint(
            uint layerID,
            uint figureID,
            int pointIndex,
            ref CadPoint pt)
            : base(layerID, figureID, pointIndex)
        {
            Point = pt;
        }

        public override string toString()
        {
            return GetType().FullName;
        }

        public override void undo(CadObjectDB db)
        {
            CadFigure fig = db.getFigure(FigureID);
            fig.removePointAt(PointIndex);
        }

        public override void redo(CadObjectDB db)
        {
            CadFigure fig = db.getFigure(FigureID);
            fig.addPoint(Point);
        }
    }


    public class CadOpeInsertPoints : CadOpePointBase
    {
        private int InsertNum;

        private List<CadPoint> mPointList = null;

        public CadOpeInsertPoints(
            uint layerID,
            uint figureID,
            int startIndex,
            int insertNum)
            : base(layerID, figureID, startIndex)
        {
            InsertNum = insertNum;
        }

        public override string toString()
        {
            return GetType().FullName;
        }

        public override void undo(CadObjectDB db)
        {
            CadLayer layer = db.getLayer(LayerID);
            CadFigure fig = db.getFigure(FigureID);

            if (fig == null)
            {
                return;
            }

            int idx = PointIndex;
            int i = 0;

            if (mPointList == null)
            {
                mPointList = new List<CadPoint>();
            }

            mPointList.Clear();

            for (; i < InsertNum; i++)
            {
                mPointList.Add(fig.getPointAt(idx + i));
            }

            fig.PointList.RemoveRange(idx, InsertNum);
        }

        public override void redo(CadObjectDB db)
        {
            CadLayer layer = db.getLayer(LayerID);
            CadFigure fig = db.getFigure(FigureID);
            fig.PointList.InsertRange(PointIndex, mPointList);
        }
    }
    #endregion


    #region Figure base
    public abstract class CadOpeFigureBase : CadOpe
    {
        protected uint LayerID;
        protected uint FigureID;

        public CadOpeFigureBase(
            uint layerID,
            uint figureID
            )
        {
            LayerID = layerID;
            FigureID = figureID;
        }
    }

    public class CadOpeSetClose : CadOpeFigureBase
    {
        bool Close = false;

        public CadOpeSetClose(uint layerID, uint figureID, bool on)
            : base(layerID, figureID)
        {
            Close = on;
        }

        public override string toString()
        {
            return GetType().FullName;
        }

        public override void undo(CadObjectDB db)
        {
            CadFigure fig = db.getFigure(FigureID);
            fig.Closed = !Close;
        }

        public override void redo(CadObjectDB db)
        {
            CadFigure fig = db.getFigure(FigureID);
            fig.Closed = Close;
        }
    }

    public class CadOpeAddFigure : CadOpeFigureBase
    {
        public CadOpeAddFigure(uint layerID, uint figureID)
            : base(layerID, figureID)
        {
        }

        public override string toString()
        {
            return GetType().FullName;
        }

        public override void undo(CadObjectDB db)
        {
            CadLayer layer = db.getLayer(LayerID);
            layer.removeFigureByID(db, FigureID);
        }

        public override void redo(CadObjectDB db)
        {
            CadLayer layer = db.getLayer(LayerID);
            CadFigure fig = db.getFigure(FigureID);
            layer.addFigure(fig);
        }

        public override void releaseResource(CadObjectDB db)
        {
            db.relaseFigure(FigureID);
        }
    }

    public class CadOpeRemoveFigure : CadOpeFigureBase
    {
        int mFigureIndex = 0;

        public CadOpeRemoveFigure(CadLayer layer, uint figureID)
            : base(layer.ID, figureID)
        {
            int figIndex = layer.getFigureIndex(figureID);
            mFigureIndex = figIndex;
        }

        public override string toString()
        {
            return GetType().FullName;
        }

        public override void undo(CadObjectDB db)
        {
            CadLayer layer = db.getLayer(LayerID);
            CadFigure fig = db.getFigure(FigureID);
            layer.insertFigure(mFigureIndex, fig);
        }

        public override void redo(CadObjectDB db)
        {
            CadLayer layer = db.getLayer(LayerID);
            layer.removeFigureByID(db, FigureID);
        }
    }
    #endregion

    public class CadOpeRemoveRelPoint : CadOpe
    {
        protected uint LayerID;
        CadRelativePoint RelPoint;

        public CadOpeRemoveRelPoint(uint layerID, CadRelativePoint relPoint)
        {
            LayerID = layerID;
            RelPoint = relPoint;
        }

        public override void undo(CadObjectDB db)
        {
            CadLayer layer = db.getLayer(LayerID);
            layer.RelPointList.Add(RelPoint);
        }

        public override void redo(CadObjectDB db)
        {
            CadLayer layer = db.getLayer(LayerID);
            layer.RelPointList.RemoveAll( rp => rp.ID == RelPoint.ID);
        }
    }

    public class CadOpeAddChildlen : CadOpe
    {
        private uint ParentID = 0; 
        private List<uint> ChildIDList = new List<uint>();

        public CadOpeAddChildlen(CadFigure parent, List<CadFigure> childlen)
        {
            ParentID = parent.ID;

            childlen.ForEach(a =>
            {
               ChildIDList.Add(a.ID);
            });
        }

        public override void undo(CadObjectDB db)
        {
            CadFigure parent = db.getFigure(ParentID);

            foreach (uint childID in ChildIDList)
            {
                parent.ChildList.RemoveAll( a => a.ID == childID);
                CadFigure fig = db.getFigure(childID);
                fig.Parent = null;
            }
        }

        public override void redo(CadObjectDB db)
        {
            CadFigure parent = db.getFigure(ParentID);

            foreach (uint childID in ChildIDList)
            {
                CadFigure fig = db.getFigure(childID);
                parent.addChild(fig);
            }
        }
    }

    public class CadOpeRemoveChildlen : CadOpe
    {
        private uint ParentID = 0;
        private List<uint> ChildIDList = new List<uint>();

        public CadOpeRemoveChildlen(CadFigure parent, List<CadFigure> childlen)
        {
            ParentID = parent.ID;

            childlen.ForEach(a =>
            {
                ChildIDList.Add(a.ID);
            });
        }

        public override void undo(CadObjectDB db)
        {
            CadFigure parent = db.getFigure(ParentID);

            foreach (uint childID in ChildIDList)
            {
                CadFigure fig = db.getFigure(childID);
                parent.addChild(fig);
            }
        }

        public override void redo(CadObjectDB db)
        {
            CadFigure parent = db.getFigure(ParentID);

            foreach (uint childID in ChildIDList)
            {
                parent.ChildList.RemoveAll(a => a.ID == childID);
                CadFigure fig = db.getFigure(childID);
                fig.Parent = null;
            }
        }
    }
}