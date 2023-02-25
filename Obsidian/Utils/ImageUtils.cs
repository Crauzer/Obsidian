using BCnEncoder.Shared;
using CommunityToolkit.HighPerformance;
using LeagueToolkit.Core.Renderer;
using LeagueToolkit.Toolkit;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
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
}
