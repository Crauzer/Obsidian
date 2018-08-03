using System.Runtime.InteropServices;

namespace Imaging.DDSReader.Utils
{
	public class Helper
	{
		#region Constants

		// DDSStruct flags
		public const int DDSD_CAPS = 0x00000001;

		public const int DDSD_HEIGHT = 0x00000002;
		public const int DDSD_WIDTH = 0x00000004;
		public const int DDSD_PITCH = 0x00000008;
		public const int DDSD_PIXELFORMAT = 0x00001000;
		public const int DDSD_MIPMAPCOUNT = 0x00020000;
		public const int DDSD_LINEARSIZE = 0x00080000;
		public const int DDSD_DEPTH = 0x00800000;

		// PixelFormat values
		public const int DDPF_ALPHAPIXELS = 0x00000001;

		public const int DDPF_FOURCC = 0x00000004;
		public const int DDPF_RGB = 0x00000040;
		public const int DDPF_LUMINANCE = 0x00020000;

		// DDSCaps
		public const int DDSCAPS_COMPLEX = 0x00000008;

		public const int DDSCAPS_TEXTURE = 0x00001000;
		public const int DDSCAPS_MIPMAP = 0x00400000;
		public const int DDSCAPS2_CUBEMAP = 0x00000200;
		public const int DDSCAPS2_CUBEMAP_POSITIVEX = 0x00000400;
		public const int DDSCAPS2_CUBEMAP_NEGATIVEX = 0x00000800;
		public const int DDSCAPS2_CUBEMAP_POSITIVEY = 0x00001000;
		public const int DDSCAPS2_CUBEMAP_NEGATIVEY = 0x00002000;
		public const int DDSCAPS2_CUBEMAP_POSITIVEZ = 0x00004000;
		public const int DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x00008000;
		public const int DDSCAPS2_VOLUME = 0x00200000;

		// FOURCC
		public const uint FOURCC_DXT1 = 0x31545844;

		public const uint FOURCC_DXT2 = 0x32545844;
		public const uint FOURCC_DXT3 = 0x33545844;
		public const uint FOURCC_DXT4 = 0x34545844;
		public const uint FOURCC_DXT5 = 0x35545844;
		public const uint FOURCC_ATI1 = 0x31495441;
		public const uint FOURCC_ATI2 = 0x32495441;
		public const uint FOURCC_RXGB = 0x42475852;
		public const uint FOURCC_DOLLARNULL = 0x24;
		public const uint FOURCC_oNULL = 0x6f;
		public const uint FOURCC_pNULL = 0x70;
		public const uint FOURCC_qNULL = 0x71;
		public const uint FOURCC_rNULL = 0x72;
		public const uint FOURCC_sNULL = 0x73;
		public const uint FOURCC_tNULL = 0x74;

		#endregion Constants

		// iCompFormatToBpp
		internal static uint PixelFormatToBpp(PixelFormat pf, uint rgbbitcount)
		{
			switch (pf)
			{
				case PixelFormat.LUMINANCE:
				case PixelFormat.LUMINANCE_ALPHA:
				case PixelFormat.RGBA:
				case PixelFormat.RGB:
					return rgbbitcount / 8;

				case PixelFormat.THREEDC:
				case PixelFormat.RXGB:
					return 3;

				case PixelFormat.ATI1N:
					return 1;

				case PixelFormat.R16F:
					return 2;

				case PixelFormat.A16B16G16R16:
				case PixelFormat.A16B16G16R16F:
				case PixelFormat.G32R32F:
					return 8;

				case PixelFormat.A32B32G32R32F:
					return 16;

				default:
					return 4;
			}
		}

		// iCompFormatToBpc
		internal static uint PixelFormatToBpc(PixelFormat pf)
		{
			switch (pf)
			{
				case PixelFormat.R16F:
				case PixelFormat.G16R16F:
				case PixelFormat.A16B16G16R16F:
					return 4;

				case PixelFormat.R32F:
				case PixelFormat.G32R32F:
				case PixelFormat.A32B32G32R32F:
					return 4;

				case PixelFormat.A16B16G16R16:
					return 2;

				default:
					return 1;
			}
		}

		internal static bool Check16BitComponents(DDSStruct header)
		{
			if (header.pixelformat.rgbbitcount != 32)
				return false;
			// a2b10g10r10 format
			if (header.pixelformat.rbitmask == 0x3FF00000 && header.pixelformat.gbitmask == 0x000FFC00 && header.pixelformat.bbitmask == 0x000003FF
				&& header.pixelformat.alphabitmask == 0xC0000000)
				return true;
			// a2r10g10b10 format
			else if (header.pixelformat.rbitmask == 0x000003FF && header.pixelformat.gbitmask == 0x000FFC00 && header.pixelformat.bbitmask == 0x3FF00000
				&& header.pixelformat.alphabitmask == 0xC0000000)
				return true;

			return false;
		}

		internal static void CorrectPremult(uint pixnum, ref byte[] buffer)
		{
			for (uint i = 0; i < pixnum; i++)
			{
				byte alpha = buffer[i + 3];
				if (alpha == 0) continue;
				int red = (buffer[i] << 8) / alpha;
				int green = (buffer[i + 1] << 8) / alpha;
				int blue = (buffer[i + 2] << 8) / alpha;

				buffer[i] = (byte)red;
				buffer[i + 1] = (byte)green;
				buffer[i + 2] = (byte)blue;
			}
		}

		internal static void ComputeMaskParams(uint mask, ref int shift1, ref int mul, ref int shift2)
		{
			shift1 = 0; mul = 1; shift2 = 0;
			if (mask == 0 || mask == uint.MaxValue)
				return;
			while ((mask & 1) == 0)
			{
				mask >>= 1;
				shift1++;
			}
			uint bc = 0;
			while ((mask & (1 << (int)bc)) != 0) bc++;
			while ((mask * mul) < 255)
				mul = (mul << (int)bc) + 1;
			mask *= (uint)mul;

			while ((mask & ~0xff) != 0)
			{
				mask >>= 1;
				shift2++;
			}
		}

		internal static unsafe void DxtcReadColors(byte* data, ref Colour8888[] op)
		{
			byte r0, g0, b0, r1, g1, b1;

			b0 = (byte)(data[0] & 0x1F);
			g0 = (byte)(((data[0] & 0xE0) >> 5) | ((data[1] & 0x7) << 3));
			r0 = (byte)((data[1] & 0xF8) >> 3);

			b1 = (byte)(data[2] & 0x1F);
			g1 = (byte)(((data[2] & 0xE0) >> 5) | ((data[3] & 0x7) << 3));
			r1 = (byte)((data[3] & 0xF8) >> 3);

			op[0].red = (byte)(r0 << 3 | r0 >> 2);
			op[0].green = (byte)(g0 << 2 | g0 >> 3);
			op[0].blue = (byte)(b0 << 3 | b0 >> 2);

			op[1].red = (byte)(r1 << 3 | r1 >> 2);
			op[1].green = (byte)(g1 << 2 | g1 >> 3);
			op[1].blue = (byte)(b1 << 3 | b1 >> 2);
		}

		internal static void DxtcReadColor(ushort data, ref Colour8888 op)
		{
			byte r, g, b;

			b = (byte)(data & 0x1f);
			g = (byte)((data & 0x7E0) >> 5);
			r = (byte)((data & 0xF800) >> 11);

			op.red = (byte)(r << 3 | r >> 2);
			op.green = (byte)(g << 2 | g >> 3);
			op.blue = (byte)(b << 3 | r >> 2);
		}

		internal static unsafe void DxtcReadColors(byte* data, ref Colour565 color_0, ref Colour565 color_1)
		{
			color_0.blue = (byte)(data[0] & 0x1F);
			color_0.green = (byte)(((data[0] & 0xE0) >> 5) | ((data[1] & 0x7) << 3));
			color_0.red = (byte)((data[1] & 0xF8) >> 3);

			color_0.blue = (byte)(data[2] & 0x1F);
			color_0.green = (byte)(((data[2] & 0xE0) >> 5) | ((data[3] & 0x7) << 3));
			color_0.red = (byte)((data[3] & 0xF8) >> 3);
		}

		internal static void GetBitsFromMask(uint mask, ref uint shiftLeft, ref uint shiftRight)
		{
			uint temp, i;

			if (mask == 0)
			{
				shiftLeft = shiftRight = 0;
				return;
			}

			temp = mask;
			for (i = 0; i < 32; i++, temp >>= 1)
			{
				if ((temp & 1) != 0)
					break;
			}
			shiftRight = i;

			// Temp is preserved, so use it again:
			for (i = 0; i < 8; i++, temp >>= 1)
			{
				if ((temp & 1) == 0)
					break;
			}
			shiftLeft = 8 - i;
		}

		// This function simply counts how many contiguous bits are in the mask.
		internal static uint CountBitsFromMask(uint mask)
		{
			uint i, testBit = 0x01, count = 0;
			bool foundBit = false;

			for (i = 0; i < 32; i++, testBit <<= 1)
			{
				if ((mask & testBit) != 0)
				{
					if (!foundBit)
						foundBit = true;
					count++;
				}
				else if (foundBit)
					return count;
			}

			return count;
		}

		internal static uint HalfToFloat(ushort y)
		{
			int s = (y >> 15) & 0x00000001;
			int e = (y >> 10) & 0x0000001f;
			int m = y & 0x000003ff;

			if (e == 0)
			{
				if (m == 0)
				{
					//
					// Plus or minus zero
					//
					return (uint)(s << 31);
				}
				else
				{
					//
					// Denormalized number -- renormalize it
					//
					while ((m & 0x00000400) == 0)
					{
						m <<= 1;
						e -= 1;
					}

					e += 1;
					m &= ~0x00000400;
				}
			}
			else if (e == 31)
			{
				if (m == 0)
				{
					//
					// Positive or negative infinity
					//
					return (uint)((s << 31) | 0x7f800000);
				}
				else
				{
					//
					// Nan -- preserve sign and significand bits
					//
					return (uint)((s << 31) | 0x7f800000 | (m << 13));
				}
			}

			//
			// Normalized number
			//
			e = e + (127 - 15);
			m = m << 13;

			//
			// Assemble s, e and m.
			//
			return (uint)((s << 31) | (e << 23) | m);
		}

		internal static unsafe void ConvFloat16ToFloat32(uint* dest, ushort* src, uint size)
		{
			uint i;
			for (i = 0; i < size; ++i, ++dest, ++src)
			{
				//float: 1 sign bit, 8 exponent bits, 23 mantissa bits
				//half: 1 sign bit, 5 exponent bits, 10 mantissa bits
				*dest = HalfToFloat(*src);
			}
		}

		internal static unsafe void ConvG16R16ToFloat32(uint* dest, ushort* src, uint size)
		{
			uint i;
			for (i = 0; i < size; i += 3)
			{
				//float: 1 sign bit, 8 exponent bits, 23 mantissa bits
				//half: 1 sign bit, 5 exponent bits, 10 mantissa bits
				*dest++ = HalfToFloat(*src++);
				*dest++ = HalfToFloat(*src++);
				*((float*)dest++) = 1.0f;
			}
		}

		internal static unsafe void ConvR16ToFloat32(uint* dest, ushort* src, uint size)
		{
			uint i;
			for (i = 0; i < size; i += 3)
			{
				//float: 1 sign bit, 8 exponent bits, 23 mantissa bits
				//half: 1 sign bit, 5 exponent bits, 10 mantissa bits
				*dest++ = HalfToFloat(*src++);
				*((float*)dest++) = 1.0f;
				*((float*)dest++) = 1.0f;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Colour8888
	{
		public byte red;
		public byte green;
		public byte blue;
		public byte alpha;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct Colour565
	{
		public ushort blue; //: 5;
		public ushort green; //: 6;
		public ushort red; //: 5;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct DDSStruct
	{
		public uint size;       // equals size of struct (which is part of the data file!)
		public uint flags;
		public uint height;
		public uint width;
		public uint sizeorpitch;
		public uint depth;
		public uint mipmapcount;
		public uint alphabitdepth;

		//[MarshalAs(UnmanagedType.U4, SizeConst = 11)]
		public uint[] reserved;//[11];

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct pixelformatstruct
		{
			public uint size;   // equals size of struct (which is part of the data file!)
			public uint flags;
			public uint fourcc;
			public uint rgbbitcount;
			public uint rbitmask;
			public uint gbitmask;
			public uint bbitmask;
			public uint alphabitmask;
		}

		public pixelformatstruct pixelformat;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct ddscapsstruct
		{
			public uint caps1;
			public uint caps2;
			public uint caps3;
			public uint caps4;
		}

		public ddscapsstruct ddscaps;
		public uint texturestage;
	}

	public enum PixelFormat
	{
		RGBA,
		RGB,
		DXT1,
		DXT2,
		DXT3,
		DXT4,
		DXT5,
		THREEDC,
		ATI1N,
		LUMINANCE,
		LUMINANCE_ALPHA,
		RXGB,
		A16B16G16R16,
		R16F,
		G16R16F,
		A16B16G16R16F,
		R32F,
		G32R32F,
		A32B32G32R32F,
		UNKNOWN
	}

	public class TestHelper
	{
		public static int[] ComputeMaskParams(uint mask)
		{
			int rShift1 = 0; int rMul = 0; int rShift2 = 0;
			Helper.ComputeMaskParams(mask, ref rShift1, ref rMul, ref rShift2);
			return new int[] { rShift1, rMul, rShift2 };
		}
	}
}