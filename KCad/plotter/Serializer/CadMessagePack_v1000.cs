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
    // TODO 旧Versionの読み込み用なのでCreate系は不要

    [MessagePackObject]
    public class MpCadData_v1000
    {
        [Key("DB")]
        public MpCadObjectDB_v1000 MpDB;

        [Key("ViewInfo")]
        public MpViewInfo ViewInfo;

        [IgnoreMember]
        CadObjectDB DB = null;

        public static MpCadData_v1000 Create(CadObjectDB db)
        {
            MpCadData_v1000 ret = new MpCadData_v1000();

            ret.MpDB = MpCadObjectDB_v1000.Create(db);

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
    public class MpCadObjectDB_v1000
    {
        [Key("LayerIdCnt")]
        public uint LayerIdCount;

        [Key("FigIdCnt")]
        public uint FigureIdCount;

        [Key("FigList")]
        public List<MpFigure_v1000> FigureList;

        [Key("LayerList")]
        public List<MpLayer> LayerList;

        [Key("CurrentLayerID")]
        public uint CurrentLayerID;

        public static MpCadObjectDB_v1000 Create(CadObjectDB db)
        {
            MpCadObjectDB_v1000 ret = new MpCadObjectDB_v1000();

            ret.LayerIdCount = db.LayerIdProvider.Counter;
            ret.FigureIdCount = db.FigIdProvider.Counter;

            ret.FigureList = MpUtil.FigureMapToMp_v1000(db.FigureMap);

            ret.LayerList = MpUtil.LayerListToMp(db.LayerList);

            ret.CurrentLayerID = db.CurrentLayerID;

            return ret;
        }

        public void GarbageCollect()
        {
            var idMap = new Dictionary<uint, MpFigure_v1000>();

            foreach (MpFigure_v1000 fig in FigureList)
            {
                idMap.Add(fig.ID, fig);
            }

            var activeSet = new HashSet<uint>();

            foreach (MpLayer layer in LayerList)
            {
                foreach (uint id in layer.FigureIdList)
                {
                    MpFigure_v1000 fig = idMap[id];

                    fig.ForEachFigID(idMap, (a) => {
                        activeSet.Add(a);
                    });
                }
            }

            int i = FigureList.Count - 1;

            for (; i >= 0; i--)
            {
                MpFigure_v1000 fig = FigureList[i];

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
            List<CadFigure> figList = MpUtil.FigureListFromMp_v1000(FigureList);

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
                MpFigure_v1000 mpfig = FigureList[i];
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

        private void SetFigChild(MpFigure_v1000 mpfig, Dictionary<uint, CadFigure> dic)
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
    public class MpFigure_v1000
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
        public MpVertex Normal;

        [Key("ChildList")]
        public List<MpFigure_v1000> ChildList;

        [Key("ChildIdList")]
        public List<uint> ChildIdList;

        [Key("GeoData")]
        public MpGeometricData GeoData;

        [IgnoreMember]
        public CadFigure TempFigure = null;

        public static MpFigure_v1000 Create(CadFigure fig, bool withChild = false)
        {
            MpFigure_v1000 ret = new MpFigure_v1000();

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

        public virtual void ForEachFig(Action<MpFigure_v1000> d)
        {
            d(this);

            if (ChildList == null)
            {
                return;
            }

            int i;
            for (i = 0; i < ChildList.Count; i++)
            {
                MpFigure_v1000 c = ChildList[i];
                c.ForEachFig(d);
            }
        }

        public virtual void ForEachFigID(Dictionary<uint, MpFigure_v1000> allMap, Action<uint> d)
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
                MpFigure_v1000 childFig = allMap[id];
                childFig.ForEachFigID(allMap, d);
            }
        }

        public void StoreCommon(CadFigure fig)
        {
            ID = fig.ID;
            Type = (byte)fig.Type;
            Locked = fig.Locked;
            IsLoop = fig.IsLoop;
            Normal = MpVertex.Create((CadVertex)fig.Normal);

            GeoData = fig.GeometricDataToMp();
        }

        public void StoreChildIdList(CadFigure fig)
        {
            ChildIdList = MpUtil.FigureListToIdList(fig.ChildList);
        }

        public void StoreChildList(CadFigure fig)
        {
            ChildList = MpUtil.FigureListToMp_v1000(fig.ChildList);
        }

        public void RestoreTo(CadFigure fig)
        {
            fig.ID = ID;
            fig.Locked = Locked;
            fig.IsLoop = IsLoop;
            fig.Normal = Normal.Restore().vector;

            if (ChildList != null)
            {
                fig.ChildList = MpUtil.FigureListFromMp_v1000(ChildList);

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