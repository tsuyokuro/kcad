using HalfEdgeNS;
using MyCollections;
using System;
using System.Collections.Generic;
using CadDataTypes;


namespace Plotter.Serializer
{
    public partial class MpUtil
    {
        public static List<MpVertex> VertexListToMp(VertexList v)
        {
            List<MpVertex> ret = new List<MpVertex>();
            for (int i=0; i<v.Count; i++)
            {
                ret.Add(MpVertex.Create(v[i]));
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

        public static VertexList VertexListFromMp(List<MpVertex> list)
        {
            VertexList ret = new VertexList(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                ret.Add(list[i].Restore());
            }

            return ret;
        }

        public static List<MpVector3d> Vector3dListToMp(Vector3dList v)
        {
            List<MpVector3d> ret = new List<MpVector3d>();
            for (int i = 0; i < v.Count; i++)
            {
                ret.Add(MpVector3d.Create(v[i]));
            }

            return ret;
        }

        public static Vector3dList Vector3dListFromMp(List<MpVector3d> list)
        {
            Vector3dList ret = new Vector3dList(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                ret.Add(list[i].Restore());
            }

            return ret;
        }

        public static List<MpFigure_v1000> FigureListToMp_v1000(List<CadFigure> figList, bool withChild=false)
        {
            List<MpFigure_v1000> ret = new List<MpFigure_v1000>();
            for (int i = 0; i < figList.Count; i++)
            {
                ret.Add(MpFigure_v1000.Create(figList[i], withChild));
            }

            return ret;
        }

        public static List<MpFigure_v1001> FigureListToMp_1001(List<CadFigure> figList, bool withChild = false)
        {
            List<MpFigure_v1001> ret = new List<MpFigure_v1001>();
            for (int i = 0; i < figList.Count; i++)
            {
                ret.Add(MpFigure_v1001.Create(figList[i], withChild));
            }

            return ret;
        }

        public static List<CadFigure> FigureListFromMp_v1000(List<MpFigure_v1000> list)
        {
            List<CadFigure> ret = new List<CadFigure>();
            for (int i = 0; i < list.Count; i++)
            {
                ret.Add(list[i].Restore());
            }

            return ret;
        }

        public static List<CadFigure> FigureListFromMp_1001(List<MpFigure_v1001> list)
        {
            List<CadFigure> ret = new List<CadFigure>();
            for (int i = 0; i < list.Count; i++)
            {
                ret.Add(list[i].Restore());
            }

            return ret;
        }

        public static List<MpFigure_v1000> FigureMapToMp_v1000(
            Dictionary<uint, CadFigure> figMap, bool withChild = false)
        {
            List<MpFigure_v1000> ret = new List<MpFigure_v1000>();
            foreach (CadFigure fig in figMap.Values)
            {
                ret.Add(MpFigure_v1000.Create(fig, withChild));
            }

            return ret;
        }

        public static List<MpFigure_v1001> FigureMapToMp_1001(
            Dictionary<uint, CadFigure> figMap, bool withChild = false)
        {
            List<MpFigure_v1001> ret = new List<MpFigure_v1001>();
            foreach (CadFigure fig in figMap.Values)
            {
                ret.Add(MpFigure_v1001.Create(fig, withChild));
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
