using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using CommunityToolkit.HighPerformance;
using LeagueToolkit.Core.Animation;
using LeagueToolkit.Core.Mesh;
using LeagueToolkit.Core.Meta;
using LeagueToolkit.Core.Wad;
using LeagueToolkit.Helpers;
using LeagueToolkit.IO.MapGeometryFile;
using LeagueToolkit.IO.OBJ;
using LeagueToolkit.IO.SimpleSkinFile;
using LeagueToolkit.IO.StaticObjectFile;
using LeagueToolkit.Meta;
using Obsidian.MVVM.ViewModels.WAD;
using SharpGLTF.Schema2;

namespace Obsidian.Utilities
{
    public static class FileConversionHelper
    {
        public static FileConversionOptions GetFileConversionOptions(LeagueFileType fileType)
        {
            if (fileType == LeagueFileType.SimpleSkin)
            {
                return new FileConversionOptions(new List<FileConversion>
                {
                    new FileConversion("glTF", ".glb", null, ConvertSimpleSkinToGltf),
                    new FileConversion("glTF (with Skeleton)", ".glb", ConstructSimpleSkinWithSkeletonParameter, ConvertSimpleSkinWithSkeletonToGltf)
                });
            }
            else if (fileType == LeagueFileType.StaticObjectBinary)
            {
                return new FileConversionOptions(new List<FileConversion>()
                {
                    new FileConversion("glTF", ".glb", null, ConvertScbToGltf),
                    new FileConversion("OBJ", ".obj", null, ConvertScbToObj)
                });
            }
            else if (fileType == LeagueFileType.StaticObjectAscii)
            {
                return new FileConversionOptions(new List<FileConversion>()
                {
                    new FileConversion("glTF", ".glb", null, ConvertScoToGltf),
                    new FileConversion("OBJ", ".obj", null, ConvertScoToObj)
                });
            }
            else if (fileType == LeagueFileType.MapGeometry)
            {
                return new FileConversionOptions(new List<FileConversion>()
                {
                    new FileConversion("glTF", ".glb", MapGeoMaterialBinParameter, ConvertMapGeometryToGltf)
                });
            }
            else
            {
                return new FileConversionOptions(new List<FileConversion>());
            }
        }

        private static void ConvertSimpleSkinToGltf(FileConversionParameter parameter)
        {
            WadChunk simpleSkinWadChunk = parameter.WadEntry;
            SkinnedMesh simpleSkin = SkinnedMesh.ReadFromSimpleSkin(parameter.Wad.OpenChunk(simpleSkinWadChunk), false);
            ModelRoot gltf = simpleSkin.ToGltf(Array.Empty<(string, Stream)>());

            gltf.SaveGLB(Path.ChangeExtension(parameter.OutputPath, "glb"));
        }
        private static void ConvertSimpleSkinWithSkeletonToGltf(FileConversionParameter parameter)
        {
            WadChunk simpleSkinWadChunk = parameter.WadEntry;
            WadChunk skeletonWadChunk = parameter.AdditionalParameters.FirstOrDefault(x => x.Item1 == FileConversionAdditionalParameterType.Skeleton).Item2;

            SkinnedMesh simpleSkin = SkinnedMesh.ReadFromSimpleSkin(parameter.Wad.OpenChunk(simpleSkinWadChunk), false);
            using Stream skeletonStream = parameter.Wad.OpenChunk(skeletonWadChunk);
            RigResource skeleton = new RigResource(skeletonStream);

            ModelRoot gltf = simpleSkin.ToGltf(skeleton, Array.Empty<(string Material, Stream Texture)>(), Array.Empty<(string, IAnimationAsset)>());

            gltf.SaveGLB(Path.ChangeExtension(parameter.OutputPath, "glb"));
        }

        private static void ConvertScbToGltf(FileConversionParameter parameter)
        {
            WadChunk staticObjectWadChunk = parameter.WadEntry;
            StaticObject staticObject = StaticObject.ReadSCB(parameter.Wad.OpenChunk(staticObjectWadChunk)); // leaveOpen: false
            ModelRoot gltf = staticObject.ToGltf();

            gltf.SaveGLB(Path.ChangeExtension(parameter.OutputPath, "glb"));
        }
        private static void ConvertScbToObj(FileConversionParameter parameter)
        {
            WadChunk staticObjectWadChunk = parameter.WadEntry;
            StaticObject staticObject = StaticObject.ReadSCB(parameter.Wad.OpenChunk(staticObjectWadChunk)); // leaveOpen: false
            var objs = staticObject.ToObj();

            string baseName = Path.GetFileNameWithoutExtension(parameter.OutputPath);
            foreach ((string material, OBJFile obj) in objs)
            {
                string objPath = parameter.OutputPath.Replace(baseName, baseName + '_' + material);
                obj.Write(objPath);
            }
        }

        private static void ConvertScoToGltf(FileConversionParameter parameter)
        {
            WadChunk staticObjectWadChunk = parameter.WadEntry;
            StaticObject staticObject = StaticObject.ReadSCO(parameter.Wad.OpenChunk(staticObjectWadChunk)); // leaveOpen: false
            ModelRoot gltf = staticObject.ToGltf();

            gltf.SaveGLB(Path.ChangeExtension(parameter.OutputPath, "glb"));
        }
        private static void ConvertScoToObj(FileConversionParameter parameter)
        {
            WadChunk staticObjectWadChunk = parameter.WadEntry;
            StaticObject staticObject = StaticObject.ReadSCO(parameter.Wad.OpenChunk(staticObjectWadChunk)); // leaveOpen: false
            var objs = staticObject.ToObj();

            string baseName = Path.GetFileNameWithoutExtension(parameter.OutputPath);
            foreach ((string material, OBJFile obj) in objs)
            {
                string objPath = parameter.OutputPath.Replace(baseName, baseName + '_' + material);
                obj.Write(objPath);
            }
        }

        private static void ConvertMapGeometryToGltf(FileConversionParameter parameter)
        {
            WadChunk mapGeometryWadChunk = parameter.WadEntry;
            WadChunk materialBinWadChunk = parameter.AdditionalParameters.First(x => x.Item1 == FileConversionAdditionalParameterType.MaterialBin).Item2;
            MapGeometry mapGeometry = new MapGeometry(parameter.Wad.LoadChunkDecompressed(mapGeometryWadChunk).AsStream()); // leaveOpen: false
            using Stream materialBinStream = parameter.Wad.LoadChunkDecompressed(materialBinWadChunk).AsStream();
            ModelRoot gltf = mapGeometry.ToGltf(new BinTree(materialBinStream), new MapGeometryGltfConversionContext
            {
                MetaEnvironment = MetaEnvironment.Create(Assembly.Load("LeagueToolkit.Meta.Classes").GetExportedTypes().Where(x => x.IsClass))
            });

            gltf.SaveGLB(Path.ChangeExtension(parameter.OutputPath, "glb"));
        }

        private static FileConversionParameter ConstructSimpleSkinWithSkeletonParameter(string outputPath, WadFileViewModel parameter, WadViewModel wad)
        {
            // We need to find a skeleton file with the same filename as the Simple Skin
            string skeletonPath = Path.ChangeExtension(parameter.Path, "skl");
            WadChunk? skeletonWadChunk = wad.GetAllFiles().FirstOrDefault(x => x.Path == skeletonPath)?.Entry;
            if (skeletonWadChunk is null)
            {
                throw new Exception(Localization.Get("ConversionSimpleSkinWithSkeletonSkeletonNotFound"));
            }
            else
            {
                return new FileConversionParameter(outputPath, parameter, new (FileConversionAdditionalParameterType, WadChunk)[]
                {
                    (FileConversionAdditionalParameterType.Skeleton, skeletonWadChunk.Value)
                });
            }
        }

        private static FileConversionParameter MapGeoMaterialBinParameter(string outputPath, WadFileViewModel parameter, WadViewModel wad)
        {
            string materialBinPath = Path.ChangeExtension(parameter.Path, "materials.bin");
            WadChunk? materialBinWadChunk = wad.GetAllFiles().FirstOrDefault(x => x.Path == materialBinPath)?.Entry;

            if (materialBinWadChunk is null) throw new Exception("Failed to find corresponding materials.bin file.");

            return new FileConversionParameter(outputPath, parameter, new (FileConversionAdditionalParameterType, WadChunk)[]
            {
                (FileConversionAdditionalParameterType.MaterialBin, materialBinWadChunk.Value)
            });
        }
    }

    public class FileConversionOptions
    {
        public bool IsConversionAvailable => this._availableConversions.Count != 0;

        public ReadOnlyCollection<string> AvailableConversions => this._availableConversions.AsReadOnly();

        private readonly List<string> _availableConversions;
        private readonly List<FileConversion> _conversions;

        public FileConversionOptions(List<FileConversion> conversions)
        {
            this._conversions = conversions;
            this._availableConversions = conversions.Select(x => x.Name).ToList();
        }

        public FileConversion GetConversion(string name)
        {
            return this._conversions.FirstOrDefault(x => x.Name == name);
        }
    }

    public class FileConversion
    {
        public string Name { get; }
        public string OutputExtension { get; }

        private Func<string, WadFileViewModel, WadViewModel, FileConversionParameter> _parameterConstructor { get; }
        private Action<FileConversionParameter> _conversion { get; }

        public FileConversion(
            string name,
            string outputExtension,
            Func<string, WadFileViewModel, WadViewModel, FileConversionParameter> parameterConstructor,
            Action<FileConversionParameter> conversion)
        {
            this.Name = name;
            this.OutputExtension = outputExtension;
            this._parameterConstructor = parameterConstructor;
            this._conversion = conversion;
        }

        public FileConversionParameter ConstructParameter(string outputPath, WadFileViewModel parameter, WadViewModel wad)
        {
            if (this._parameterConstructor != null)
            {
                return this._parameterConstructor.Invoke(outputPath, parameter, wad);
            }
            else
            {
                return new FileConversionParameter(outputPath, parameter);
            }
        }

        public void Convert(FileConversionParameter parameter)
        {
            this._conversion.Invoke(parameter);
        }
    }

    public class FileConversionParameter
    {
        public string OutputPath { get; }
        public WadFile Wad { get; }
        public WadChunk WadEntry { get; }
        public IReadOnlyList<(FileConversionAdditionalParameterType, WadChunk)> AdditionalParameters { get; }

        public FileConversionParameter(string outputPath, WadFileViewModel parameter)
            : this(outputPath, parameter, Array.Empty<(FileConversionAdditionalParameterType, WadChunk)>())
        {

        }
        public FileConversionParameter(
            string outputPath,
            WadFileViewModel parameter,
            IReadOnlyList<(FileConversionAdditionalParameterType, WadChunk)> additionalParameters)
        {
            this.OutputPath = outputPath;
            this.Wad = parameter.ParentWad;
            this.WadEntry = parameter.Entry;
            this.AdditionalParameters = additionalParameters;
        }

    }
    public enum FileConversionAdditionalParameterType
    {
        Skeleton,
        MaterialBin
    }
}
