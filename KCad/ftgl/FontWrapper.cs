using System;
using System.IO;
using System.Collections.Generic;

namespace FTGL
{
	public enum FontKind
	{
		Pixmap,
		Texture
	}

    public class FTEncord
    {
        public static uint UNICODE = EncodeVal('u', 'n', 'i', 'c');
        public static uint SJIS = EncodeVal('s', 'j', 'i', 's');

        private static uint EncodeVal(uint a, uint b, uint c, uint d)
        {
            return (a << 24) | (b << 16) | (c << 8) | d;
        }
    }


    public class FontWrapper : IDisposable
	{
		class FontHolder
		{
			public readonly IntPtr pFont;
			public int ReferenceCount;

			public FontHolder(IntPtr pftglFont)
			{
				pFont = pftglFont;
				ReferenceCount = 1;
			}
		}

		static Dictionary<int, FontHolder> FontCache = new Dictionary<int, FontHolder>();
		static Dictionary<int, uint> LastSetSizes = new Dictionary<int, uint>();

		readonly FontKind Kind;

		readonly int Hash;

        public override int GetHashCode (){return Hash;}

        readonly IntPtr pFTGLFont;

        readonly bool Disp;

        uint RenderSize;

        public uint FontSize
		{
			get
            { 
				return RenderSize != 0 ? RenderSize : FtglAPI.GetFontFaceSize(pFTGLFont);
			}

            set
            {
				RenderSize= value;
			}
		}


		void SetRenderSize()
		{
			if (LastSetSizes[Hash] != RenderSize)
            {
				FtglAPI.SetFontFaceSize(pFTGLFont, RenderSize);
				LastSetSizes[Hash] = RenderSize;
			}
		}

		public float LineHeight
		{
			get
            {
				SetRenderSize();
				return FtglAPI.GetFontLineHeight (pFTGLFont);
			}
		}

		public float GetAdvance(string text)
		{
			SetRenderSize ();
			return FtglAPI.GetFontAdvance (pFTGLFont, text);
		}

		public FontBoundaries GetBoundaries(string text)
		{
			SetRenderSize ();
			return FtglAPI.GetFontBoundaryBox (pFTGLFont, text);
		}

		public void Render(string text, RenderMode mode = RenderMode.Front)
		{
			if (text != null) {
				SetRenderSize ();
                FtglAPI.RenderFont(pFTGLFont, text, mode);
			}
		}

        public void RenderW(string text, RenderMode mode = RenderMode.Front)
        {
            if (text != null)
            {
                SetRenderSize();
                FtglAPI.RenderFontW(pFTGLFont, text, mode);
            }
        }

        public void SetCharMap(uint encoding)
        {
            FtglAPI.SetFontCharMap(pFTGLFont, encoding);
        }

		private static int CalcHash(string fontPathName, FontKind kind)
		{
			return fontPathName.GetHashCode() + ((byte)kind << 24);
		}

		public static FontWrapper LoadFile(string pathToFont, FontKind kind = FontKind.Texture)
		{
			if (!File.Exists (pathToFont))
				throw new FileNotFoundException (pathToFont + " could not be found!");

			var hash = CalcHash(pathToFont, kind);

            FontHolder holder;

			if (FontCache.TryGetValue (hash, out holder)) {
				holder.ReferenceCount++;
				return new FontWrapper(hash,holder.pFont, kind);
			}

			IntPtr ftglFont;

			switch (kind) {
				case FontKind.Pixmap:
					ftglFont = FtglAPI.CreatePixmapFont(pathToFont);
					break;
				default:
				case FontKind.Texture:
					ftglFont = FtglAPI.CreateTextureFont(pathToFont);
					break;
			}

            if (ftglFont == IntPtr.Zero)
            {
                throw new FTGLException(IntPtr.Zero);
            }

			FontCache[hash] = new FontHolder(ftglFont);

            return new FontWrapper (hash, ftglFont, kind);
		}

		private FontWrapper(int hash,IntPtr font, FontKind kind)
		{
			if (!LastSetSizes.ContainsKey (hash))
				LastSetSizes [hash] = 0;

			this.Kind = kind;
			this.Hash = hash;
			this.Disp = kind != FontKind.Texture;
			this.pFTGLFont = font;
		}

		~FontWrapper()
		{
			Dispose ();
		}

		bool disposed=false;
		public virtual void Dispose ()
		{
			if (!disposed) {
				disposed = true;
				var fe = FontCache [Hash];
				if (--fe.ReferenceCount <= 0) {
					if(Disp)
						FtglAPI.DestroyFont(pFTGLFont);
					FontCache.Remove (Hash);
					LastSetSizes.Remove (Hash);
				}
			}
		}
	}
}
