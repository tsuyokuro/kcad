using System;
using System.Collections.Generic;
using SharpFont;
using OpenTK.Graphics.OpenGL;
using System.Windows.Resources;
using System.Windows;
using System.IO;
using OpenTK;

namespace GLFont
{
    public class FontTex
    {
        public byte[] Data = null;
        public int W = 0;
        public int H = 0;

        public int ImgW = 0;
        public int ImgH = 0;

        public int PosX = 0;
        public int PosY = 0;

        public int FontW = 0;
        public int FontH = 0;

        public bool IsSpace
        {
            get => Data == null;
        }

        public FontTex() { }

        public FontTex(int imgW, int imgH)
        {
            ImgW = imgW;
            ImgH = imgH;

            W = ((imgW + 3) / 4) * 4;
            H = imgH;

            Data = new byte[W * H];
        }

        public void Set(int x, int y, byte v)
        {
            int i = ((H - 1 - y) * W) + x;
            Data[i] = v;
        }

        public static FontTex CreateSpace(int w, int h)
        {
            FontTex ft = new FontTex();
            ft.ImgW = w;
            ft.ImgH = h;
            return ft;
        }

        public static FontTex Merge(FontTex[] ta)
        {
            int fw = 0;
            int fh = 0;

            foreach (FontTex f in ta)
            {
                fw += f.ImgW;
                if (f.ImgH > fh)
                {
                    fh = f.ImgH;
                }
            }

            FontTex ft = new FontTex(fw, fh);

            int fx = 0;
            int fy = 0;

            foreach (FontTex f in ta)
            {
                ft.Paste(fx, fy, f);
                fx += f.ImgW;
            }

            return ft;
        }

        public void Paste(int x, int y, FontTex src)
        {
            if (src.IsSpace)
            {
                return;
            }

            int sx = 0;
            int sy = 0;
            int sw = src.ImgW;
            int sh = src.ImgH;

            if (x < 0)
            {
                sx += -x;
                x = 0;
            }

            if (x + sw > W)
            {
                sw = W - x;
            }

            if (y < 0)
            {
                sy += -y;
                y = 0;
            }

            if (y + sh > H)
            {
                sh = H - y;
            }

            int dx = x;
            int dy = y;

            int cx = sx;

            for (; sy < sh; sy++)
            {
                dx = x;
                sx = cx;
                for (; sx < sw; sx++)
                {
                    int si = ((src.H - 1 - sy) * src.W) + sx;
                    int di = ((H - 1 - dy) * W) + dx;

                    Data[di] = src.Data[si];

                    dx++;
                }
                dy++;
            }
        }

        public static unsafe FontTex Create(FTBitmap ftb)
        {
            byte* buffer = (byte*)ftb.Buffer;
            int sw = ftb.Width;
            int sh = ftb.Rows;

            FontTex ft = new FontTex(ftb.Width, ftb.Rows);
            int dw = ft.W;
            int dh = ft.H;

            byte[] data = ft.Data;

            int si;
            int di;

            int dy = 0;

            if (ftb.GrayLevels == 2)
            {
                int sbw = ((sw + 7) / 8) * 8;

                for (int sy = sh - 1; sy >= 0;)
                {
                    si = sy * sbw;
                    di = dy * dw;

                    for (int x = 0; x < ftb.Width; x++)
                    {
                        data[di + x] = (byte)(BitUtil.GetAt(buffer, si + x) * 255);
                    }

                    sy--;
                    dy++;
                }
            }
            else
            {
                for (int sy = sh - 1; sy >= 0;)
                {
                    si = sy * sw;
                    di = dy * dw;

                    for (int x = 0; x < sw; x++)
                    {
                        data[di + x] = buffer[si + x];
                    }

                    sy--;
                    dy++;
                }
            }

            return ft;
        }

        public void dump()
        {
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    int i = y * W + x;
                    byte v = Data[i];
                    Console.Write(v.ToString("x2") + " ");
                }
                Console.WriteLine();
            }
        }

        public void dump_b()
        {
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    int i = y * W + x;
                    byte v = Data[i];
                    if (v != 0)
                    {
                        Console.Write("@");
                    }
                    else
                    {
                        Console.Write(".");
                    }
                }
                Console.WriteLine();
            }
        }

        public unsafe struct BitUtil
        {
            public static int GetAt(byte* p, int idx)
            {
                int di = idx / 8;
                int bp = idx - (di * 8);

                return (p[di] >> (7 - bp)) & 0x01;
            }

            public static int GetAt(byte[] p, int idx)
            {
                int di = idx / 8;
                int bp = idx - (di * 8);

                return (p[di] >> (7 - bp)) & 0x01;
            }
        }
    }

    public class FontFaceW
    {
        private Library mLib;

        private Face FontFace;

        private float Size;

        private Dictionary<char, FontTex> Cache = new Dictionary<char, FontTex>();

        public FontFaceW()
        {
            mLib = new Library();
            Size = 8.25f;
        }

        public void SetFont(string filename, int face_index = 0)
        {
            FontFace = new Face(mLib, filename, face_index);
            SetSize(this.Size);

            Cache.Clear();
        }

        public void SetSize(float size)
        {
            Size = size;
            if (FontFace != null)
            {
                FontFace.SetCharSize(0, size, 0, 96);
            }

            Cache.Clear();
        }

        public FontTex CreateTexture(char c)
        {
            FontTex ft;

            if (Cache.TryGetValue(c, out ft))
            {
                return ft;
            }

            uint glyphIndex = FontFace.GetCharIndex(c);
            FontFace.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
            FontFace.Glyph.RenderGlyph(RenderMode.Light);
            FTBitmap ftbmp = FontFace.Glyph.Bitmap;

            int fontW = (int)((float)FontFace.Glyph.Metrics.HorizontalAdvance);
            int fontH = (int)((float)FontFace.Glyph.Metrics.VerticalAdvance);

            if (ftbmp.Width > 0 && ftbmp.Rows > 0)
            {
                float top = (float)FontFace.Size.Metrics.Ascender;
                int y = (int)(top - (float)FontFace.Glyph.Metrics.HorizontalBearingY);

                ft = FontTex.Create(ftbmp);

                ft.PosX = (int)((float)FontFace.Glyph.Metrics.HorizontalBearingX);
                if (ft.PosX < 0) { ft.PosX = 0; };
                ft.PosY = y;
                ft.FontW = Math.Max(fontW, ft.ImgW);
                ft.FontH = fontH;
            }
            else
            {
                ft = FontTex.CreateSpace((int)FontFace.Glyph.Advance.X, (int)FontFace.Glyph.Advance.Y);
                ft.FontW = fontW;
                ft.FontH = fontH;
            }

            Cache.Add(c, ft);

            //ft.dump_b();
            //Console.WriteLine();

            return ft;
        }

        public FontTex CreateTexture(string s)
        {
            List<FontTex> ta = new List<FontTex>();

            int fw = 0;
            int fh = 0;

            foreach (char c in s)
            {
                FontTex ft = CreateTexture(c);

                fw += ft.FontW;
                if (ft.FontH > fh)
                {
                    fh = ft.FontH;
                }

                ta.Add(ft);
            }

            FontTex mft = new FontTex(fw, fh + 1);

            int x = 0;
            int y = 0;

            foreach (FontTex ft in ta)
            {
                mft.Paste(x + ft.PosX, y + ft.PosY, ft);
                x += ft.FontW;
            }

            //mft.dump_b();

            return mft;
        }
    }

    public class FontRenderer
    {
        public static string VertexShaderSrc =
@"
void main(void)
{
	gl_FrontColor = gl_Color;
	gl_TexCoord[0] = gl_MultiTexCoord0;
	gl_Position = ftransform();
}
";

        public static string FragmentShaderSrc =
@"
uniform sampler2D tex;

void main()
{
	vec4 a = texture2D(tex, gl_TexCoord[0].st);
	vec4 color = gl_Color;
	color[3] = color[3] * a[3];
	gl_FragColor = color;
}
";

        public int Texture = -1;
        public int FontShaderProgram = -1;

        private bool mInitialized = false;

        public bool Initialized
        {
            get => mInitialized;
        }

        public void Init()
        {
            Dispose();

            Texture = GL.GenTexture();

            SetupFontShader();

            mInitialized = true;
        }

        public void Dispose()
        {
            if (mInitialized)
            {
                GL.DeleteTexture(Texture);
                GL.DeleteProgram(FontShaderProgram);
            }

            mInitialized = false;
        }

        private void SetupFontShader()
        {
            string vertexSrc = VertexShaderSrc;
            string fragmentSrc = FragmentShaderSrc;

            int status;

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexSrc);
            GL.CompileShader(vertexShader);
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out status);
            if (status == 0)
            {
                throw new ApplicationException(GL.GetShaderInfoLog(vertexShader));
            }

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentSrc);
            GL.CompileShader(fragmentShader);
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out status);
            if (status == 0)
            {
                throw new ApplicationException(GL.GetShaderInfoLog(fragmentShader));
            }

            int shaderProgram = GL.CreateProgram();

            //各シェーダオブジェクトをシェーダプログラムへ登録
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);

            //不要になった各シェーダオブジェクトを削除
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            //シェーダプログラムのリンク
            GL.LinkProgram(shaderProgram);

            GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out status);

            //シェーダプログラムのリンクのチェック
            if (status == 0)
            {
                throw new ApplicationException(GL.GetProgramInfoLog(shaderProgram));
            }

            FontShaderProgram = shaderProgram;
        }

        public void Render(FontTex tex)
        {
            Vector3d p = Vector3d.Zero;
            Vector3d xv = Vector3d.UnitX * tex.ImgW;
            Vector3d yv = Vector3d.UnitY * tex.ImgH;

            Render(tex, p, xv, yv);
        }

        public void Render(FontTex tex, Vector3d p, Vector3d xv, Vector3d yv)
        {
            GL.ActiveTexture(TextureUnit.Texture0);

            GL.BindTexture(TextureTarget.Texture2D, Texture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexImage2D(
                TextureTarget.Texture2D, 0,
                PixelInternalFormat.Alpha8,
                tex.W, tex.H, 0,
                PixelFormat.Alpha,
                PixelType.UnsignedByte, tex.Data);


            GL.UseProgram(FontShaderProgram);

            GL.TexCoord2(1.0, 1.0);

            int texLoc = GL.GetUniformLocation(FontShaderProgram, "tex");

            GL.Uniform1(texLoc, 0);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Normal3(new Vector3d(0, 0, 1));

            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(1.0, 1.0);
            GL.Vertex3(p + xv + yv);

            GL.TexCoord2(0.0, 1.0);
            GL.Vertex3(p + yv);

            GL.TexCoord2(0.0, 0.0);
            GL.Vertex3(p);

            GL.TexCoord2(1.0, 0.0);
            GL.Vertex3(p + xv);

            GL.End();

            GL.UseProgram(0);
        }
    }
}
