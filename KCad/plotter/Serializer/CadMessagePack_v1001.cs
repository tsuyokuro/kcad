using HalfEdgeNS;
using MessagePack;
using System;
using System.Collections.Generic;
using CadDataTypes;
using SplineCurve;
using System.Drawing.Printing;
using System.Linq;
using OpenTK;

namespace Plotter.Serializer
{
    [MessagePackObject]
    public class MpCadData_Latest
    {
        [Key("DB")]
        public MpCadObjectDB_Latest MpDB;

        [Key("ViewInfo")]
        public MpViewInfo ViewInfo;

        [IgnoreMember]
        CadObjectDB DB = null;

        public static MpCadData_Latest Create(CadObjectDB db)
        {
            MpCadData_Latest ret = new MpCadData_Latest();

            ret.MpDB = MpCadObjectDB_Latest.Create(db);

            ret.ViewInfo = new MpViewInfo();

            return ret;
        }

        public CadObjectDB GetDB()
        {
            if (DB == null)
            {
                DB = MpDB.Restore();
            }

            return DB;
        }
    }

    [MessagePackObject]
    public class MpCadObjectDB_Latest
    {
        [Key("LayerIdCnt")]
        public uint LayerIdCount;

        [Key("FigIdCnt")]
        public uint FigureIdCount;

        [Key("FigList")]
        public List<MpFigure_Latest> FigureList;

        [Key("LayerList")]
        public List<MpLayer> LayerList;

        [Key("CurrentLayerID")]
        public uint CurrentLayerID;

        public static MpCadObjectDB_Latest Create(CadObjectDB db)
        {
            MpCadObjectDB_Latest ret = new MpCadObjectDB_Latest();

            ret.LayerIdCount = db.LayerIdProvider.Counter;
            ret.FigureIdCount = db.FigIdProvider.Counter;

            ret.FigureList = MpUtil.FigureMapToMp_Latest(db.FigureMap);

            ret.LayerList = MpUtil.LayerListToMp(db.LayerList);

            ret.CurrentLayerID = db.CurrentLayerID;

            return ret;
        }

        public void GarbageCollect()
        {
            var idMap = new Dictionary<uint, MpFigure_Latest>();

            foreach (MpFigure_Latest fig in FigureList)
            {
                idMap.Add(fig.ID, fig);
            }

            var activeSet = new HashSet<uint>();

            foreach (MpLayer layer in LayerList)
            {
                foreach (uint id in layer.FigureIdList)
                {
                    MpFigure_Latest fig = idMap[id];

                    fig.ForEachFigID(idMap, (a) =>
                    {
                        activeSet.Add(a);
                    });
                }
            }

            int i = FigureList.Count - 1;

            for (; i >= 0; i--)
            {
                MpFigure_Latest fig = FigureList[i];

                if (!activeSet.Contains(fig.ID))
                {
                    FigureList.RemoveAt(i);
                }
            }
        }

        public CadObjectDB Restore()
        {
            CadObjectDB ret = new CadObjectDB();

            ret.LayerIdProvider.Counter = LayerIdCount;
            ret.FigIdProvider.Counter = FigureIdCount;

            // Figure map
            List<CadFigure> figList = MpUtil.FigureListFromMp_Latest(FigureList);

            var dic = new Dictionary<uint, CadFigure>();

            for (int i = 0; i < figList.Count; i++)
            {
                CadFigure fig = figList[i];

                dic.Add(fig.ID, fig);
                FigureList[i].TempFigure = fig;
            }

            ret.FigureMap = dic;


            // Child list
            for (int i = 0; i < figList.Count; i++)
            {
                MpFigure_Latest mpfig = FigureList[i];
                SetFigChild(mpfig, dic);
            }


            // Layer map
            ret.LayerList = MpUtil.LayerListFromMp(LayerList, dic);

            ret.LayerMap = new Dictionary<uint, CadLayer>();

            for (int i = 0; i < ret.LayerList.Count; i++)
            {
                CadLayer layer = ret.LayerList[i];

                ret.LayerMap.Add(layer.ID, layer);
            }

            ret.CurrentLayerID = CurrentLayerID;

            return ret;
        }

        private void SetFigChild(MpFigure_Latest mpfig, Dictionary<uint, CadFigure> dic)
        {
            for (int i = 0; i < mpfig.ChildIdList.Count; i++)
            {
                uint id = mpfig.ChildIdList[i];

                mpfig.TempFigure.ChildList.Add(dic[id]);
                dic[id].Parent = mpfig.TempFigure;
            }
        }
    }

    [MessagePackObject]
    public struct MpVector3d
    {
        [Key(0)]
        public double X;

        [Key(1)]
        public double Y;

        [Key(2)]
        public double Z;

        public static MpVector3d Create(Vector3d v)
        {
            MpVector3d ret = new MpVector3d();

            ret.X = v.X;
            ret.Y = v.Y;
            ret.Z = v.Z;

            return ret;
        }

        public Vector3d Restore()
        {
            return new Vector3d(X, Y, Z);
        }
    }

    [MessagePackObject]
    public class MpFigure_Latest
    {
        [Key("ID")]
        public uint ID;

        [Key("Type")]
        public byte Type;

        [Key("Locked")]
        public bool Locked;

        [Key("IsLoop")]
        public bool IsLoop;

        [Key("Normal")]
        public MpVector3d Normal;

        [Key("ChildList")]
        public List<MpFigure_Latest> ChildList;

        [Key("ChildIdList")]
        public List<uint> ChildIdList;

        [Key("GeoData")]
        public MpGeometricData GeoData;

        [IgnoreMember]
        public CadFigure TempFigure = null;

        public static MpFigure_Latest Create(CadFigure fig, bool withChild = false)
        {
            MpFigure_Latest ret = new MpFigure_Latest();

            ret.StoreCommon(fig);

            if (withChild)
            {
                ret.StoreChildList(fig);
            }
            else
            {
                ret.StoreChildIdList(fig);
            }
            return ret;
        }

        public virtual void ForEachFig(Action<MpFigure_Latest> d)
        {
            d(this);

            if (ChildList == null)
            {
                return;
            }

            int i;
            for (i = 0; i < ChildList.Count; i++)
            {
                MpFigure_Latest c = ChildList[i];
                c.ForEachFig(d);
            }
        }

        public virtual void ForEachFigID(Dictionary<uint, MpFigure_Latest> allMap, Action<uint> d)
        {
            d(ID);

            if (ChildIdList == null)
            {
                return;
            }

            int i;
            for (i = 0; i < ChildIdList.Count; i++)
            {
                uint id = ChildIdList[i];
                MpFigure_Latest childFig = allMap[id];
                childFig.ForEachFigID(allMap, d);
            }
        }

        public void StoreCommon(CadFigure fig)
        {
            ID = fig.ID;
            Type = (byte)fig.Type;
            Locked = fig.Locked;
            IsLoop = fig.IsLoop;
            Normal = MpVector3d.Create(fig.Normal.vector);

            GeoData = fig.GeometricDataToMp();
        }

        public void StoreChildIdList(CadFigure fig)
        {
            ChildIdList = MpUtil.FigureListToIdList(fig.ChildList);
        }

        public void StoreChildList(CadFigure fig)
        {
            ChildList = MpUtil.FigureListToMp_Latest(fig.ChildList);
        }

        public void RestoreTo(CadFigure fig)
        {
            fig.ID = ID;
            fig.Locked = Locked;
            fig.IsLoop = IsLoop;
            fig.Normal = (CadVertex)Normal.Restore();

            if (ChildList != null)
            {
                fig.ChildList = MpUtil.FigureListFromMp_Latest(ChildList);

                for (int i = 0; i < fig.ChildList.Count; i++)
                {
                    CadFigure c = fig.ChildList[i];
                    c.Parent = fig;
                }
            }
            else
            {
                fig.ChildList.Clear();
            }

            fig.GeometricDataFromMp(GeoData);
        }

        public CadFigure Restore()
        {
            CadFigure fig = CadFigure.Create((CadFigure.Types)Type);

            RestoreTo(fig);

            return fig;
        }
    }
}