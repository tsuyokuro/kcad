﻿using CadDataTypes;
using Plotter;
using System;
using System.Collections.Generic;

namespace HalfEdgeNS
{
    public class HeModelBuilder
    {
        public Dictionary<uint, HalfEdge> HeMap = new Dictionary<uint, HalfEdge>();

        public HeModel mHeModel;

        public void Start()
        {
            mHeModel = new HeModel();
        }

        public void Start(HeModel model)
        {
            mHeModel = model;
            SetupMap(HeMap, mHeModel);
        }

        public void SetupMap(Dictionary<uint, HalfEdge> map, HeModel hem)
        {
            for (int i = 0; i < hem.FaceStore.Count; i++)
            {
                HalfEdge head = hem.FaceStore[i].Head;
                HalfEdge c = head;

                for (; ; )
                {
                    map[HeConnector.GetHeKey(c)] = c;

                    c = c.Next;
                    if (c == head) break;
                }
            }
        }

        public HeModel Get()
        {
            return mHeModel;
        }


        // 三角形の追加
        // 左右回り方を統一して追加するようにする
        public void AddTriangle(CadVector v0, CadVector v1, CadVector v2)
        {
            AddTriangle(
                AddVertexWithoutSame(v0),
                AddVertexWithoutSame(v1),
                AddVertexWithoutSame(v2)
                );
        }

        // 三角形の追加
        // 左右回り方を統一して追加するようにする
        public void AddTriangle(int v0, int v1, int v2)
        {
            HalfEdge he0 = mHeModel.CreateHalfEdge(v0);
            HalfEdge he1 = mHeModel.CreateHalfEdge(v1);
            HalfEdge he2 = mHeModel.CreateHalfEdge(v2);

            he0.Next = he1;
            he0.Prev = he2;
            he1.Next = he2;
            he1.Prev = he0;
            he2.Next = he0;
            he2.Prev = he1;

            // 法線の設定
            CadVector normal = CadMath.Normal(
                mHeModel.VertexStore[v0],
                mHeModel.VertexStore[v1],
                mHeModel.VertexStore[v2]);

            int normalIndex = HeModel.INVALID_INDEX;

            if (!normal.Invalid)
            {
                normalIndex = mHeModel.NormalStore.Add(normal);
            }

            he0.Normal = normalIndex;
            he1.Normal = normalIndex;
            he2.Normal = normalIndex;

            // Faceの設定
            HeFace face = mHeModel.CreateFace(he0);
            face.Normal = normalIndex;

            int faceIndex = mHeModel.FaceStore.Add(face);

            he0.Face = faceIndex;
            he1.Face = faceIndex;
            he2.Face = faceIndex;

            // Pairの設定
            HeConnector.SetHalfEdgePair(he0, HeMap);
            HeMap[HeConnector.GetHeKey(he0)] = he0;

            HeConnector.SetHalfEdgePair(he1, HeMap);
            HeMap[HeConnector.GetHeKey(he1)] = he1;

            HeConnector.SetHalfEdgePair(he2, HeMap);
            HeMap[HeConnector.GetHeKey(he2)] = he2;
        }

        // 同じ座標がなければ追加してIndexを返す
        // 同じ座標があれば、そのIndexを返す
        public int AddVertexWithoutSame(CadVector v)
        {
            int cnt = mHeModel.VertexStore.Count;
            for (int i = 0; i < cnt; i++)
            {
                ref CadVector rv = ref mHeModel.VertexStore.Ref(i);
                if (v.Equals(rv))
                {
                    return i;
                }
            }

            return mHeModel.VertexStore.Add(v);
        }
    }
}