using System;
using Imaging.DDSReader.Utils;

namespace Imaging.DDSReader
{
	internal class Decompressor
	{
		internal static byte[] Expand(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			System.Diagnostics.Debug.WriteLine(pixelFormat);
			// allocate bitmap
			byte[] rawData = null;

			switch (pixelFormat)
			{
				case PixelFormat.RGBA:
					rawData = DecompressRGBA(header, data, pixelFormat);
					break;

				case PixelFormat.RGB:
					rawData = DecompressRGB(header, data, pixelFormat);
					break;

				case PixelFormat.LUMINANCE:
				case PixelFormat.LUMINANCE_ALPHA:
					rawData = DecompressLum(header, data, pixelFormat);
					break;

				case PixelFormat.DXT1:
					rawData = DecompressDXT1(header, data, pixelFormat);
					break;

				case PixelFormat.DXT2:
					rawData = DecompressDXT2(header, data, pixelFormat);
					break;

				case PixelFormat.DXT3:
					rawData = DecompressDXT3(header, data, pixelFormat);
					break;

				case PixelFormat.DXT4:
					rawData = DecompressDXT4(header, data, pixelFormat);
					break;

				case PixelFormat.DXT5:
					rawData = DecompressDXT5(header, data, pixelFormat);
					break;

				case PixelFormat.THREEDC:
					rawData = Decompress3Dc(header, data, pixelFormat);
					break;

				case PixelFormat.ATI1N:
					rawData = DecompressAti1n(header, data, pixelFormat);
					break;

				case PixelFormat.RXGB:
					rawData = DecompressRXGB(header, data, pixelFormat);
					break;

				case PixelFormat.R16F:
				case PixelFormat.G16R16F:
				case PixelFormat.A16B16G16R16F:
				case PixelFormat.R32F:
				case PixelFormat.G32R32F:
				case PixelFormat.A32B32G32R32F:
					rawData = DecompressFloat(header, data, pixelFormat);
					break;

				default:
					throw new UnknownFileFormatException();
			}

			return rawData;
		}

		private static unsafe byte[] DecompressDXT1(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			// DXT1 decompressor
			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

			Colour8888[] colours = new Colour8888[4];
			colours[0].alpha = 0xFF;
			colours[1].alpha = 0xFF;
			colours[2].alpha = 0xFF;

			fixed (byte* bytePtr = data)
			{
				byte* temp = bytePtr;
				for (int z = 0; z < depth; z++)
				{
					for (int y = 0; y < height; y += 4)
					{
						for (int x = 0; x < width; x += 4)
						{
							ushort colour0 = *((ushort*)temp);
							ushort colour1 = *((ushort*)(temp + 2));
							Helper.DxtcReadColor(colour0, ref colours[0]);
							Helper.DxtcReadColor(colour1, ref colours[1]);

							uint bitmask = ((uint*)temp)[1];
							temp += 8;

							if (colour0 > colour1)
							{
								// Four-color block: derive the other two colors.
								// 00 = color_0, 01 = color_1, 10 = color_2, 11 = color_3
								// These 2-bit codes correspond to the 2-bit fields
								// stored in the 64-bit block.
								colours[2].blue = (byte)((2 * colours[0].blue + colours[1].blue + 1) / 3);
								colours[2].green = (byte)((2 * colours[0].green + colours[1].green + 1) / 3);
								colours[2].red = (byte)((2 * colours[0].red + colours[1].red + 1) / 3);
								//colours[2].alpha = 0xFF;

								colours[3].blue = (byte)((colours[0].blue + 2 * colours[1].blue + 1) / 3);
								colours[3].green = (byte)((colours[0].green + 2 * colours[1].green + 1) / 3);
								colours[3].red = (byte)((colours[0].red + 2 * colours[1].red + 1) / 3);
								colours[3].alpha = 0xFF;
							}
							else
							{
								// Three-color block: derive the other color.
								// 00 = color_0,  01 = color_1,  10 = color_2,
								// 11 = transparent.
								// These 2-bit codes correspond to the 2-bit fields
								// stored in the 64-bit block.
								colours[2].blue = (byte)((colours[0].blue + colours[1].blue) / 2);
								colours[2].green = (byte)((colours[0].green + colours[1].green) / 2);
								colours[2].red = (byte)((colours[0].red + colours[1].red) / 2);
								//colours[2].alpha = 0xFF;

								colours[3].blue = (byte)((colours[0].blue + 2 * colours[1].blue + 1) / 3);
								colours[3].green = (byte)((colours[0].green + 2 * colours[1].green + 1) / 3);
								colours[3].red = (byte)((colours[0].red + 2 * colours[1].red + 1) / 3);
								colours[3].alpha = 0x00;
							}

							for (int j = 0, k = 0; j < 4; j++)
							{
								for (int i = 0; i < 4; i++, k++)
								{
									int select = (int)((bitmask & (0x03 << k * 2)) >> k * 2);
									Colour8888 col = colours[select];
									if (((x + i) < width) && ((y + j) < height))
									{
										uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp);
										rawData[offset + 0] = (byte)col.red;
										rawData[offset + 1] = (byte)col.green;
										rawData[offset + 2] = (byte)col.blue;
										rawData[offset + 3] = (byte)col.alpha;
									}
								}
							}
						}
					}
				}
			}

			return rawData;
		}

		private static byte[] DecompressDXT2(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			// Can do color & alpha same as dxt3, but color is pre-multiplied
			// so the result will be wrong unless corrected.
			byte[] rawData = DecompressDXT3(header, data, pixelFormat);
			Helper.CorrectPremult((uint)(width * height * depth), ref rawData);

			return rawData;
		}

		private static unsafe byte[] DecompressDXT3(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			// DXT3 decompressor
			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];
			Colour8888[] colours = new Colour8888[4];

			fixed (byte* bytePtr = data)
			{
				byte* temp = bytePtr;
				for (int z = 0; z < depth; z++)
				{
					for (int y = 0; y < height; y += 4)
					{
						for (int x = 0; x < width; x += 4)
						{
							byte* alpha = temp;
							temp += 8;

							Helper.DxtcReadColors(temp, ref colours);
							temp += 4;

							uint bitmask = ((uint*)temp)[1];
							temp += 4;

							// Four-color block: derive the other two colors.
							// 00 = color_0, 01 = color_1, 10 = color_2, 11	= color_3
							// These 2-bit codes correspond to the 2-bit fields
							// stored in the 64-bit block.
							colours[2].blue = (byte)((2 * colours[0].blue + colours[1].blue + 1) / 3);
							colours[2].green = (byte)((2 * colours[0].green + colours[1].green + 1) / 3);
							colours[2].red = (byte)((2 * colours[0].red + colours[1].red + 1) / 3);
							//colours[2].alpha = 0xFF;

							colours[3].blue = (byte)((colours[0].blue + 2 * colours[1].blue + 1) / 3);
							colours[3].green = (byte)((colours[0].green + 2 * colours[1].green + 1) / 3);
							colours[3].red = (byte)((colours[0].red + 2 * colours[1].red + 1) / 3);
							//colours[3].alpha = 0xFF;

							for (int j = 0, k = 0; j < 4; j++)
							{
								for (int i = 0; i < 4; k++, i++)
								{
									int select = (int)((bitmask & (0x03 << k * 2)) >> k * 2);

									if (((x + i) < width) && ((y + j) < height))
									{
										uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp);
										rawData[offset + 0] = (byte)colours[select].red;
										rawData[offset + 1] = (byte)colours[select].green;
										rawData[offset + 2] = (byte)colours[select].blue;
									}
								}
							}

							for (int j = 0; j < 4; j++)
							{
								//ushort word = (ushort)(alpha[2 * j] + 256 * alpha[2 * j + 1]);
								ushort word = (ushort)(alpha[2 * j] | (alpha[2 * j + 1] << 8));
								for (int i = 0; i < 4; i++)
								{
									if (((x + i) < width) && ((y + j) < height))
									{
										uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp + 3);
										rawData[offset] = (byte)(word & 0x0F);
										rawData[offset] = (byte)(rawData[offset] | (rawData[offset] << 4));
									}
									word >>= 4;
								}
							}
						}
					}
				}
			}
			return rawData;
		}

		private static byte[] DecompressDXT4(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			// Can do color & alpha same as dxt5, but color is pre-multiplied
			// so the result will be wrong unless corrected.
			byte[] rawData = DecompressDXT5(header, data, pixelFormat);
			Helper.CorrectPremult((uint)(width * height * depth), ref rawData);

			return rawData;
		}

		private static unsafe byte[] DecompressDXT5(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];
			Colour8888[] colours = new Colour8888[4];
			ushort[] alphas = new ushort[8];

			fixed (byte* bytePtr = data)
			{
				byte* temp = bytePtr;
				for (int z = 0; z < depth; z++)
				{
					for (int y = 0; y < height; y += 4)
					{
						for (int x = 0; x < width; x += 4)
						{
							if (y >= height || x >= width)
								break;

							alphas[0] = temp[0];
							alphas[1] = temp[1];
							byte* alphamask = (temp + 2);
							temp += 8;

							Helper.DxtcReadColors(temp, ref colours);
							uint bitmask = ((uint*)temp)[1];
							temp += 8;

							// Four-color block: derive the other two colors.
							// 00 = color_0, 01 = color_1, 10 = color_2, 11	= color_3
							// These 2-bit codes correspond to the 2-bit fields
							// stored in the 64-bit block.
							colours[2].blue = (byte)((2 * colours[0].blue + colours[1].blue + 1) / 3);
							colours[2].green = (byte)((2 * colours[0].green + colours[1].green + 1) / 3);
							colours[2].red = (byte)((2 * colours[0].red + colours[1].red + 1) / 3);
							//colours[2].alpha = 0xFF;

							colours[3].blue = (byte)((colours[0].blue + 2 * colours[1].blue + 1) / 3);
							colours[3].green = (byte)((colours[0].green + 2 * colours[1].green + 1) / 3);
							colours[3].red = (byte)((colours[0].red + 2 * colours[1].red + 1) / 3);
							//colours[3].alpha = 0xFF;

							int k = 0;
							for (int j = 0; j < 4; j++)
							{
								for (int i = 0; i < 4; k++, i++)
								{
									int select = (int)((bitmask & (0x03 << k * 2)) >> k * 2);
									Colour8888 col = colours[select];
									// only put pixels out < width or height
									if (((x + i) < width) && ((y + j) < height))
									{
										uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp);
										rawData[offset] = (byte)col.red;
										rawData[offset + 1] = (byte)col.green;
										rawData[offset + 2] = (byte)col.blue;
									}
								}
							}

							// 8-alpha or 6-alpha block?
							if (alphas[0] > alphas[1])
							{
								// 8-alpha block:  derive the other six alphas.
								// Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
								alphas[2] = (ushort)((6 * alphas[0] + 1 * alphas[1] + 3) / 7); // bit code 010
								alphas[3] = (ushort)((5 * alphas[0] + 2 * alphas[1] + 3) / 7); // bit code 011
								alphas[4] = (ushort)((4 * alphas[0] + 3 * alphas[1] + 3) / 7); // bit code 100
								alphas[5] = (ushort)((3 * alphas[0] + 4 * alphas[1] + 3) / 7); // bit code 101
								alphas[6] = (ushort)((2 * alphas[0] + 5 * alphas[1] + 3) / 7); // bit code 110
								alphas[7] = (ushort)((1 * alphas[0] + 6 * alphas[1] + 3) / 7); // bit code 111
							}
							else
							{
								// 6-alpha block.
								// Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
								alphas[2] = (ushort)((4 * alphas[0] + 1 * alphas[1] + 2) / 5); // Bit code 010
								alphas[3] = (ushort)((3 * alphas[0] + 2 * alphas[1] + 2) / 5); // Bit code 011
								alphas[4] = (ushort)((2 * alphas[0] + 3 * alphas[1] + 2) / 5); // Bit code 100
								alphas[5] = (ushort)((1 * alphas[0] + 4 * alphas[1] + 2) / 5); // Bit code 101
								alphas[6] = 0x00; // Bit code 110
								alphas[7] = 0xFF; // Bit code 111
							}

							// Note: Have to separate the next two loops,
							// it operates on a 6-byte system.

							// First three bytes
							//uint bits = (uint)(alphamask[0]);
							uint bits = (uint)((alphamask[0]) | (alphamask[1] << 8) | (alphamask[2] << 16));
							for (int j = 0; j < 2; j++)
							{
								for (int i = 0; i < 4; i++)
								{
									// only put pixels out < width or height
									if (((x + i) < width) && ((y + j) < height))
									{
										uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp + 3);
										rawData[offset] = (byte)alphas[bits & 0x07];
									}
									bits >>= 3;
								}
							}

							// Last three bytes
							//bits = (uint)(alphamask[3]);
							bits = (uint)((alphamask[3]) | (alphamask[4] << 8) | (alphamask[5] << 16));
							for (int j = 2; j < 4; j++)
							{
								for (int i = 0; i < 4; i++)
								{
									// only put pixels out < width or height
									if (((x + i) < width) && ((y + j) < height))
									{
										uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp + 3);
										rawData[offset] = (byte)alphas[bits & 0x07];
									}
									bits >>= 3;
								}
							}
						}
					}
				}
			}

			return rawData;
		}

		private static unsafe byte[] DecompressRGB(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

			uint valMask = (uint)((header.pixelformat.rgbbitcount == 32) ? ~0 : (1 << (int)header.pixelformat.rgbbitcount) - 1);
			uint pixSize = (uint)(((int)header.pixelformat.rgbbitcount + 7) / 8);
			int rShift1 = 0; int rMul = 0; int rShift2 = 0;
			Helper.ComputeMaskParams(header.pixelformat.rbitmask, ref rShift1, ref rMul, ref rShift2);
			int gShift1 = 0; int gMul = 0; int gShift2 = 0;
			Helper.ComputeMaskParams(header.pixelformat.gbitmask, ref gShift1, ref gMul, ref gShift2);
			int bShift1 = 0; int bMul = 0; int bShift2 = 0;
			Helper.ComputeMaskParams(header.pixelformat.bbitmask, ref bShift1, ref bMul, ref bShift2);

			int offset = 0;
			int pixnum = width * height * depth;
			fixed (byte* bytePtr = data)
			{
				byte* temp = bytePtr;
				while (pixnum-- > 0)
				{
					uint px = *((uint*)temp) & valMask;
					temp += pixSize;
					uint pxc = px & header.pixelformat.rbitmask;
					rawData[offset + 0] = (byte)(((pxc >> rShift1) * rMul) >> rShift2);
					pxc = px & header.pixelformat.gbitmask;
					rawData[offset + 1] = (byte)(((pxc >> gShift1) * gMul) >> gShift2);
					pxc = px & header.pixelformat.bbitmask;
					rawData[offset + 2] = (byte)(((pxc >> bShift1) * bMul) >> bShift2);
					rawData[offset + 3] = 0xff;
					offset += 4;
				}
			}
			return rawData;
		}

		private static unsafe byte[] DecompressRGBA(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

			uint valMask = (uint)((header.pixelformat.rgbbitcount == 32) ? ~0 : (1 << (int)header.pixelformat.rgbbitcount) - 1);
			// Funny x86s, make 1 << 32 == 1
			uint pixSize = (header.pixelformat.rgbbitcount + 7) / 8;
			int rShift1 = 0; int rMul = 0; int rShift2 = 0;
			Helper.ComputeMaskParams(header.pixelformat.rbitmask, ref rShift1, ref rMul, ref rShift2);
			int gShift1 = 0; int gMul = 0; int gShift2 = 0;
			Helper.ComputeMaskParams(header.pixelformat.gbitmask, ref gShift1, ref gMul, ref gShift2);
			int bShift1 = 0; int bMul = 0; int bShift2 = 0;
			Helper.ComputeMaskParams(header.pixelformat.bbitmask, ref bShift1, ref bMul, ref bShift2);
			int aShift1 = 0; int aMul = 0; int aShift2 = 0;
			Helper.ComputeMaskParams(header.pixelformat.alphabitmask, ref aShift1, ref aMul, ref aShift2);

			int offset = 0;
			int pixnum = width * height * depth;
			fixed (byte* bytePtr = data)
			{
				byte* temp = bytePtr;

				while (pixnum-- > 0)
				{
					uint px = *((uint*)temp) & valMask;
					temp += pixSize;
					uint pxc = px & header.pixelformat.rbitmask;
					rawData[offset + 0] = (byte)(((pxc >> rShift1) * rMul) >> rShift2);
					pxc = px & header.pixelformat.gbitmask;
					rawData[offset + 1] = (byte)(((pxc >> gShift1) * gMul) >> gShift2);
					pxc = px & header.pixelformat.bbitmask;
					rawData[offset + 2] = (byte)(((pxc >> bShift1) * bMul) >> bShift2);
					pxc = px & header.pixelformat.alphabitmask;
					rawData[offset + 3] = (byte)(((pxc >> aShift1) * aMul) >> aShift2);
					offset += 4;
				}
			}
			return rawData;
		}

		private static unsafe byte[] Decompress3Dc(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];
			byte[] yColours = new byte[8];
			byte[] xColours = new byte[8];

			int offset = 0;
			fixed (byte* bytePtr = data)
			{
				byte* temp = bytePtr;
				for (int z = 0; z < depth; z++)
				{
					for (int y = 0; y < height; y += 4)
					{
						for (int x = 0; x < width; x += 4)
						{
							byte* temp2 = temp + 8;

							//Read Y palette
							int t1 = yColours[0] = temp[0];
							int t2 = yColours[1] = temp[1];
							temp += 2;
							if (t1 > t2)
								for (int i = 2; i < 8; ++i)
									yColours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 7);
							else
							{
								for (int i = 2; i < 6; ++i)
									yColours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 5);
								yColours[6] = 0;
								yColours[7] = 255;
							}

							// Read X palette
							t1 = xColours[0] = temp2[0];
							t2 = xColours[1] = temp2[1];
							temp2 += 2;
							if (t1 > t2)
								for (int i = 2; i < 8; ++i)
									xColours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 7);
							else
							{
								for (int i = 2; i < 6; ++i)
									xColours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 5);
								xColours[6] = 0;
								xColours[7] = 255;
							}

							//decompress pixel data
							int currentOffset = offset;
							for (int k = 0; k < 4; k += 2)
							{
								// First three bytes
								uint bitmask = ((uint)(temp[0]) << 0) | ((uint)(temp[1]) << 8) | ((uint)(temp[2]) << 16);
								uint bitmask2 = ((uint)(temp2[0]) << 0) | ((uint)(temp2[1]) << 8) | ((uint)(temp2[2]) << 16);
								for (int j = 0; j < 2; j++)
								{
									// only put pixels out < height
									if ((y + k + j) < height)
									{
										for (int i = 0; i < 4; i++)
										{
											// only put pixels out < width
											if (((x + i) < width))
											{
												int t;
												byte tx, ty;

												t1 = currentOffset + (x + i) * 3;
												rawData[t1 + 1] = ty = yColours[bitmask & 0x07];
												rawData[t1 + 0] = tx = xColours[bitmask2 & 0x07];

												//calculate b (z) component ((r/255)^2 + (g/255)^2 + (b/255)^2 = 1
												t = 127 * 128 - (tx - 127) * (tx - 128) - (ty - 127) * (ty - 128);
												if (t > 0)
													rawData[t1 + 2] = (byte)(Math.Sqrt(t) + 128);
												else
													rawData[t1 + 2] = 0x7F;
											}
											bitmask >>= 3;
											bitmask2 >>= 3;
										}
										currentOffset += bps;
									}
								}
								temp += 3;
								temp2 += 3;
							}

							//skip bytes that were read via Temp2
							temp += 8;
						}
						offset += bps * 4;
					}
				}
			}

			return rawData;
		}

		private static unsafe byte[] DecompressAti1n(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];
			byte[] colours = new byte[8];

			uint offset = 0;
			fixed (byte* bytePtr = data)
			{
				byte* temp = bytePtr;
				for (int z = 0; z < depth; z++)
				{
					for (int y = 0; y < height; y += 4)
					{
						for (int x = 0; x < width; x += 4)
						{
							//Read palette
							int t1 = colours[0] = temp[0];
							int t2 = colours[1] = temp[1];
							temp += 2;
							if (t1 > t2)
								for (int i = 2; i < 8; ++i)
									colours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 7);
							else
							{
								for (int i = 2; i < 6; ++i)
									colours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 5);
								colours[6] = 0;
								colours[7] = 255;
							}

							//decompress pixel data
							uint currOffset = offset;
							for (int k = 0; k < 4; k += 2)
							{
								// First three bytes
								uint bitmask = ((uint)(temp[0]) << 0) | ((uint)(temp[1]) << 8) | ((uint)(temp[2]) << 16);
								for (int j = 0; j < 2; j++)
								{
									// only put pixels out < height
									if ((y + k + j) < height)
									{
										for (int i = 0; i < 4; i++)
										{
											// only put pixels out < width
											if (((x + i) < width))
											{
												t1 = (int)(currOffset + (x + i));
												rawData[t1] = colours[bitmask & 0x07];
											}
											bitmask >>= 3;
										}
										currOffset += (uint)bps;
									}
								}
								temp += 3;
							}
						}
						offset += (uint)(bps * 4);
					}
				}
			}
			return rawData;
		}

		private static unsafe byte[] DecompressLum(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

			int lShift1 = 0; int lMul = 0; int lShift2 = 0;
			Helper.ComputeMaskParams(header.pixelformat.rbitmask, ref lShift1, ref lMul, ref lShift2);

			int offset = 0;
			int pixnum = width * height * depth;
			fixed (byte* bytePtr = data)
			{
				byte* temp = bytePtr;
				while (pixnum-- > 0)
				{
					byte px = *(temp++);
					rawData[offset + 0] = (byte)(((px >> lShift1) * lMul) >> lShift2);
					rawData[offset + 1] = (byte)(((px >> lShift1) * lMul) >> lShift2);
					rawData[offset + 2] = (byte)(((px >> lShift1) * lMul) >> lShift2);
					rawData[offset + 3] = (byte)(((px >> lShift1) * lMul) >> lShift2);
					offset += 4;
				}
			}
			return rawData;
		}

		private static unsafe byte[] DecompressRXGB(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

			Colour565 color_0 = new Colour565();
			Colour565 color_1 = new Colour565();
			Colour8888[] colours = new Colour8888[4];
			byte[] alphas = new byte[8];

			fixed (byte* bytePtr = data)
			{
				byte* temp = bytePtr;
				for (int z = 0; z < depth; z++)
				{
					for (int y = 0; y < height; y += 4)
					{
						for (int x = 0; x < width; x += 4)
						{
							if (y >= height || x >= width)
								break;
							alphas[0] = temp[0];
							alphas[1] = temp[1];
							byte* alphamask = temp + 2;
							temp += 8;

							Helper.DxtcReadColors(temp, ref color_0, ref color_1);
							temp += 4;

							uint bitmask = ((uint*)temp)[1];
							temp += 4;

							colours[0].red = (byte)(color_0.red << 3);
							colours[0].green = (byte)(color_0.green << 2);
							colours[0].blue = (byte)(color_0.blue << 3);
							colours[0].alpha = 0xFF;

							colours[1].red = (byte)(color_1.red << 3);
							colours[1].green = (byte)(color_1.green << 2);
							colours[1].blue = (byte)(color_1.blue << 3);
							colours[1].alpha = 0xFF;

							// Four-color block: derive the other two colors.
							// 00 = color_0, 01 = color_1, 10 = color_2, 11 = color_3
							// These 2-bit codes correspond to the 2-bit fields
							// stored in the 64-bit block.
							colours[2].blue = (byte)((2 * colours[0].blue + colours[1].blue + 1) / 3);
							colours[2].green = (byte)((2 * colours[0].green + colours[1].green + 1) / 3);
							colours[2].red = (byte)((2 * colours[0].red + colours[1].red + 1) / 3);
							colours[2].alpha = 0xFF;

							colours[3].blue = (byte)((colours[0].blue + 2 * colours[1].blue + 1) / 3);
							colours[3].green = (byte)((colours[0].green + 2 * colours[1].green + 1) / 3);
							colours[3].red = (byte)((colours[0].red + 2 * colours[1].red + 1) / 3);
							colours[3].alpha = 0xFF;

							int k = 0;
							for (int j = 0; j < 4; j++)
							{
								for (int i = 0; i < 4; i++, k++)
								{
									int select = (int)((bitmask & (0x03 << k * 2)) >> k * 2);
									Colour8888 col = colours[select];

									// only put pixels out < width or height
									if (((x + i) < width) && ((y + j) < height))
									{
										uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp);
										rawData[offset + 0] = col.red;
										rawData[offset + 1] = col.green;
										rawData[offset + 2] = col.blue;
									}
								}
							}

							// 8-alpha or 6-alpha block?
							if (alphas[0] > alphas[1])
							{
								// 8-alpha block:  derive the other six alphas.
								// Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
								alphas[2] = (byte)((6 * alphas[0] + 1 * alphas[1] + 3) / 7);    // bit code 010
								alphas[3] = (byte)((5 * alphas[0] + 2 * alphas[1] + 3) / 7);    // bit code 011
								alphas[4] = (byte)((4 * alphas[0] + 3 * alphas[1] + 3) / 7);    // bit code 100
								alphas[5] = (byte)((3 * alphas[0] + 4 * alphas[1] + 3) / 7);    // bit code 101
								alphas[6] = (byte)((2 * alphas[0] + 5 * alphas[1] + 3) / 7);    // bit code 110
								alphas[7] = (byte)((1 * alphas[0] + 6 * alphas[1] + 3) / 7);    // bit code 111
							}
							else
							{
								// 6-alpha block.
								// Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
								alphas[2] = (byte)((4 * alphas[0] + 1 * alphas[1] + 2) / 5);    // Bit code 010
								alphas[3] = (byte)((3 * alphas[0] + 2 * alphas[1] + 2) / 5);    // Bit code 011
								alphas[4] = (byte)((2 * alphas[0] + 3 * alphas[1] + 2) / 5);    // Bit code 100
								alphas[5] = (byte)((1 * alphas[0] + 4 * alphas[1] + 2) / 5);    // Bit code 101
								alphas[6] = 0x00;                                       // Bit code 110
								alphas[7] = 0xFF;                                       // Bit code 111
							}

							// Note: Have to separate the next two loops,
							//	it operates on a 6-byte system.
							// First three bytes
							uint bits = *((uint*)alphamask);
							for (int j = 0; j < 2; j++)
							{
								for (int i = 0; i < 4; i++)
								{
									// only put pixels out < width or height
									if (((x + i) < width) && ((y + j) < height))
									{
										uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp + 3);
										rawData[offset] = alphas[bits & 0x07];
									}
									bits >>= 3;
								}
							}

							// Last three bytes
							bits = *((uint*)&alphamask[3]);
							for (int j = 2; j < 4; j++)
							{
								for (int i = 0; i < 4; i++)
								{
									// only put pixels out < width or height
									if (((x + i) < width) && ((y + j) < height))
									{
										uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp + 3);
										rawData[offset] = alphas[bits & 0x07];
									}
									bits >>= 3;
								}
							}
						}
					}
				}
			}
			return rawData;
		}

		private static unsafe byte[] DecompressFloat(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];
			int size = 0;
			fixed (byte* bytePtr = data)
			{
				byte* temp = bytePtr;
				fixed (byte* destPtr = rawData)
				{
					byte* destData = destPtr;
					switch (pixelFormat)
					{
						case PixelFormat.R32F:  // Red float, green = blue = max
							size = width * height * depth * 3;
							for (int i = 0, j = 0; i < size; i += 3, j++)
							{
								((float*)destData)[i] = ((float*)temp)[j];
								((float*)destData)[i + 1] = 1.0f;
								((float*)destData)[i + 2] = 1.0f;
							}
							break;

						case PixelFormat.A32B32G32R32F:  // Direct copy of float RGBA data
							Array.Copy(data, rawData, data.Length);
							break;

						case PixelFormat.G32R32F:  // Red float, green float, blue = max
							size = width * height * depth * 3;
							for (int i = 0, j = 0; i < size; i += 3, j += 2)
							{
								((float*)destData)[i] = ((float*)temp)[j];
								((float*)destData)[i + 1] = ((float*)temp)[j + 1];
								((float*)destData)[i + 2] = 1.0f;
							}
							break;

						case PixelFormat.R16F:  // Red float, green = blue = max
							size = width * height * depth * bpp;
							Helper.ConvR16ToFloat32((uint*)destData, (ushort*)temp, (uint)size);
							break;

						case PixelFormat.A16B16G16R16F:  // Just convert from half to float.
							size = width * height * depth * bpp;
							Helper.ConvFloat16ToFloat32((uint*)destData, (ushort*)temp, (uint)size);
							break;

						case PixelFormat.G16R16F:  // Convert from half to float, set blue = max.
							size = width * height * depth * bpp;
							Helper.ConvG16R16ToFloat32((uint*)destData, (ushort*)temp, (uint)size);
							break;

						default:
							break;
					}
				}
			}

			return rawData;
		}
	}
}