using System;
using System.Runtime.InteropServices;

namespace FTGL
{
	public enum RenderMode : int
	{
		Front = 0x0001,
		Back  = 0x0002,
		Side  = 0x0004,
		All   = 0xffff
	}

	public enum TextAlignment : int
	{
		Left    = 0,
		Center  = 1,
		Right   = 2,
		Justify = 3
	}

	public class FTGLException : Exception
	{
		public FTGLException(string msg) : base(msg) {}

		public FTGLException(IntPtr font) : base(string.Format("ftgl Exception: {0}", FtglAPI.GetFontError(font))) {

		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct FontBoundaries
	{
		public float Lower;
		public float Left;
		public float Near;

		public float Upper;
		public float Right;
		public float Far;
	}

	public static class FtglAPI
	{
		private const string nativeLibName = "ftgl.dll";

		[DllImport(nativeLibName, EntryPoint = "ftglCreatePixmapFont", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr CreatePixmapFont([In] [MarshalAs(UnmanagedType.LPStr)] string pathToTtf);
	
		[DllImport(nativeLibName, EntryPoint = "ftglCreateTextureFont", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr CreateTextureFont([In] [MarshalAs(UnmanagedType.LPStr)] string pathToTtf);


		[DllImport(nativeLibName, EntryPoint = "ftglDestroyFont", CallingConvention = CallingConvention.Cdecl)]
		public static extern void DestroyFont(IntPtr font);


		[DllImport(nativeLibName, EntryPoint = "ftglGetFontFaceSize", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint GetFontFaceSize(IntPtr font);

		[DllImport(nativeLibName, EntryPoint = "ftglGetFontLineHeight", CallingConvention = CallingConvention.Cdecl)]
		public static extern float GetFontLineHeight(IntPtr font);

		[DllImport(nativeLibName, EntryPoint = "ftglRenderFont", CallingConvention = CallingConvention.Cdecl)]
		public static extern void RenderFont(IntPtr font,[In] [MarshalAs(UnmanagedType.LPStr)] string text, RenderMode mode);


        [DllImport(nativeLibName, EntryPoint = "ftglRenderFontW", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RenderFontW(IntPtr font, [In] [MarshalAs(UnmanagedType.LPWStr)] string text, RenderMode mode);


        [DllImport(nativeLibName, EntryPoint = "ftglSetFontFaceSize", CallingConvention = CallingConvention.Cdecl)]
		public static extern int SetFontFaceSize(IntPtr font, uint faceSize, uint deviceResolution=0);


		public static FontBoundaries GetFontBoundaryBox(IntPtr font, string text)
		{
			var bbox=new FontBoundaries();
			if(text != null)
				GetFontBoundaryBox (font, text, text.Length, ref bbox);
			return bbox;
		}

		[DllImport(nativeLibName, EntryPoint = "ftglGetFontBBox", CallingConvention = CallingConvention.Cdecl)]
		public static extern void GetFontBoundaryBox(IntPtr font, 
			[MarshalAs(UnmanagedType.LPStr)] string text, int textLength,
			[MarshalAs(UnmanagedType.Struct)] ref FontBoundaries boundaries
		);

		[DllImport(nativeLibName, EntryPoint = "ftglGetFontBBox", CallingConvention = CallingConvention.Cdecl)]
		public static extern void GetFontBoundaryBox(IntPtr font, 
			[In] [MarshalAs(UnmanagedType.LPStr)] string text, int textLength, 
			[In] [MarshalAs(UnmanagedType.LPArray, SizeConst = 6)] float[] boundaries
		);

		[DllImport(nativeLibName, EntryPoint = "ftglGetFontAdvance", CallingConvention = CallingConvention.Cdecl)]
		public static extern float GetFontAdvance(IntPtr font, [In] [MarshalAs(UnmanagedType.LPStr)] string text);

		[DllImport(nativeLibName, EntryPoint = "ftglSetFontOutset", CallingConvention = CallingConvention.Cdecl)]
		public static extern void SetFontOutset(IntPtr font, float frontOutsetDistance, float backOutsetDistance);

		[DllImport(nativeLibName, EntryPoint = "ftglGetFontError", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr GetFontError(IntPtr font);

        [DllImport(nativeLibName, EntryPoint = "ftglSetFontCharMap", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetFontCharMap(IntPtr font, uint encoding);
    }
}

