using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Plotter
{
    public struct FigureBelong
    {
        public CadLayer Layer;
        public int Index;
    }

    public class CadObjectDB
    {
        public const uint Version = 0x00010000;

        #region "Manage Layer"

        public uint CurrentLayerID
        {
            get
            {
                if (mCurrentLayer == null)
                {
                    return 0;
                }
                else
                {
                    return mCurrentLayer.ID;
                }
            }

            set
            {
                mCurrentLayer = GetLayer(value);
            }
        }

        private CadLayer mCurrentLayer;

        public CadLayer CurrentLayer
        {
            get
            {
                return mCurrentLayer;
            }

            set
            {
                mCurrentLayer = value;
            }
        }

        private Dictionary<uint, CadLayer> mLayerIdMap = new Dictionary<uint, CadLayer>();
        public Dictionary<uint, CadLayer> LayerMap
        {
            get
            {
                return mLayerIdMap;
            }

            set
            {
                mLayerIdMap = value;
            }
        }


        private IdProvider mLayerIdProvider = new IdProvider();
        public IdProvider LayerIdProvider
        {
            get
            {
                return mLayerIdProvider;
            }
        }


        private List<CadLayer> mLayerList = new List<CadLayer>();
        public List<CadLayer> LayerList
        {
            get
            {
                return mLayerList;
            }

            set
            {
                mLayerList = value;
            }
        }


        public CadLayer GetLayer(uint id)
        {
            if (id == 0)
            {
                return null;
            }

            return mLayerIdMap[id];
        }

        public CadLayer NewLayer()
        {
            CadLayer layer = new CadLayer();
            AddLayer(layer);
            return layer;
        }

        public uint AddLayer(CadLayer layer)
        {
            layer.ID = mLayerIdProvider.getNew();
            mLayerIdMap.Add(layer.ID, layer);
            return layer.ID;
        }

        public void RemoveLayer(uint id)
        {
            mLayerIdMap.Remove(id);
            mLayerList.RemoveAll(a => a.ID == id);
        }

        public int LayerIndex(uint id)
        {
            int idx = 0;
            foreach (CadLayer layer in mLayerList)
            {
                if (layer.ID == id)
                {
                    return idx;
                }

                idx++;
            }

            return -1;
        }
        
        #endregion



        #region "Manage Figure"
        private Dictionary<uint, CadFigure> mFigureIdMap = new Dictionary<uint, CadFigure>();
        public Dictionary<uint, CadFigure> FigureMap
        {
            get
            {
                return mFigureIdMap;
            }

            set
            {
                mFigureIdMap = value;
            }
        }

        IdProvider mFigIdProvider = new IdProvider();
        public IdProvider FigIdProvider
        {
            get
            {
                return mFigIdProvider;
            }
        }


        public CadFigure GetFigure(uint id)
        {
            if (id == 0)
            {
                return null;
            }

            return mFigureIdMap[id];
        }

        public CadFigure NewFigure(CadFigure.Types type)
        {
            CadFigure fig = CadFigure.Create(type);

            AddFigure(fig);
            return fig;
        }

        public uint AddFigure(CadFigure fig)
        {
            fig.ID = mFigIdProvider.getNew();
            mFigureIdMap.Add(fig.ID, fig);

            return fig.ID;
        }

        public void RelaseFigure(uint id)
        {
            mFigureIdMap.Remove(id);
        }

        #endregion Manage Figure


        
        #region Walk

        public void Walk(Action<CadLayer, CadFigure> walk, Func<CadLayer, bool> layerFilter)
        {
            mLayerList.ForEach(layer =>
            {
                if (layerFilter != null && !layerFilter(layer))
                {
                    return;
                }

                layer.ForEachFig(fig =>
                {
                    walk(layer, fig);
                });
            });
        }

        public static Func<CadLayer, bool> EditableLayerFilter = (layer) =>
        {
            if (layer.Locked) return false;
            if (!layer.Visible) return false;

            return true;
        };

        public void WalkEditable(Action<CadLayer, CadFigure> walk)
        {
            mLayerList.ForEach(layer =>
            {
                if (!EditableLayerFilter(layer))
                {
                    return;
                }

                layer.ForEachFig(fig =>
                {
                    walk(layer, fig);
                });
            });
        }

        #endregion Walk

        #region Query
        public List<uint> GetSelectedFigIDList()
        {
            List<uint> idList = new List<uint>();

            foreach (CadLayer layer in LayerList)
            {
                layer.ForEachFig(fig =>
                {
                    if (fig.HasSelectedPoint())
                    {
                        idList.Add(fig.ID);
                    }
                });
            }
            return idList;
        }

        public List<CadFigure> GetSelectedFigList()
        {
            List<CadFigure> list = new List<CadFigure>();

            HashSet<CadFigure> fset = new HashSet<CadFigure>();


            foreach (CadLayer layer in LayerList)
            {
                layer.ForEachFig(fig =>
                {
                    if (fig.HasSelectedPoint())
                    {
                        fset.Add(fig);
                        if (fig.Parent != null)
                        {
                            fset.Add(fig.Parent);
                        }
                    }
                });
            }

            list.AddRange(fset);

            return list;
        }
        #endregion Query

        public void ClearAll()
        {
            LayerMap.Clear();
            mLayerIdProvider.Reset();
            LayerList.Clear();
            FigureMap.Clear();
            FigIdProvider.Reset();

            CadLayer layer = NewLayer();

            LayerList.Add(layer);

            CurrentLayerID = layer.ID;
            CurrentLayer = layer;
        }

        #region "For debug"
        public void dump()
        {
            DbgOut.pln(this.GetType().Name + "(" + this.GetHashCode().ToString() + ") {");
            DbgOut.Indent++;

            {
                List<uint> ids = new List<uint>(mLayerIdMap.Keys);

                DbgOut.pln("Layer map {");
                DbgOut.Indent++;
                foreach (uint id in ids)
                {
                    CadLayer layer = mLayerIdMap[id];
                    layer.sdump();
                }
                DbgOut.Indent--;
                DbgOut.pln("}");
            }

            {
                DbgOut.pln("Layer list {");
                DbgOut.Indent++;
                foreach (CadLayer layer in mLayerList)
                {
                    layer.sdump();
                }
                DbgOut.Indent--;
                DbgOut.pln("}");
            }

            dumpFigureMap();

            DbgOut.Indent--;
            DbgOut.pln("}");
        }

        public void dumpFigureMap()
        {
            List<uint> ids = new List<uint>(mFigureIdMap.Keys);

            DbgOut.pln("Figure map {");
            DbgOut.Indent++;
            foreach (uint id in ids)
            {
                CadFigure fig = mFigureIdMap[id];
                fig.Dump("fig");
            }
            DbgOut.Indent--;
            DbgOut.pln("}");
        }

        #endregion
    }

    //public static class DUtil
    //{
    //    public static Dictionary<uint, T> ListToDict<T>(List<T> list)
    //    {
    //        Dictionary<uint, T> dict = new Dictionary<uint, T>();

    //        foreach (T item in list)
    //        {
    //            dynamic d = item;
    //            dict.Add(d.ID, item);
    //        }

    //        return dict;
    //    }

    //    public static List<T> IdListToObjList<T>(List<uint> list, Dictionary<uint, T> dict)
    //    {
    //        var objList = new List<T>();

    //        foreach (uint id in list)
    //        {
    //            T obj = dict[id];

    //            if (obj == null) continue;

    //            objList.Add(obj);
    //        }

    //        return objList;
    //    }
    //}
}