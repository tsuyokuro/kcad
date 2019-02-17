using MessagePack;
using Plotter.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using CadDataTypes;

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

        public abstract void Undo(CadObjectDB db);
        public abstract void Redo(CadObjectDB db);

        public virtual void Dispose(CadObjectDB db)
        {
        }
    }


    public class CadOpeFigureSnapShot : CadOpe
    {
        public byte[] Before;
        public byte[] After;

        public uint FigureID = 0;

        public CadOpeFigureSnapShot()
        {

        }

        public void StoreBefore(CadFigure fig)
        {
            MpFigure mpfig = MpFigure.Create(fig);
            Before = LZ4MessagePackSerializer.Serialize(mpfig);

            FigureID = fig.ID;
        }

        public void StoreAfter(CadFigure fig)
        {
            MpFigure mpfig = MpFigure.Create(fig);
            After = LZ4MessagePackSerializer.Serialize(mpfig);
        }

        public override void Undo(CadObjectDB db)
        {
            MpFigure mpfig = LZ4MessagePackSerializer.Deserialize<MpFigure>(Before);

            CadFigure fig = db.GetFigure(mpfig.ID);

            mpfig.RestoreTo(fig);

            SetChildren(fig, mpfig.ChildIdList, db);
        }

        public override void Redo(CadObjectDB db)
        {
            MpFigure mpfig = LZ4MessagePackSerializer.Deserialize<MpFigure>(After);

            CadFigure fig = db.GetFigure(mpfig.ID);

            mpfig.RestoreTo(fig);

            SetChildren(fig, mpfig.ChildIdList, db);
        }

        private void SetChildren(CadFigure fig, List<uint> idList, CadObjectDB db)
        {
            for (int i=0; i<idList.Count; i++)
            {
                fig.AddChild(db.GetFigure(idList[i]));
            }
        }
    }

    public class CadOpeFigureSnapShotList : CadOpe
    {
        public List<CadOpeFigureSnapShot> SnapShotList = new List<CadOpeFigureSnapShot>();

        public CadOpeFigureSnapShotList()
        {

        }

        public void StoreBefore(List<CadFigure> figList)
        {
            for (int i=0; i<figList.Count; i++)
            {
                CadOpeFigureSnapShot ss = new CadOpeFigureSnapShot();

                ss.StoreBefore(figList[i]);

                SnapShotList.Add(ss);
            }
        }

        public void StoreAfter(CadObjectDB db)
        {
            for (int i = 0; i<SnapShotList.Count; i++)
            {
                CadOpeFigureSnapShot ss = SnapShotList[i];
                ss.StoreAfter(db.GetFigure(ss.FigureID));
            }
        }

        public override void Undo(CadObjectDB db)
        {
            for (int i=0; i< SnapShotList.Count; i++)
            {
                SnapShotList[i].Undo(db);
            }
        }

        public override void Redo(CadObjectDB db)
        {
            for (int i = 0; i < SnapShotList.Count; i++)
            {
                SnapShotList[i].Redo(db);
            }
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

        public void Clear()
        {
            OpeList.Clear();
        }

        public int Count()
        {
            return OpeList.Count();
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
        private CadVector Point;

        public CadOpeAddPoint(
            uint layerID,
            uint figureID,
            int pointIndex,
            ref CadVector pt)
            : base(layerID, figureID, pointIndex)
        {
            Point = pt;
        }

        public override void Undo(CadObjectDB db)
        {
            CadFigure fig = db.GetFigure(FigureID);
            fig.RemovePointAt(PointIndex);
        }

        public override void Redo(CadObjectDB db)
        {
            CadFigure fig = db.GetFigure(FigureID);
            fig.AddPoint(Point);
        }
    }


    public class CadOpeInsertPoints : CadOpePointBase
    {
        private int InsertNum;

        private VectorList mPointList = null;

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
            CadLayer layer = db.GetLayer(LayerID);
            CadFigure fig = db.GetFigure(FigureID);

            if (fig == null)
            {
                return;
            }

            int idx = PointIndex;
            int i = 0;

            if (mPointList == null)
            {
                mPointList = new VectorList();
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
            CadLayer layer = db.GetLayer(LayerID);
            CadFigure fig = db.GetFigure(FigureID);
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
            CadFigure fig = db.GetFigure(FigureID);
            fig.IsLoop = !Close;
        }

        public override void Redo(CadObjectDB db)
        {
            CadFigure fig = db.GetFigure(FigureID);
            fig.IsLoop = Close;
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
            CadLayer layer = db.GetLayer(LayerID);
            layer.RemoveFigureByID(db, FigureID);
        }

        public override void Redo(CadObjectDB db)
        {
            CadLayer layer = db.GetLayer(LayerID);
            CadFigure fig = db.GetFigure(FigureID);
            layer.AddFigure(fig);
        }

        public override void Dispose(CadObjectDB db)
        {
            db.RelaseFigure(FigureID);
        }
    }

    public class CadOpeRemoveFigure : CadOpeFigureBase
    {
        int mFigureIndex = 0;

        public CadOpeRemoveFigure(CadLayer layer, uint figureID)
            : base(layer.ID, figureID)
        {
            int figIndex = layer.GetFigureIndex(figureID);
            mFigureIndex = figIndex;
        }

        public override void Undo(CadObjectDB db)
        {
            CadLayer layer = db.GetLayer(LayerID);
            CadFigure fig = db.GetFigure(FigureID);
            layer.InsertFigure(mFigureIndex, fig);
        }

        public override void Redo(CadObjectDB db)
        {
            CadLayer layer = db.GetLayer(LayerID);
            layer.RemoveFigureByID(db, FigureID);
        }
    }
    #endregion

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
            CadFigure parent = db.GetFigure(ParentID);

            foreach (uint childID in ChildIDList)
            {
                parent.ChildList.RemoveAll( a => a.ID == childID);
                CadFigure fig = db.GetFigure(childID);
                fig.Parent = null;
            }
        }

        public override void Redo(CadObjectDB db)
        {
            CadFigure parent = db.GetFigure(ParentID);

            foreach (uint childID in ChildIDList)
            {
                CadFigure fig = db.GetFigure(childID);
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
            CadFigure parent = db.GetFigure(ParentID);

            foreach (uint childID in ChildIDList)
            {
                CadFigure fig = db.GetFigure(childID);
                parent.AddChild(fig);
            }
        }

        public override void Redo(CadObjectDB db)
        {
            CadFigure parent = db.GetFigure(ParentID);

            foreach (uint childID in ChildIDList)
            {
                parent.ChildList.RemoveAll(a => a.ID == childID);
                CadFigure fig = db.GetFigure(childID);
                fig.Parent = null;
            }
        }
    }

    public class CadOpeRemoveChild : CadOpe
    {
        private uint ParentID = 0;
        private uint ChildID;

        public CadOpeRemoveChild(CadFigure parent, CadFigure child)
        {
            ParentID = parent.ID;
            ChildID = child.ID;
        }

        public override void Undo(CadObjectDB db)
        {
            CadFigure parent = db.GetFigure(ParentID);
            CadFigure child = db.GetFigure(ChildID);
            parent.AddChild(child);
        }

        public override void Redo(CadObjectDB db)
        {
            CadFigure parent = db.GetFigure(ParentID);

            parent.ChildList.RemoveAll(a => a.ID == ChildID);
            CadFigure fig = db.GetFigure(ChildID);
            fig.Parent = null;
        }
    }

    public class CadOpeChangeNormal : CadOpe
    {
        private uint FigureID;
        private CadVector NewNormal;
        private CadVector OldNormal;

        public CadOpeChangeNormal(uint figID, CadVector oldNormal, CadVector newNormal)
        {
            FigureID = figID;
            OldNormal = oldNormal;
            NewNormal = newNormal;
        }

        public override void Undo(CadObjectDB db)
        {
            CadFigure fig = db.GetFigure(FigureID);
            fig.Normal = OldNormal;
        }

        public override void Redo(CadObjectDB db)
        {
            CadFigure fig = db.GetFigure(FigureID);
            fig.Normal = NewNormal;
        }
    }

    public class CadOpeInvertDir : CadOpe
    {
        private uint FigureID;

        public CadOpeInvertDir(uint figID)
        {
            FigureID = figID;
        }

        public override void Redo(CadObjectDB db)
        {
            CadFigure fig = db.GetFigure(FigureID);
            fig.InvertDir();
        }

        public override void Undo(CadObjectDB db)
        {
            CadFigure fig = db.GetFigure(FigureID);
            fig.InvertDir();
        }
    }

    public class CadOpeRemoveLayer : CadOpe
    {
        private CadLayer Layer;
        private int Index;

        public CadOpeRemoveLayer(CadLayer layer, int index)
        {
            Layer = layer;
            Index = index;
        }

        public override void Redo(CadObjectDB db)
        {
            db.RemoveLayer(Layer.ID);
        }

        public override void Undo(CadObjectDB db)
        {
            db.InserLayer(Layer, Index);
        }
    }
}