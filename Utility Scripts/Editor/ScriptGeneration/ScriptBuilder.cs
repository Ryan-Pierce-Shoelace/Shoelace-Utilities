using System.IO;
using UnityEngine;

namespace ShoelaceStudios.Utilities.Editor.ScriptGeneration
{
    public class ScriptBuilder
    {
        private readonly ProceduralScriptSpec spec = new();
        public ScriptBuilder NameSpace(string nameSpace)
        {
            spec.Namespace = nameSpace;
            return this;
        }

        public ScriptBuilder Class(string name, string baseClass = null)
        {
            spec.ClassName = name;
            spec.BaseClass = baseClass;
            return this;
        }
        public ScriptBuilder Interface(string iname)
        {
            spec.AddInterface(iname);
            return this;
        }
        public ScriptBuilder Region(string name, System.Action<RegionBuilder> build)
        {
            var region = new RegionSpec { Name = name };
            var builder = new RegionBuilder(region);
            build(builder);
            spec.Regions.Add(region);
            return this;
        }
        public ProceduralScriptSpec Build()
        {
            Validate(spec);
            return spec;
        }
        
        public void WriteScript(string filePath, ProceduralScriptSpec script, string subfolder = "")
        {
            using StreamWriter writer =
                new StreamWriter(Path.Combine(Application.dataPath, filePath, string.IsNullOrEmpty(subfolder) ? "" : $"/{subfolder}"));
            
            if (!string.IsNullOrEmpty(script.Namespace))
            {
                //Namespace
                writer.WriteLine($"namespace {script.Namespace}");
                writer.WriteLine("{");

                //Class Declaration
                bool hasBaseClass = !string.IsNullOrEmpty(script.BaseClass);
                bool hasInterfaces = script.Interfaces.Count > 0;
                if (!hasBaseClass && !hasInterfaces)
                {
                    writer.WriteLine($"public class {script.ClassName}");
                }
                else
                {
                    string interfaces = hasInterfaces ? string.Join(", ", script.Interfaces) : "";
                    if (hasBaseClass)
                    {
                        writer.WriteLine($"public class {script.ClassName} : {script.BaseClass}, {interfaces}");
                    }
                    else
                    {
                        writer.WriteLine($"public class {script.ClassName} : {interfaces}");
                    }
                }
                writer.WriteLine("{");
                
                
                //Regions
                foreach (RegionSpec region in script.Regions)
                {
                    writer.WriteLine($"#region {region.Name}");
                    
                    writer.WriteLine("// Fields");
                    foreach (FieldSpec field in region.Fields)
                    {
                        writer.WriteLine(field.GetCodeString());
                    }
                    writer.WriteLine("// Properties");
                    foreach (PropertySpec property in region.Properties)
                    {
                        writer.WriteLine(property.GetCodeString());
                    }
                    
                    writer.WriteLine("// Events");
                    foreach (EventSpec rEvent in region.Events)
                    {
                        writer.WriteLine(rEvent.GetCodeString());
                    }
                    
                    writer.WriteLine("// Methods");
                    foreach (MethodSpec method in region.Methods)
                    {
                        writer.WriteLine(method.GetDeclarationLine());
                        writer.WriteLine("{");
                        foreach (string line in method.BodyLines)
                        {
                            writer.WriteLine(line);
                        }
                        writer.WriteLine("}");
                    }
                    writer.WriteLine("#endregion");
                }
                writer.WriteLine("}");
                writer.WriteLine("}");
            }
        }

        private static void Validate(ProceduralScriptSpec spec)
        {
            if (string.IsNullOrEmpty(spec.ClassName))
                throw new System.InvalidOperationException("ClassName is required");

            foreach (var region in spec.Regions)
            {
                if (string.IsNullOrEmpty(region.Name))
                    throw new System.InvalidOperationException("Region name is required");

                if (!region.Validate())
                    throw new System.InvalidOperationException("Region Specs invalid");
            }
        }
    }
}