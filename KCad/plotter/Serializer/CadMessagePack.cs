using MessagePack;
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
        List<MpFigure> ChildList;

        [Key("geo")]
        public MpGeometricData GeoData;

        public static MpFigure Create(CadFigure fig, bool withChild = false)
        {
            MpFigure ret = new MpFigure();

            if (withChild)
            {
                ret.StoreWithChild(fig);
            }
            else
            {
                ret.Store(fig);
            }
            return ret;
        }

        public void Store(CadFigure fig)
        {
            ID = fig.ID;
            Type = (byte)fig.Type;
            Locked = fig.Locked;
            IsLoop = fig.IsLoop;
            Normal = MpVector.Create(fig.Normal);
            Tickness = fig.Thickness;

            GeoData = fig.GetMpGeometricData();
        }

        public void StoreWithChild(CadFigure fig)
        {
            Store(fig);
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
                fig.ChildList = MpUtil.MpToFigureList(ChildList);
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
        [Key("point_list")]
        public List<MpVector> VertexStore;

        [Key("normal_list")]
        public List<MpVector> NormalStore;
    }

    [MessagePackObject]
    public class MpGroupInfoItem
    {
        [Key("fig_id")]
        public uint FigureID;

        [Key("child_id_list")]
        public List<uint> ChildIdList;
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

        public static VectorList MpToVectortList(List<MpVector> list)
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

        public static List<CadFigure> MpToFigureList(List<MpFigure> list)
        {
            List<CadFigure> ret = new List<CadFigure>();
            for (int i = 0; i < list.Count; i++)
            {
                ret.Add(list[i].Restore());
            }

            return ret;
        }
    }



    class CadMessagePack
    {
    }
}
