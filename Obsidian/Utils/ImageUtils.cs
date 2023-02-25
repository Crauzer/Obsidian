﻿using BCnEncoder.Shared;
using CommunityToolkit.HighPerformance;
using LeagueToolkit.Core.Renderer;
using LeagueToolkit.Toolkit;
using LeagueToolkit.Utils;
using Obsidian.Data.Wad;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian.Utils;

public static class ImageUtils
{
    public static MemoryStream ConvertTextureToPng(Texture texture)
    {
        Image<Rgba32> image = ConvertTextureToImage(texture);
        MemoryStream imageStream = new();

        image.SaveAsPng(imageStream);
        imageStream.Position = 0;

        return imageStream;
    }

    public static Image<Rgba32> ConvertTextureToImage(Texture texture)
    {
        ReadOnlyMemory2D<ColorRgba32> mip = texture.Mips[0];
        return mip.ToImage();
    }

    public static Image<Rgba32> GetImageFromStream(Stream stream)
    {
        LeagueFileType fileType = LeagueFile.GetFileType(stream);
        if (fileType is (LeagueFileType.TextureDds or LeagueFileType.Texture))
        {
            return ConvertTextureToImage(Texture.Load(stream));
        }

        throw new InvalidDataException($"Failed to create Image for fileType: {fileType}");
    }
}
