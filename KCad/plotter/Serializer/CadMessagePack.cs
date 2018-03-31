using HalfEdgeNS;
using MessagePack;
using MyCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter.Serializer
{
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

    [MessagePackObject]
    public class MpFigure
    {
        [Key("id")]
        public uint ID;

        [Key("type")]
        public byte Type;

        [Key("lock")]
        public bool Locked;

        [Key("loop")]
        public bool IsLoop;

        [Key("n")]
        public MpVector Normal;

        [Key("thick")]
        public double Tickness;

        [Key("children")]
        public List<MpFigure> ChildList;

        [Key("child_ids")]
        public List<uint> ChildIdList;

        [Key("geo")]
        public MpGeometricData GeoData;

        public static MpFigure Create(CadFigure fig, bool withChild = false)
        {
            MpFigure ret = new MpFigure();

            if (withChild)
            {
                ret.StoreWithChildList(fig);
            }
            else
            {
                ret.StoreWithChildIdList(fig);
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
            Tickness = fig.Thickness;

            GeoData = fig.GeometricDataToMp();
        }
        public void StoreWithChildIdList(CadFigure fig)
        {
            StoreCommon(fig);
            ChildIdList = MpUtil.FigureListToIdList(fig.ChildList);
        }

        public void StoreWithChildList(CadFigure fig)
        {
            StoreCommon(fig);
            ChildList = MpUtil.FigureListToMp(fig.ChildList);
        }

        public CadFigure Restore()
        {
            CadFigure fig = CadFigure.Create((CadFigure.Types)Type);

            fig.ID = ID;
            fig.Locked = Locked;
            fig.IsLoop = IsLoop;
            fig.Normal = Normal.Restore();
            fig.Thickness = Tickness;

            if (ChildList != null)
            {
                fig.ChildList = MpUtil.FigureListFromMp(ChildList);
            }

            fig.GeometricDataFromMp(GeoData);

            return fig;
        }
    }

    [MessagePack.Union(0, typeof(MpPolylineGeometricData))]
    [MessagePack.Union(1, typeof(MpMeshGeometricData))]
    public interface MpGeometricData
    {
    }

    [MessagePackObject]
    public class MpPolylineGeometricData : MpGeometricData
    {
        [Key("point_list")]
        public List<MpVector> PointList;
    }


    [MessagePackObject]
    public class MpMeshGeometricData : MpGeometricData
    {
        [Key("he_model")]
        public MpHeModel HeModel;

        [Key("edge")]
        public List<int> Edge;
    }

    [MessagePackObject]
    public class MpHeModel
    {
        [Key("vertex_store")]
        public List<MpVector> VertexStore;

        [Key("normal_store")]
        public List<MpVector> NormalStore;

        [Key("face_store")]
        public List<MpHeFace> FaceStore;


        [Key("id_count")]
        public uint HeIdCount;

        [Key("he_list")]
        public List<MpHalfEdge> HalfEdgeList;

        public static MpHeModel Create(HeModel model)
        {
            MpHeModel ret = new MpHeModel();

            ret.VertexStore = MpUtil.VectortListToMp(model.VertexStore);

            ret.NormalStore = MpUtil.VectortListToMp(model.NormalStore);

            ret.FaceStore = MpUtil.HeFaceListToMp(model.FaceStore);

            ret.HeIdCount = model.HeIdProvider.Counter;

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

            return ret;
        }
    }

    [MessagePackObject]
    public class MpHeFace
    {
        [Key("head")]
        public uint HeadID;
        [Key("normal")]
        public int Normal = HeModel.INVALID_INDEX;

        public static MpHeFace Create(HeFace face)
        {
            MpHeFace ret = new MpHeFace();
            ret.HeadID = face.Head.ID;
            ret.Normal = face.Normal;

            return ret;
        }

        public HeFace Restore(Dictionary<uint, HalfEdge> dic)
        {
            HalfEdge he = dic[HeadID];

            HeFace ret = new HeFace(he);

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

        [Key("normal")]
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

        public static List<MpHeFace> HeFaceListToMp(FlexArray<HeFace> list)
        {
            List<MpHeFace> ret = new List<MpHeFace>();
            for (int i = 0; i < list.Count; i++)
            {
                ret.Add(MpHeFace.Create(list[i]));
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
    }
}
