﻿using Plotter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KCad
{
    public struct TextAttr
    {
        public int FColor;
        public int BColor;
    }

    public struct AttrSpan
    {
        public TextAttr Attr;
        public int Len;

        public AttrSpan(TextAttr attr, int len)
        {
            Attr = attr;
            Len = len;
        }
    }

    public class TextLine
    {
        public bool IsSelected = false;
        public string Data = "";
        public List<AttrSpan> Attrs = new List<AttrSpan>();

        private AttrSpan LastAttrSpan
        {
            get
            {
                return Attrs[Attrs.Count - 1];
            }

            set
            {
                Attrs[Attrs.Count - 1] = value;
            }
        }

        public TextLine(TextAttr attr)
        {
            Attrs.Add(new AttrSpan(attr, 0));
        }

        public void Clear()
        {
            TextAttr lastAttr = LastAttrSpan.Attr;
            Attrs.Clear();
            AppendAttr(lastAttr);
            Data = "";
        }

        public void AppendAttr(TextAttr attr)
        {
            Attrs.Add(new AttrSpan(attr, 0));
        }

        private void AddLastAttrSpanLen(int len)
        {
            AttrSpan attrItem = LastAttrSpan;
            attrItem.Len += len;
            LastAttrSpan = attrItem;
        }

        private void AddAttrSpanLen(int idx, int len)
        {
            AttrSpan attrItem = Attrs[idx];
            attrItem.Len += len;
            Attrs[idx] = attrItem;
        }

        public void Parse(string s)
        {
            TextAttr attr = LastAttrSpan.Attr;

            StringBuilder sb = new StringBuilder();

            int blen = 0;

            int state = 0;

            int x = 0;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\x1b')
                {
                    state = 1;

                    AddLastAttrSpanLen(blen);

                    blen = 0;
                    continue;
                }

                switch (state)
                {
                    case 0:
                        if (s[i] == '\r')
                        {
                            Clear();
                            blen = 0;
                            sb.Clear();
                        }
                        else
                        {
                            sb.Append(s[i]);
                            blen++;
                        }
                        break;
                    case 1:
                        if (s[i] == '[')
                        {
                            state = 2;
                        }
                        break;
                    case 2:
                        if (s[i] >= '0' && s[i] <= '9')
                        {
                            state = 3;
                            x = s[i] - '0';
                        }
                        else if (s[i] == 'm')
                        {
                            if (x == 0)
                            {
                                attr.BColor = 0;
                                attr.FColor = 7;
                            }

                            AppendAttr(attr);

                            blen = 0;
                            state = 0;
                        }
                        else
                        {
                            sb.Append(s[i]);
                            blen++;
                            state = 0;
                        }
                        break;
                    case 3:
                        if (s[i] >= '0' && s[i] <= '9')
                        {
                            x = x * 10 + (s[i] - '0');
                        }
                        else if (s[i] == 'm')
                        {
                            if (x == 0)
                            {
                                attr.BColor = 0;
                                attr.FColor = 7;
                            }
                            else if (x >= 30 && x <= 37) // front std
                            {
                                attr.FColor = (byte)(x - 30);
                            }
                            else if (x >= 40 && x <= 47) // back std
                            {
                                attr.BColor = (byte)(x - 40);
                            }
                            else if (x >= 90 && x <= 97) // front strong
                            {
                                attr.FColor = (byte)(x - 90 + 8);
                            }
                            else if (x >= 100 && x <= 107) // back std
                            {
                                attr.BColor = (byte)(x - 100 + 8);
                            }

                            AppendAttr(attr);
                            blen = 0;
                            state = 0;
                        }
                        else
                        {
                            sb.Append(s[i]);
                            blen++;
                            state = 0;
                        }

                        break;
                }
            }

            if (blen > 0)
            {
                AddLastAttrSpanLen(blen);
                blen = 0;
            }

            Data += sb.ToString();
        }
    }
}