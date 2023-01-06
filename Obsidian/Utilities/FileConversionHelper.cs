using LeagueToolkit.Helpers;
using LeagueToolkit.IO.OBJ;
using LeagueToolkit.IO.SkeletonFile;
using LeagueToolkit.IO.StaticObjectFile;
using LeagueToolkit.IO.WadFile;
using Obsidian.MVVM.ViewModels.WAD;
using SharpGLTF.Schema2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using LeagueToolkit.Core.Mesh;
using LeagueToolkit.IO.MapGeometryFile;
using LeagueToolkit.IO.SimpleSkinFile;
using Animation = LeagueToolkit.IO.AnimationFile.Animation;

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
                    // TODO: remove entirely or fix code when a matching gltf function exists in LeagueToolkit
                    // new FileConversion("glTF", ".glb", null, ConvertSimpleSkinToGltf),
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
                    new FileConversion("glTF", ".glb", null, ConvertMapGeometryToGltf)
                });
            }
            else
            {
                return new FileConversionOptions(new List<FileConversion>());
            }
        }

        private static void ConvertSimpleSkinToGltf(FileConversionParameter parameter)
        {
            // TODO: this is not proper
            WadEntry simpleSkinWadEntry = parameter.Parameter;
            SkinnedMesh simpleSkin = SkinnedMesh.ReadFromSimpleSkin(simpleSkinWadEntry.GetDataHandle().GetDecompressedStream());
            ModelRoot gltf = simpleSkin.ToGltf(new Skeleton(
                new List<SkeletonJoint>(), new List<short>(new short[simpleSkin.VerticesView.VertexCount])),
                new Dictionary<string, ReadOnlyMemory<byte>>(),
                new List<(string, Animation)>());

            gltf.SaveGLB(Path.ChangeExtension(parameter.OutputPath, "glb"));
        }
        private static void ConvertSimpleSkinWithSkeletonToGltf(FileConversionParameter parameter)
        {
            WadEntry simpleSkinWadEntry = parameter.Parameter;
            WadEntry skeletonWadEntry = parameter.AdditionalParameters.FirstOrDefault(x => x.Item1 == FileConversionAdditionalParameterType.Skeleton).Item2;

            SkinnedMesh simpleSkin = SkinnedMesh.ReadFromSimpleSkin(simpleSkinWadEntry.GetDataHandle().GetDecompressedStream());
            Skeleton skeleton = new Skeleton(skeletonWadEntry.GetDataHandle().GetDecompressedStream());

            ModelRoot gltf = simpleSkin.ToGltf(skeleton, new Dictionary<string, ReadOnlyMemory<byte>>(), new List<(string, Animation)>());

            gltf.SaveGLB(Path.ChangeExtension(parameter.OutputPath, "glb"));
        }

        private static void ConvertScbToGltf(FileConversionParameter parameter)
        {
            WadEntry staticObjectWadEntry = parameter.Parameter;
            StaticObject staticObject = StaticObject.ReadSCB(staticObjectWadEntry.GetDataHandle().GetDecompressedStream());
            ModelRoot gltf = staticObject.ToGltf();

            gltf.SaveGLB(Path.ChangeExtension(parameter.OutputPath, "glb"));
        }
        private static void ConvertScbToObj(FileConversionParameter parameter)
        {
            WadEntry staticObjectWadEntry = parameter.Parameter;
            StaticObject staticObject = StaticObject.ReadSCB(staticObjectWadEntry.GetDataHandle().GetDecompressedStream());
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
            WadEntry staticObjectWadEntry = parameter.Parameter;
            StaticObject staticObject = StaticObject.ReadSCO(staticObjectWadEntry.GetDataHandle().GetDecompressedStream());
            ModelRoot gltf = staticObject.ToGltf();

            gltf.SaveGLB(Path.ChangeExtension(parameter.OutputPath, "glb"));
        }
        private static void ConvertScoToObj(FileConversionParameter parameter)
        {
            WadEntry staticObjectWadEntry = parameter.Parameter;
            StaticObject staticObject = StaticObject.ReadSCO(staticObjectWadEntry.GetDataHandle().GetDecompressedStream());
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
            WadEntry mapGeometryWadEntry = parameter.Parameter;
            MapGeometry mapGeometry = new MapGeometry(mapGeometryWadEntry.GetDataHandle().GetDecompressedStream());
            ModelRoot gltf = mapGeometry.ToGLTF();

            gltf.SaveGLB(Path.ChangeExtension(parameter.OutputPath, "glb"));
        }

        private static FileConversionParameter ConstructSimpleSkinWithSkeletonParameter(string outputPath, WadFileViewModel parameter, WadViewModel wad)
        {
            // We need to find a skeleton file with the same filename as the Simple Skin
            string skeletonPath = Path.ChangeExtension(parameter.Path, "skl");
            WadEntry skeletonWadEntry = wad.GetAllFiles().FirstOrDefault(x => x.Path == skeletonPath).Entry;
            if (skeletonWadEntry is null)
            {
                throw new Exception(Localization.Get("ConversionSimpleSkinWithSkeletonSkeletonNotFound"));
            }
            else
            {
                return new FileConversionParameter(outputPath, parameter.Entry, new List<(FileConversionAdditionalParameterType, WadEntry)>()
                {
                    (FileConversionAdditionalParameterType.Skeleton, skeletonWadEntry)
                });
            }
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
                return new FileConversionParameter(outputPath, parameter.Entry);
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
        public WadEntry Parameter { get; }
        public List<(FileConversionAdditionalParameterType, WadEntry)> AdditionalParameters { get; set; }

        public FileConversionParameter(string outputPath, WadEntry parameter)
            : this(outputPath, parameter, new List<(FileConversionAdditionalParameterType, WadEntry)>())
        {

        }
        public FileConversionParameter(
            string outputPath,
            WadEntry parameter,
            List<(FileConversionAdditionalParameterType, WadEntry)> additionalParameters)
        {
            this.OutputPath = outputPath;
            this.Parameter = parameter;
            this.AdditionalParameters = additionalParameters;
        }

    }
    public enum FileConversionAdditionalParameterType
    {
        Skeleton
    }
}
