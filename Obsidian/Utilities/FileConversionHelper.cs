using Fantome.Libraries.League.Helpers;
using Fantome.Libraries.League.IO.MapGeometry;
using Fantome.Libraries.League.IO.OBJ;
using Fantome.Libraries.League.IO.SimpleSkin;
using Fantome.Libraries.League.IO.SkeletonFile;
using Fantome.Libraries.League.IO.StaticObject;
using Fantome.Libraries.League.IO.WAD;
using Obsidian.MVVM.ViewModels.WAD;
using SharpGLTF.Schema2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Obsidian.Utilities
{
    public static class FileConversionHelper
    {
        public static FileConversionOptions GetFileConversionOptions(LeagueFileType fileType)
        {
            if (fileType == LeagueFileType.SKN)
            {
                return new FileConversionOptions(new List<FileConversion>
                {
                    new FileConversion("glTF", ".glb", null, ConvertSimpleSkinToGltf),
                    new FileConversion("glTF (with Skeleton)", ".glb", ConstructSimpleSkinWithSkeletonParameter, ConvertSimpleSkinWithSkeletonToGltf)
                });
            }
            else if (fileType == LeagueFileType.SCB)
            {
                return new FileConversionOptions(new List<FileConversion>()
                {
                    new FileConversion("glTF", ".glb", null, ConvertScbToGltf),
                    new FileConversion("OBJ", ".obj", null, ConvertScbToObj)
                });
            }
            else if (fileType == LeagueFileType.SCO)
            {
                return new FileConversionOptions(new List<FileConversion>()
                {
                    new FileConversion("glTF", ".glb", null, ConvertScoToGltf),
                    new FileConversion("OBJ", ".obj", null, ConvertScoToObj)
                });
            }
            else if (fileType == LeagueFileType.MAPGEO)
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
            WADEntry simpleSkinWadEntry = parameter.Parameter;
            using MemoryStream stream = new MemoryStream(simpleSkinWadEntry.GetContent(true));
            SimpleSkin simpleSkin = new SimpleSkin(stream);
            ModelRoot gltf = simpleSkin.ToGltf();

            gltf.SaveGLB(Path.ChangeExtension(parameter.OutputPath, "glb"));
        }
        private static void ConvertSimpleSkinWithSkeletonToGltf(FileConversionParameter parameter)
        {
            WADEntry simpleSkinWadEntry = parameter.Parameter;
            WADEntry skeletonWadEntry = parameter.AdditionalParameters.FirstOrDefault(x => x.Item1 == FileConversionAdditionalParameterType.Skeleton).Item2;

            using MemoryStream simpleSkinStream = new MemoryStream(simpleSkinWadEntry.GetContent(true));
            using MemoryStream skeletonStream = new MemoryStream(skeletonWadEntry.GetContent(true));

            SimpleSkin simpleSkin = new SimpleSkin(simpleSkinStream);
            Skeleton skeleton = new Skeleton(skeletonStream);

            ModelRoot gltf = simpleSkin.ToGltf(skeleton);

            gltf.SaveGLB(Path.ChangeExtension(parameter.OutputPath, "glb"));
        }

        private static void ConvertScbToGltf(FileConversionParameter parameter)
        {
            WADEntry staticObjectWadEntry = parameter.Parameter;
            using MemoryStream stream = new MemoryStream(staticObjectWadEntry.GetContent(true));
            StaticObject staticObject = StaticObject.ReadSCB(stream);
            ModelRoot gltf = staticObject.ToGltf();

            gltf.SaveGLB(Path.ChangeExtension(parameter.OutputPath, "glb"));
        }
        private static void ConvertScbToObj(FileConversionParameter parameter)
        {
            WADEntry staticObjectWadEntry = parameter.Parameter;
            using MemoryStream stream = new MemoryStream(staticObjectWadEntry.GetContent(true));
            StaticObject staticObject = StaticObject.ReadSCB(stream);
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
            WADEntry staticObjectWadEntry = parameter.Parameter;
            using MemoryStream stream = new MemoryStream(staticObjectWadEntry.GetContent(true));
            StaticObject staticObject = StaticObject.ReadSCO(stream);
            ModelRoot gltf = staticObject.ToGltf();

            gltf.SaveGLB(Path.ChangeExtension(parameter.OutputPath, "glb"));
        }
        private static void ConvertScoToObj(FileConversionParameter parameter)
        {
            WADEntry staticObjectWadEntry = parameter.Parameter;
            using MemoryStream stream = new MemoryStream(staticObjectWadEntry.GetContent(true));
            StaticObject staticObject = StaticObject.ReadSCO(stream);
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
            WADEntry mapGeometryWadEntry = parameter.Parameter;
            using MemoryStream stream = new MemoryStream(mapGeometryWadEntry.GetContent(true));
            MapGeometry mapGeometry = new MapGeometry(stream);
            ModelRoot gltf = mapGeometry.ToGLTF();

            gltf.SaveGLB(Path.ChangeExtension(parameter.OutputPath, "glb"));
        }

        private static FileConversionParameter ConstructSimpleSkinWithSkeletonParameter(string outputPath, WadFileViewModel parameter, WadViewModel wad)
        {
            // We need to find a skeleton file with the same filename as the Simple Skin
            string skeletonPath = Path.ChangeExtension(parameter.Path, "skl");
            WADEntry skeletonWadEntry = wad.GetAllFiles().FirstOrDefault(x => x.Path == skeletonPath).Entry;
            if (skeletonWadEntry is null)
            {
                throw new Exception(Localization.Get("ConversionSimpleSkinWithSkeletonSkeletonNotFound"));
            }
            else
            {
                return new FileConversionParameter(outputPath, parameter.Entry, new List<(FileConversionAdditionalParameterType, WADEntry)>()
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
        public WADEntry Parameter { get; }
        public List<(FileConversionAdditionalParameterType, WADEntry)> AdditionalParameters { get; set; }

        public FileConversionParameter(string outputPath, WADEntry parameter)
            : this(outputPath, parameter, new List<(FileConversionAdditionalParameterType, WADEntry)>())
        {

        }
        public FileConversionParameter(
            string outputPath,
            WADEntry parameter,
            List<(FileConversionAdditionalParameterType, WADEntry)> additionalParameters)
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
