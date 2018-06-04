using HalfEdgeNS;
using MessagePack;
using MyCollections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadDataTypes;
using SplineCurve;

namespace Plotter.Serializer
{
    [MessagePackObject]
    public class MpDummy
    {
        [Key("value")]
        int value = 0;
    }

    public class MpInitializer
    {
        public static void Init()
        {
            MpDummy v = new MpDummy();

            byte[] b = MessagePackSerializer.Serialize(v);

            v = MessagePackSerializer.Deserialize<MpDummy>(b);
        }
    }


    [MessagePackObject]
    public struct MpVector
    {
        [Key(0)]
        public byte Flag;

        [Key(1)]
        public double x;

        [Key(2)]
        public double y;

        [Key(3)]
        public double z;

        public static MpVector Create(CadVector v)
        {
            MpVector ret = new MpVector();
            ret.Flag = v.Flag;
            ret.x = v.x;
            ret.y = v.y;
            ret.z = v.z;
            return ret;
        }

        public CadVector Restore()
        {
            CadVector v = CadVector.Create(x, y, z);
            v.Flag = Flag;

            return v;
        }
    }

    public class MpCadFile
    {
        private static byte[] Sign;
        private static byte[] CurrentVersion = { 1, 0, 0, 0 };

        static MpCadFile()
        {
            Sign = Encoding.ASCII.GetBytes("KCAD_BIN");
        }

        public static byte[] Load(string fname)
        {
            FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read);

            byte[] sig = new byte[Sign.Length];

            fs.Read(sig,0, Sign.Length);

            if (!Sign.SequenceEqual<byte>(sig))
            {
                fs.Close();
                return null;
            }

            fs.Read(CurrentVersion, 0, CurrentVersion.Length);

            byte[] data = new byte[fs.Length - Sign.Length - CurrentVersion.Length];

            fs.Read(data, 0, data.Length);

            fs.Close();

            return data;
        }

        public static void Save(string fname, byte[] data)
        {
            FileStream fs = new FileStream(fname, FileMode.Create, FileAccess.Write);

            fs.Write(Sign, 0, Sign.Length);
            fs.Write(CurrentVersion, 0, CurrentVersion.Length);
            fs.Write(data, 0, data.Length);

            fs.Close();
        }
    }


    [MessagePackObject]
    public class MpCadData
    {
        [Key("db")]
        public MpCadObjectDB MpDB;

        [IgnoreMember]
        CadObjectDB DB = null;

        public static MpCadData Create(CadObjectDB db)
        {
            MpCadData ret = new MpCadData();

            ret.MpDB = MpCadObjectDB.Create(db);

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
    public class MpCadObjectDB
    {
        [Key("layerIdC")]
        public uint LayerIdCount;

        [Key("figIdC")]
        public uint FigureIdCount;

        [Key("figL")]
        public List<MpFigure> FigureList;

        [Key("layerL")]
        public List<MpLayer> LayerList;

        [Key("layerID")]
        public uint CurrentLayerID;

        public static MpCadObjectDB Create(CadObjectDB db)
        {
            MpCadObjectDB ret = new MpCadObjectDB();

            ret.LayerIdCount = db.LayerIdProvider.Counter;
            ret.FigureIdCount = db.FigIdProvider.Counter;

            ret.FigureList = MpUtil.FigureMapToMp(db.FigureMap);

            ret.LayerList = MpUtil.LayerListToMp(db.LayerList);

            ret.CurrentLayerID = db.CurrentLayerID;

            return ret;
        }

        public CadObjectDB Restore()
        {
            CadObjectDB ret = new CadObjectDB();

            ret.LayerIdProvider.Counter = LayerIdCount;
            ret.FigIdProvider.Counter = FigureIdCount;

            // Figure map
            List<CadFigure> figList = MpUtil.FigureListFromMp(FigureList);

            var dic = new Dictionary<uint, CadFigure>();

            for (int i=0; i<figList.Count; i++)
            {
                CadFigure fig = figList[i];

                dic.Add(fig.ID, fig);
                FigureList[i].TempFigure = fig;
            }

            ret.FigureMap = dic;


            // Child list
            for (int i = 0; i < figList.Count; i++)
            {
                MpFigure mpfig = FigureList[i];
                SetFigChild(mpfig, dic);
            }


            // Layer map
            ret.LayerList = MpUtil.LayerListFromMp(LayerList, dic);

            ret.LayerMap = new Dictionary<uint, CadLayer>();

            for (int i=0; i< ret.LayerList.Count; i++)
            {
                CadLayer layer = ret.LayerList[i];

                ret.LayerMap.Add(layer.ID, layer);
            }

            ret.CurrentLayerID = CurrentLayerID;

            return ret;
        }

        private void SetFigChild(MpFigure mpfig, Dictionary<uint, CadFigure> dic)
        {
            for (int i=0; i<mpfig.ChildIdList.Count; i++)
            {
                uint id = mpfig.ChildIdList[i];

                mpfig.TempFigure.ChildList.Add(dic[id]);
                dic[id].Parent = mpfig.TempFigure;
            }
        }

    }

    [MessagePackObject]
    public class MpLayer
    {
        [Key("id")]
        public uint ID;

        [Key("vis")]
        public bool Visible;

        [Key("lck")]
        public bool Locked;

        [Key("figIL")]
        public List<uint> FigureIdList;

        public static MpLayer Create(CadLayer layer)
        {
            MpLayer ret = new MpLayer();

            ret.ID = layer.ID;
            ret.Visible = layer.Visible;
            ret.Locked = layer.Locked;

            ret.FigureIdList = MpUtil.FigureListToIdList(layer.FigureList);

            return ret;
        }

        public CadLayer Restore(Dictionary<uint, CadFigure> dic)
        {
            CadLayer ret = new CadLayer();
            ret.ID = ID;
            ret.Visible = Visible;
            ret.Locked = Locked;
            ret.FigureList = new List<CadFigure>();

            for (int i=0; i<FigureIdList.Count; i++)
            {
                ret.AddFigure(dic[FigureIdList[i]]);
            }

            return ret;
        }
    }

    [MessagePackObject]
    public class MpFigure
    {
        [Key("id")]
        public uint ID;

        [Key("typ")]
        public byte Type;

        [Key("lck")]
        public bool Locked;

        [Key("lop")]
        public bool IsLoop;

        [Key("nrm")]
        public MpVector Normal;

        //[Key("tck")]
        //public double Tickness;

        [Key("cL")]
        public List<MpFigure> ChildList;

        [Key("cIL")]
        public List<uint> ChildIdList;

        [Key("geo")]
        public MpGeometricData GeoData;

        [IgnoreMember]
        public CadFigure TempFigure = null;

        public static MpFigure Create(CadFigure fig, bool withChild = false)
        {
            MpFigure ret = new MpFigure();

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

        public void StoreCommon(CadFigure fig)
        {
            ID = fig.ID;
            Type = (byte)fig.Type;
            Locked = fig.Locked;
            IsLoop = fig.IsLoop;
            Normal = MpVector.Create(fig.Normal);
            //Tickness = fig.Thickness;

            GeoData = fig.GeometricDataToMp();
        }
        public void StoreChildIdList(CadFigure fig)
        {
            ChildIdList = MpUtil.FigureListToIdList(fig.ChildList);
        }

        public void StoreChildList(CadFigure fig)
        {
            ChildList = MpUtil.FigureListToMp(fig.ChildList);
        }

        public void RestoreTo(CadFigure fig)
        {
            fig.ID = ID;
            fig.Locked = Locked;
            fig.IsLoop = IsLoop;
            fig.Normal = Normal.Restore();
            //fig.Thickness = Tickness;

            if (ChildList != null)
            {
                fig.ChildList = MpUtil.FigureListFromMp(ChildList);

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

    [MessagePack.Union(0, typeof(MpSimpleGeometricData))]
    [MessagePack.Union(1, typeof(MpMeshGeometricData))]
    [MessagePack.Union(2, typeof(MpNurbsLineGeometricData))]
    public interface MpGeometricData
    {
    }

    #region Geomeric data

    [MessagePackObject]
    public class MpSimpleGeometricData : MpGeometricData
    {
        [Key("ptL")]
        public List<MpVector> PointList;
    }


    [MessagePackObject]
    public class MpMeshGeometricData : MpGeometricData
    {
        [Key("heModel")]
        public MpHeModel HeModel;
    }

    [MessagePackObject]
    public class MpNurbsLineGeometricData : MpGeometricData
    {
        [Key("ptL")]
        public List<MpVector> PointList;

        [Key("Nurbs")]
        public MpNurbsLine Nurbs;
    }

    #endregion


    [MessagePackObject]
    public class MpHeModel
    {
        [Key("vStr")]
        public List<MpVector> VertexStore;

        [Key("nStr")]
        public List<MpVector> NormalStore;

        [Key("fStr")]
        public List<MpHeFace> FaceStore;

        [Key("heL")]
        public List<MpHalfEdge> HalfEdgeList;

        [Key("heIdC")]
        public uint HeIdCount;

        [Key("fcIdC")]
        public uint FaceIdCount;


        public static MpHeModel Create(HeModel model)
        {
            MpHeModel ret = new MpHeModel();

            ret.VertexStore = MpUtil.VectortListToMp(model.VertexStore);

            ret.NormalStore = MpUtil.VectortListToMp(model.NormalStore);

            ret.FaceStore = MpUtil.HeFaceListToMp(model.FaceStore);

            ret.HeIdCount = model.HeIdProvider.Counter;

            ret.FaceIdCount = model.FaceIdProvider.Counter;

            List<HalfEdge> heList = model.GetHalfEdgeList();

            ret.HalfEdgeList = MpUtil.HalfEdgeListToMp(heList);

            return ret;
        }

        public HeModel Restore()
        {
            HeModel ret = new HeModel();

            ret.VertexStore = MpUtil.VectortListFromMp(VertexStore);

            ret.NormalStore = MpUtil.VectortListFromMp(NormalStore);

            // Create dictionary
            Dictionary<uint, HalfEdge> dic = new Dictionary<uint, HalfEdge>();

            dic[0] = null;

            for (int i=0; i< HalfEdgeList.Count; i++)
            {
                HalfEdge he = HalfEdgeList[i].Restore();
                dic.Add(he.ID, he);

                HalfEdgeList[i].TempHalfEdge = he;
            }

            // Create links
            for (int i = 0; i < HalfEdgeList.Count; i++)
            {
                HalfEdge he = HalfEdgeList[i].TempHalfEdge;
                he.Pair = dic[HalfEdgeList[i].PairID];
                he.Next = dic[HalfEdgeList[i].NextID];
                he.Prev = dic[HalfEdgeList[i].PrevID];
            }

            ret.FaceStore = MpUtil.HeFaceListFromMp(FaceStore, dic);

            ret.HeIdProvider.Counter = HeIdCount;

            ret.FaceIdProvider.Counter = FaceIdCount;

            return ret;
        }
    }

    [MessagePackObject]
    public class MpHeFace
    {
        [Key("ID")]
        public uint ID;

        [Key("head")]
        public uint HeadID;

        [Key("nrm")]
        public int Normal = HeModel.INVALID_INDEX;

        public static MpHeFace Create(HeFace face)
        {
            MpHeFace ret = new MpHeFace();
            ret.ID = face.ID;
            ret.HeadID = face.Head.ID;
            ret.Normal = face.Normal;

            return ret;
        }

        public HeFace Restore(Dictionary<uint, HalfEdge> dic)
        {
            HalfEdge he = dic[HeadID];

            HeFace ret = new HeFace(he);

            ret.ID = ID;

            ret.Normal = Normal;

            return ret;
        }
    }

    [MessagePackObject]
    public class MpHalfEdge
    {
        [Key("id")]
        public uint ID;

        [Key("v")]
        public int Vertex;

        [Key("face")]
        public int Face;

        [Key("nrm")]
        public int Normal;

        // Links
        [Key("pair")]
        public uint PairID;

        [Key("next")]
        public uint NextID;

        [Key("prev")]
        public uint PrevID;

        [IgnoreMember]
        public HalfEdge TempHalfEdge = null;


        public static MpHalfEdge Create(HalfEdge he)
        {
            MpHalfEdge ret = new MpHalfEdge();

            ret.ID = he.ID;
            ret.PairID = he.Pair != null ? he.Pair.ID : 0;
            ret.NextID = he.Next != null ? he.Next.ID : 0;
            ret.PrevID = he.Prev != null ? he.Prev.ID : 0;

            ret.Vertex = he.Vertex;
            ret.Face = he.Face;
            ret.Normal = he.Normal;

            return ret;
        }

        // リンク情報はRestoreされない
        public HalfEdge Restore()
        {
            HalfEdge he = new HalfEdge();

            he.ID = ID;
            he.Vertex = Vertex;
            he.Normal = Normal;
            he.Face = Face;

            return he;
        }
    }

    [MessagePackObject]
    public class MpNurbsLine
    {
        [Key("Closed")]
        public bool Closed;

        [Key("PassEdge")]
        public bool PassEdge;

        [Key("DivCnt")]
        public int DivCnt;

        [Key("CtrlCnt")]
        public int CtrlCnt;

        [Key("CtrlDataCnt")]
        public int CtrlDataCnt;

        [Key("Weights")]
        public double[] Weights;

        [Key("Order")]
        public int[] Order;

        [Key("BSplineP")]
        public MpBSplineParam BSplineP;

        public static MpNurbsLine Create(NURBSLine src)
        {
            MpNurbsLine ret = new MpNurbsLine();

            ret.Closed = src.Closed;
            ret.PassEdge = src.PassEdge;
            ret.CtrlCnt = src.CtrlCnt;
            ret.CtrlDataCnt = src.CtrlDataCnt;
            ret.Weights = MpUtil.ArrayClone<double>(src.Weights);
            ret.Order = MpUtil.ArrayClone<int>(src.Order);

            ret.BSplineP = MpBSplineParam.Create(src.BSplineP);

            return ret;
        }

        public NURBSLine Restore()
        {
            NURBSLine nurbs = new NURBSLine();

            nurbs.Closed = Closed;
            nurbs.PassEdge = PassEdge;
            nurbs.CtrlCnt = CtrlCnt;
            nurbs.CtrlDataCnt = CtrlDataCnt;
            nurbs.Weights = MpUtil.ArrayClone<double>(Weights);
            nurbs.Order = MpUtil.ArrayClone<int>(Order);

            nurbs.BSplineP = BSplineP.Restore();

            return nurbs;
        }
    }

    [MessagePackObject]
    public class MpBSplineParam
    {
        [Key("Degree")]
        public int Degree = 3;

        [Key("DivCnt")]
        public int DivCnt = 0;

        [Key("OutputCnt")]
        public int OutputCnt = 0;

        [Key("KnotCnt")]
        public int KnotCnt;

        [Key("Knots")]
        public double[] Knots;

        [Key("CtrlCnt")]
        public int CtrlCnt;

        [Key("LowKnot")]
        public double LowKnot = 0;

        [Key("HightKnot")]
        public double HighKnot = 0;

        [Key("Step")]
        public double Step = 0;

        public static MpBSplineParam Create(BSplineParam src)
        {
            MpBSplineParam ret = new MpBSplineParam();

            ret.Degree = src.Degree;
            ret.DivCnt = src.DivCnt;
            ret.OutputCnt = src.OutputCnt;
            ret.KnotCnt = src.KnotCnt;
            ret.Knots = MpUtil.ArrayClone<double>(src.Knots);
            ret.LowKnot = src.LowKnot;
            ret.HighKnot = src.HighKnot;
            ret.Step = src.Step;

            return ret;
        }

        public BSplineParam Restore()
        {
            BSplineParam bs = new BSplineParam();

            bs.Degree = Degree;
            bs.DivCnt = DivCnt;
            bs.OutputCnt = OutputCnt;
            bs.KnotCnt = KnotCnt;
            bs.Knots = MpUtil.ArrayClone<double>(Knots);
            bs.LowKnot = LowKnot;
            bs.HighKnot = HighKnot;
            bs.Step = Step;

            return bs;
        }
    }

    public class MpUtil
    {
        public static List<MpVector> VectortListToMp(VectorList v)
        {
            List<MpVector> ret = new List<MpVector>();
            for (int i=0; i<v.Count; i++)
            {
                ret.Add(MpVector.Create(v[i]));
            }

            return ret;
        }

        public static List<uint> FigureListToIdList(List<CadFigure> figList )
        {
            List<uint> ret = new List<uint>();
            for (int i = 0; i < figList.Count; i++)
            {
                ret.Add(figList[i].ID);
            }

            return ret;
        }

        public static VectorList VectortListFromMp(List<MpVector> list)
        {
            VectorList ret = new VectorList(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                ret.Add(list[i].Restore());
            }

            return ret;
        }

        public static List<MpFigure> FigureListToMp(List<CadFigure> figList, bool withChild=false)
        {
            List<MpFigure> ret = new List<MpFigure>();
            for (int i = 0; i < figList.Count; i++)
            {
                ret.Add(MpFigure.Create(figList[i], withChild));
            }

            return ret;
        }

        public static List<CadFigure> FigureListFromMp(List<MpFigure> list)
        {
            List<CadFigure> ret = new List<CadFigure>();
            for (int i = 0; i < list.Count; i++)
            {
                ret.Add(list[i].Restore());
            }

            return ret;
        }

        public static List<MpFigure> FigureMapToMp(
            Dictionary<uint, CadFigure> figMap, bool withChild = false)
        {
            List<MpFigure> ret = new List<MpFigure>();
            foreach (CadFigure fig in figMap.Values)
            {
                ret.Add(MpFigure.Create(fig, withChild));
            }

            return ret;
        }

        public static List<MpHeFace> HeFaceListToMp(FlexArray<HeFace> list)
        {
            List<MpHeFace> ret = new List<MpHeFace>();
            for (int i = 0; i < list.Count; i++)
            {
                ret.Add(MpHeFace.Create(list[i]));
            }

            return ret;
        }


        public static List<MpLayer> LayerListToMp(List<CadLayer> src)
        {
            List<MpLayer> ret = new List<MpLayer>();
            for (int i = 0; i < src.Count; i++)
            {
                ret.Add(MpLayer.Create(src[i]));
            }

            return ret;
        }

        public static List<CadLayer> LayerListFromMp(
            List<MpLayer> src, Dictionary<uint, CadFigure> dic)
        {
            List<CadLayer> ret = new List<CadLayer>();
            for (int i = 0; i < src.Count; i++)
            {
                ret.Add(src[i].Restore(dic));
            }

            return ret;
        }

        public static FlexArray<HeFace> HeFaceListFromMp(
            List<MpHeFace> list,
            Dictionary<uint, HalfEdge> dic
            )
        {
            FlexArray<HeFace> ret = new FlexArray<HeFace>();
            for (int i = 0; i < list.Count; i++)
            {
                ret.Add(list[i].Restore(dic));
            }

            return ret;
        }

        public static List<MpHalfEdge> HalfEdgeListToMp(List<HalfEdge> list)
        {
            List<MpHalfEdge> ret = new List<MpHalfEdge>();
            for (int i=0; i<list.Count; i++)
            {
                ret.Add(MpHalfEdge.Create(list[i]));
            }

            return ret;
        }

        public static T[] ArrayClone<T>(T[] src)
        {
            T[] dst = new T[src.Length];

            Array.Copy(src, dst, src.Length);

            return dst;
        } 
    }
}
