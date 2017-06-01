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
        protected CadOpe()
        {
        }

        public static CadOpeList CreateListOpe()
        {
            CadOpeList ope = new CadOpeList();
            return ope;
        }

        public static CadOpeList CreateListOpe(List<CadOpe> list)
        {
            CadOpeList ope = new CadOpeList(list);
            return ope;
        }

        public static CadOpe CreateAddPointOpe(uint layerID, uint figureID, int pointIndex, ref CadPoint pt)
        {
            CadPoint t = pt;
            CadOpe ope = new CadOpeAddPoint(layerID, figureID, pointIndex, ref t);
            return ope;
        }

        public static CadOpe CreateInsertPointsOpe(uint layerID, uint figureID, int startIndex, int insertNum)
        {
            CadOpe ope = new CadOpeInsertPoints(layerID, figureID, startIndex, insertNum);
            return ope;
        }

        public static CadOpe CreateSetCloseOpe(uint layerID, uint figureID, bool on)
        {
            CadOpe ope = new CadOpeSetClose(layerID, figureID, on);
            return ope;
        }

        public static CadOpe CreateAddFigureOpe(uint layerID, uint figureID)
        {
            CadOpe ope = new CadOpeAddFigure(layerID, figureID);
            return ope;
        }

        public static CadOpe CreateRemoveFigureOpe(CadLayer layer, uint figureID)
        {
            CadOpe ope = new CadOpeRemoveFigure(layer, figureID);
            return ope;
        }

        public static CadOpe CreateDiffOpe(DiffDataList diffList)
        {
            CadOpe ope = new CadOpeDiff(diffList);
            return ope;
        }

        public static CadOpe CreateRemoveRelPointOpe(CadLayer layer, CadRelativePoint rp)
        {
            CadOpe ope = new CadOpeRemoveRelPoint(layer.ID, rp);
            return ope;
        }

        public static CadOpe CreateChangeNormalOpe(uint figID, CadPoint oldNormal, CadPoint newNormal)
        {
            CadOpe ope = new CadOpeChangeNormal(figID, oldNormal, newNormal);
            return ope;
        }

        public abstract void Undo(CadObjectDB db);
        public abstract void Redo(CadObjectDB db);

        public virtual void ReleaseResource(CadObjectDB db)
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

        public override void Undo(CadObjectDB db)
        {
            Diffs.undo(db);
        }

        public override void Redo(CadObjectDB db)
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

        public void Add(CadOpe ope)
        {
            OpeList.Add(ope);
        }

        public override void Undo(CadObjectDB db)
        {
            foreach (CadOpe ope in OpeList.Reverse<CadOpe>())
            {
                ope.Undo(db);
            }
        }

        public override void Redo(CadObjectDB db)
        {
            foreach (CadOpe ope in OpeList)
            {
                ope.Redo(db);
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

        public override void Undo(CadObjectDB db)
        {
            CadFigure fig = db.getFigure(FigureID);
            fig.RemovePointAt(PointIndex);
        }

        public override void Redo(CadObjectDB db)
        {
            CadFigure fig = db.getFigure(FigureID);
            fig.AddPoint(Point);
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

        public override void Undo(CadObjectDB db)
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
                mPointList.Add(fig.GetPointAt(idx + i));
            }

            fig.RemovePointsRange(idx, InsertNum);
        }

        public override void Redo(CadObjectDB db)
        {
            CadLayer layer = db.getLayer(LayerID);
            CadFigure fig = db.getFigure(FigureID);
            fig.InsertPointsRange(PointIndex, mPointList);
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

        public override void Undo(CadObjectDB db)
        {
            CadFigure fig = db.getFigure(FigureID);
            fig.Closed = !Close;
        }

        public override void Redo(CadObjectDB db)
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

        public override void Undo(CadObjectDB db)
        {
            CadLayer layer = db.getLayer(LayerID);
            layer.removeFigureByID(db, FigureID);
        }

        public override void Redo(CadObjectDB db)
        {
            CadLayer layer = db.getLayer(LayerID);
            CadFigure fig = db.getFigure(FigureID);
            layer.addFigure(fig);
        }

        public override void ReleaseResource(CadObjectDB db)
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

        public override void Undo(CadObjectDB db)
        {
            CadLayer layer = db.getLayer(LayerID);
            CadFigure fig = db.getFigure(FigureID);
            layer.insertFigure(mFigureIndex, fig);
        }

        public override void Redo(CadObjectDB db)
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

        public override void Undo(CadObjectDB db)
        {
            CadLayer layer = db.getLayer(LayerID);
            layer.RelPointList.Add(RelPoint);
        }

        public override void Redo(CadObjectDB db)
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

        public override void Undo(CadObjectDB db)
        {
            CadFigure parent = db.getFigure(ParentID);

            foreach (uint childID in ChildIDList)
            {
                parent.ChildList.RemoveAll( a => a.ID == childID);
                CadFigure fig = db.getFigure(childID);
                fig.Parent = null;
            }
        }

        public override void Redo(CadObjectDB db)
        {
            CadFigure parent = db.getFigure(ParentID);

            foreach (uint childID in ChildIDList)
            {
                CadFigure fig = db.getFigure(childID);
                parent.AddChild(fig);
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

        public override void Undo(CadObjectDB db)
        {
            CadFigure parent = db.getFigure(ParentID);

            foreach (uint childID in ChildIDList)
            {
                CadFigure fig = db.getFigure(childID);
                parent.AddChild(fig);
            }
        }

        public override void Redo(CadObjectDB db)
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

    public class CadOpeChangeNormal : CadOpe
    {
        private uint FigureID;
        private CadPoint NewNormal;
        private CadPoint OldNormal;

        public CadOpeChangeNormal(uint figID, CadPoint oldNormal, CadPoint newNormal)
        {
            FigureID = figID;
            OldNormal = oldNormal;
            NewNormal = newNormal;
        }

        public override void Undo(CadObjectDB db)
        {
            CadFigure fig = db.getFigure(FigureID);
            fig.Normal = OldNormal;
        }

        public override void Redo(CadObjectDB db)
        {
            CadFigure fig = db.getFigure(FigureID);
            fig.Normal = NewNormal;
        }
    }
}