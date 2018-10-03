﻿using KCad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KCad.CadObjectTreeView;
using CadDataTypes;
using System.Windows.Media;

namespace Plotter
{
    class CadFigTreeItem : CadObjTreeItem
    {
        public CadFigure Fig;

        public override bool IsChecked
        {
            get
            {
                return HasSelectedPoint();
            }

            set
            {
                SelectAllPoints(value);
            }
        }

        public override string Text
        {
            get
            {
                return CadFigure.TypeName(Fig.Type) + " ID:"+ Fig.ID.ToString();
            }
        }

        public CadFigTreeItem(CadFigure fig)
        {
            Fig = fig;

            int idx = 0;

            for (int i=0; i < Fig.PointList.Count; i++) {
                CadVector p = Fig.PointList[i];
                CadPointTreeItem pi = new CadPointTreeItem(fig, idx);
                Add(pi);
                idx++;
            }

            for (int i=0; i<Fig.ChildList.Count; i++)
            {
                CadFigure c = Fig.ChildList[i];
                CadFigTreeItem pi = new CadFigTreeItem(c);
                Add(pi);
            }
        }

        private void SelectAllPoints(bool sel)
        {
            if (Children == null)
            {
                return;
            }

            for (int i=0; i<Children.Count; i++)
            {
                CadObjTreeItem c = Children[i];
                c.IsChecked = sel;
            }
        }

        private bool HasSelectedPoint()
        {
            bool ret = false;

            if (Children == null)
            {
                return ret;
            }

            int i;
            for (i=0; i<Children.Count;i++)
            {
                CadObjTreeItem c = Children[i];

                if (c.IsChecked)
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }
    }
}