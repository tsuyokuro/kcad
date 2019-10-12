﻿using OpenTK;
using Plotter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TestApp
{
    public class RingBuffer<T>
    {
        private T[] Data;

        private int Top = 0;

        private int Bottom = 0;

        private int Mask;

        public T this[int i]
        {
            get
            {
                return Data[(i + Top) & Mask];
            }
        }

        public int Count
        {
            get;
            private set;
        }

        public int BufferSize
        {
            get;
            private set;
        }

        public RingBuffer(int size)
        {
            CreateBuffer(size);
        }

        public RingBuffer()
        {
        }

        private void ShallowCopyFrom(RingBuffer<T> src)
        {
            BufferSize = src.BufferSize;
            Top = src.Top;
            Bottom = src.Bottom;
            Mask = src.Mask;
            Count = src.Count;
            Data = src.Data;
        }

        private void DeepCopyFrom(RingBuffer<T> src)
        {
            BufferSize = src.BufferSize;
            Top = src.Top;
            Bottom = src.Bottom;
            Mask = src.Mask;
            Count = src.Count;

            Data = new T[src.BufferSize];
            Array.Copy(src.Data, Data, src.Data.Length);
        }

        public void CreateBuffer(int size)
        {
            BufferSize = Pow2((uint)size);
            Data = new T[BufferSize];
            Mask = BufferSize - 1;
        }

        public void ResizeBuffer(int size)
        {
            RingBuffer<T> tmp = new RingBuffer<T>();
            tmp.ShallowCopyFrom(this);

            CreateBuffer(size);
            Clear();

            for (int i = 0; i < tmp.Count; i++)
            {
                Add(tmp[i]);
            }
        }

        public void Clear()
        {
            Top = 0;
            Bottom = 0;
            Count = 0;
        }

        static int Pow2(uint n)
        {
            --n;
            int p = 0;
            for (; n != 0; n >>= 1)
            {
                p = (p << 1) + 1;
            }

            return p + 1;
        }

        public void Add(T elem)
        {
            Data[Bottom] = elem;
            Bottom = (Bottom + 1) & Mask;

            if (Count < BufferSize)
            {
                Count++;
            }
            else
            {
                Top = (Top + 1) & Mask;
            }
        }
    }

    class Program
    {
        static void XmlTest()
        {
            XDocument doc = new XDocument();

            XDocumentType docType = new XDocumentType(
                @"svg", @" -//W3C//DTD SVG 1.1//EN",
                @"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd",
                null);


            string pd = "M 100 100 L 300 100 L 200 300 z";

            XElement fig = new XElement("path", new XAttribute("d", pd));

            doc.Add(docType, fig);


            Console.WriteLine(doc.ToString());

            Console.WriteLine("end");

            Console.ReadLine();

        }

        static void Main(string[] args)
        {
            RingBuffer<int> rb = new RingBuffer<int>(16);

            for (int i=0; i<30; i++)
            {
                rb.Add(i);
            }

            for (int i=0; i<rb.Count; i++)
            {
                Console.WriteLine($"rb[{i}]:{rb[i]}");
            }

            Console.WriteLine("=============");

            rb.ResizeBuffer(8);

            for (int i = 0; i < rb.Count; i++)
            {
                Console.WriteLine($"rb[{i}]:{rb[i]}");
            }

            Console.ReadLine();
        }
    }
}
