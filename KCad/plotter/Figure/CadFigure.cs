//#define LOG_DEBUG

using System;
using System.Collections.Generic;
using System.Drawing;

namespace Plotter
{
    using Newtonsoft.Json.Linq;
    using static CadFigure;

    [Serializable]
    public partial class CadFigure
    {
        #region Enums
        public enum Types : byte
        {
            NONE,
            LINE,
            RECT,
            POLY_LINES,
            CIRCLE,
            POINT,
            GROUP,
            DIMENTION_LINE,
            MAX,
        }

        public enum States : byte
        {
            NONE,
            NOT_ENOUGH,
            ENOUGH,
            WAIT_LAST_POINT,
            CONTINUE,
            FULL,
        }
        #endregion

        #region  "public properties"
        public uint ID { get; set; }

        private Types mType;

        public Types Type {
            get
            {
                return mType;
            }

            set
            {
                mType = value;
                SetBehavior(mType);
            }
        }

        public bool Closed { get; set; }

        public CadVector Normal;

        public List<CadVector> PointList
        {
            get
            {
                return mPointList;
            }
        }

        public int PointCount
        {
            get
            {
                return mPointList.Count;
            }
        }

        public IReadOnlyList<CadVector> StoreList
        {
            get
            {
                return mStoreList;
            }
        }

        public bool Locked  { set; get; } = false;

        public uint LayerID { set; get; } = 0;
        
        public bool Current { set; get; } = false;
        #endregion

        private List<CadVector> mPointList = new List<CadVector>();

        private List<CadVector> mStoreList = null;


        #region Group management
        private CadFigure mParent = null;

        public CadFigure Parent
        {
            set { mParent = value; }
            get { return mParent; }
        }


        private List<CadFigure> mChildList = new List<CadFigure>();

        public List<CadFigure> ChildList
        {
            get
            {
                return mChildList;
            }
        }
        #endregion

        private CadFigureBehavior Behavior = null;

        private static CadFigureBehavior[] BehaviorTbl = null;

        // Static initializer
        static CadFigure()
        {
            BehaviorTbl = new CadFigureBehavior[(int)Types.MAX];
            BehaviorTbl[(int)Types.LINE] = new CadFigureLine();
            BehaviorTbl[(int)Types.RECT] = new CadFigureRect();
            BehaviorTbl[(int)Types.POLY_LINES] = new CadFigurePolyLines();
            BehaviorTbl[(int)Types.CIRCLE] = new CadFigureCircle();
            BehaviorTbl[(int)Types.POINT] = new CadFigurePoint();
            BehaviorTbl[(int)Types.GROUP] = new CadNopBehavior();
            BehaviorTbl[(int)Types.DIMENTION_LINE] = new CadFigureDimLine();
        }

        public CadFigure()
        {
            ID = 0;
            Closed = false;
            Type = Types.NONE;
        }

        public CadFigure(Types type)
        {
            ID = 0;
            Closed = false;
            Type = type;
        }

        private void SetBehavior(Types type)
        {
            if (type > Types.NONE && type < Types.MAX)
            {
                Behavior = BehaviorTbl[(int)type];
            }
        }

        public void ClearPoints()
        {
            mPointList.Clear();
        }

        public void CopyPoints(CadFigure fig)
        {
            if (Locked) return;
            copyPointList(mPointList, fig.mPointList);
        }

        public void AddPoints(IReadOnlyList<CadVector> points, int sp, int num)
        {
            for (int i = 0; i < num; i++)
            {
                CadVector p = points[i + sp];
                AddPoint(p);
            }
        }

        public void AddPoints(IReadOnlyList<CadVector> points, int sp)
        {
            AddPoints(points, sp, points.Count - sp);
        }

        public void AddPoints(IReadOnlyList<CadVector> points)
        {
            foreach (CadVector p in points)
            {
                AddPoint(p);
            }
        }

        public void AddPointsReverse(IReadOnlyList<CadVector> points)
        {
            int cnt = points.Count;
            int i = cnt - 1;

            for (; i >= 0; i--)
            {
                AddPoint(points[i]);
            }
        }

        public void AddPointsReverse(IReadOnlyList<CadVector> points, int sp)
        {
            int cnt = points.Count;
            int i = cnt - 1 - sp;

            for (; i >= 0; i--)
            {
                AddPoint(points[i]);
            }
        }

        public void InsertPointAt(int index, CadVector pt)
        {
            if (index >= mPointList.Count)
            {
                mPointList.Add(pt);
                return;
            }

            mPointList.Insert(index, pt);
        }

        public void RemovePointAt(int index)
        {
            if (mPointList == null)
            {
                return;
            }

            mPointList.RemoveAt(index);
        }

        public void RemovePointsRange(int index, int count)
        {
            mPointList.RemoveRange(index, count);
        }

        public void InsertPointsRange(int index, IEnumerable<CadVector> collection)
        {
            mPointList.InsertRange(index, collection);
        }

        public CadVector GetPointAt(int index)
        {
            return mPointList[index];
        }

        public void SelectPointAt(int index, bool sel)
        {
            CadVector p = mPointList[index];
            p.Selected = sel;
            mPointList[index] = p;
        }

        public void ClearSelectFlags()
        {
            int i;
            for (i = 0; i < mPointList.Count; i++)
            {
                SelectPointAt(i, false);
            }
        }

        public void Select()
        {
            // Set select flag to all points
            int i;
            for (i = 0; i < mPointList.Count; i++)
            {
                SelectPointAt(i, true);
            }
        }

        public void StartEdit()
        {
            if (Locked) return;

            Behavior.StartEdit(this);

            if (mStoreList != null)
            {
                return;
            }

            mStoreList = new List<CadVector>();
            mPointList.ForEach(a => mStoreList.Add(a));
        }

        public DiffData EndEdit()
        {
            if (Locked) return null;

            Behavior.EndEdit(this);

            DiffData diff = DiffData.create(this);
            mStoreList = null;
            return diff;
        }

        public void CancelEdit()
        {
            if (Locked) return;

            Behavior.CancelEdit(this);

            if (mStoreList == null)
            {
                return;
            }

            mPointList.Clear();
            mStoreList.ForEach(a => mPointList.Add(a));
            mStoreList = null;
        }

        public List<CadVector> GetPointListCopy()
        {
            return new List<CadVector>(mPointList);
        }

        public int FindPoint(CadVector t)
        {
            int i = 0;
            foreach (CadVector p in mPointList)
            {
                if (t.CoordEquals(p))
                {
                    return i;
                }

                i++;
            }

            return -1;
        }

        public void ReversePointList()
        {
            if (Locked) return;

            mPointList.Reverse();
        }

        public void AddChild(CadFigure fig)
        {
            if (Locked) return;

            mChildList.Add(fig);
            fig.setParent(this);
        }

        public void ReleaseAllChildlen()
        {
            if (Locked) return;

            foreach (CadFigure fig in mChildList)
            {
                fig.Parent = null;
            }

            mChildList.Clear();
        }

        #region "Group"
        public void SelectWithGroup()
        {
            CadFigure root = getGroupRoot();
            root.Select();
            root.SelectChildren();
        }

        public void SelectChildren()
        {
            foreach (CadFigure fig in mChildList)
            {
                fig.Select();
                fig.SelectChildren();
            }
        }

        public void setParent(CadFigure fig)
        {
            mParent = fig;
        }

        public CadFigure getParent()
        {
            return mParent;
        }

        public CadFigure getGroupRoot()
        {
            CadFigure fig = this;
            CadFigure parent = null;

            while (fig != null)
            {
                parent = fig.getParent();

                if (parent == null)
                {
                    break;
                }

                fig = parent;
            }

            return fig;
        }
        #endregion

        #region "JSON"
        public JObject ToJson()
        {
            JObject jo = new JObject();

            jo.Add("id", ID);
            jo.Add("type", (byte)Type);
            jo.Add("closed", Closed);
            jo.Add("locked", Locked);
            jo.Add("normal", Normal.ToJson());

            jo.Add("point_list", JsonUtil.ListToJsonList(PointList));
            return jo;
        }

        public void FromJson(JObject jo)
        {
            ID = (uint)jo["id"];
            Type = (Types)(byte)jo["type"];
            Closed = (bool)jo["closed"];
            Locked = (bool)jo["locked"];

            Normal.FromJson((JObject)jo["normal"]);

            mPointList = JsonUtil.JsonListToObjectList<CadVector>((JArray)jo["point_list"]);
        }


        public JObject GroupInfoToJson()
        {
            JObject jo = new JObject();

            jo.Add("id", ID);
            jo.Add("parent_id", mParent != null ? mParent.ID : 0);
            jo.Add("child_id_list", JsonUtil.ListToJsonIdList<CadFigure>(mChildList));

            return jo;
        }

        public void GroupInfoFromJson(CadObjectDB db, JObject jo)
        {
            uint joid = (uint)jo["id"];

            if (ID != joid)
            {
                Log.e("CadFigure#GroupInfoFromJson() invalid JObject. ID missmatch");
            }

            uint parentID = (uint)jo["parent_id"];
            List<uint> childList = JsonUtil.JsonIdListToList((JArray)jo["child_id_list"]);

            mParent = db.getFigure(parentID);

            mChildList.Clear();

            foreach (uint id in childList)
            {
                CadFigure fig = db.getFigure(id);
                mChildList.Add(fig);
            }
        }

        #endregion

        #region "Dump" 
        public void SimpleDump(DebugOut dout, string prefix = nameof(CadFigure))
        {
            dout.println(
                prefix +
                "(" + this.GetHashCode().ToString() + ")" +
                "ID=" + ID.ToString());
        }

        public void Dump(DebugOut dout, string prefix = nameof(CadFigure))
        {
            dout.println(this.GetType().Name + "(" + this.GetHashCode().ToString() + ") {");
            dout.Indent++;
            dout.println("ID=" + ID.ToString());
            dout.println("LayerID=" + LayerID.ToString());
            dout.println("Type=" + Type.ToString());
            dout.println("State=" + State.ToString());

            dout.println("PointList [");
            dout.Indent++;
            foreach (CadVector point in PointList)
            {
                point.dump(dout, "");
            }
            dout.Indent--;
            dout.println("]");


            dout.println("ParentID=" + (mParent != null ? mParent.ID : 0));

            dout.println("Child [");
            dout.Indent++;
            foreach (CadFigure fig in mChildList)
            {
                dout.println("" + fig.ID);
            }
            dout.Indent--;
            dout.println("]");

            dout.Indent--;
            dout.println("}");
        }

        #endregion

        #region "Behavior"
        public virtual States State
        {
            get
            {
                States st = Behavior.GetState(this);
                return st;
            }
        }

        public System.Type GetBehaviorType()
        {
            return Behavior.GetType();
        }

        public void MoveSelectedPoints(DrawContext dc, CadVector delta)
        {
            if (Locked) return;
            Log.d("moveSelectedPoints" + 
                " dx=" + delta.x.ToString() +
                " dy=" + delta.y.ToString() +
                " dz=" + delta.z.ToString()
                );
            Behavior.MoveSelectedPoint(this, dc, delta);
        }

        public void MoveAllPoints(CadVector delta)
        {
            if (Locked) return;

            Behavior.MoveAllPoints(this, delta);
        }

        public void AddPoint(CadVector p)
        {
            Behavior.AddPoint(this, p);
        }

        public void AddPointInCreating(DrawContext dc, CadVector p)
        {
            Behavior.AddPointInCreating(this, dc, p);
        }

        public void RemoveSelected()
        {
            if (Locked) return;

            Behavior.RemoveSelected(this);
        }

        public void SetPointAt(int index, CadVector pt)
        {
            Behavior.SetPointAt(this, index, pt);
        }

        public void Draw(DrawContext dc, int pen)
        {
            Behavior.Draw(this, dc, pen);
        }

        public void DrawSeg(DrawContext dc, int pen, int idxA, int idxB)
        {
            Behavior.DrawSeg(this, dc, pen, idxA, idxB);
        }

        public void DrawSelected(DrawContext dc, int pen)
        {
            Behavior.DrawSelected(this, dc, pen);
        }

        public void DrawTemp(DrawContext dc, CadVector tp, int pen)
        {
            Behavior.DrawTemp(this, dc, tp, pen);
        }

        public void StartCreate(DrawContext dc)
        {
            Behavior.StartCreate(this, dc);
        }

        public Types EndCreate(DrawContext dc)
        {
            return Behavior.EndCreate(this, dc);
        }

        public CadRect GetContainsRect()
        {
            return Behavior.GetContainsRect(this);
        }

        public IReadOnlyList<CadVector> GetPoints(int curveSplitNum)
        {
            return Behavior.GetPoints(this, curveSplitNum);
        }

        public Centroid GetCentroid()
        {
            return Behavior.GetCentroid(this);
        }
        #endregion

        #region "Utilities"
        private static void copyPointList(List<CadVector> dest, IReadOnlyList<CadVector> src)
        {
            foreach (CadVector p in src)
            {
                dest.Add(p);
            }
        }
        #endregion
    }
}