using Nuke.Common.Tooling;
using System;
using System.ComponentModel;
using System.Linq;

[TypeConverter(typeof(TypeConverter<Configuration>))]
public class Configuration : Enumeration {
    public static Configuration Debug = new Configuration { Value = nameof(Debug) };
    public static Configuration Release = new Configuration { Value = nameof(Release) };

    public static implicit operator string(Configuration configuration) {
        return configuration.Value;
    }
}