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

        public ScriptBuilder Attribute(string attribute)
        {
            spec.Attribute = attribute;
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

        public ScriptBuilder Using(string usingName)
        {
            spec.Usings.Add(usingName);
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
            string directoryPath = Path.Combine(Application.dataPath, filePath, subfolder);

            Directory.CreateDirectory(directoryPath);

            string savePath = Path.Combine(directoryPath, $"{script.ClassName}.cs");

            CodeWriter writer = new(savePath);
            if (!string.IsNullOrEmpty(script.Namespace))
            {
                //Usings
                foreach (string use in script.Usings)
                {
                    writer.WriteLine($"using {use};");
                }
                //Namespace
                writer.WriteLine($"namespace {script.Namespace}");
                writer.WriteLine("{");
                writer.Indent();

                if (!string.IsNullOrEmpty(script.Attribute))
                {
                    writer.WriteLine("["+script.Attribute+"]");
                }
                
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
                        if (hasInterfaces)
                        {
                            writer.WriteLine($"public class {script.ClassName} : {script.BaseClass}, {interfaces}");
                        }
                        else
                        {
                            writer.WriteLine($"public class {script.ClassName} : {script.BaseClass}");
                        }
                    }
                    else
                    {
                        writer.WriteLine($"public class {script.ClassName} : {interfaces}");
                    }
                }

                writer.WriteLine("{");
                writer.Indent();

                //Regions
                foreach (RegionSpec region in script.Regions)
                {
                    writer.WriteLine($"#region {region.Name}");
                    writer.Indent();

                    if (region.Fields.Count > 0)
                    {
                        if(region.Fields.Count > 1)
                            writer.WriteLine("// Fields");
                        foreach (FieldSpec field in region.Fields)
                        {
                            writer.WriteLine(field.GetCodeString());
                        } 
                    }


                    if (region.Properties.Count > 0)
                    {
                        if(region.Properties.Count > 1)
                            writer.WriteLine("// Properties");
                        foreach (PropertySpec property in region.Properties)
                        {
                            writer.WriteLine(property.GetCodeString());
                        }
                    }


                    if (region.Events.Count > 0)
                    {
                        if(region.Events.Count > 1)
                            writer.WriteLine("// Events");
                        foreach (EventSpec rEvent in region.Events)
                        {
                            writer.WriteLine(rEvent.GetCodeString());
                        }
                    }


                    if (region.Methods.Count > 0)
                    {
                        if(region.Methods.Count > 1)
                            writer.WriteLine("// Methods");
                        foreach (MethodSpec method in region.Methods)
                        {
                            writer.WriteLine();
                            writer.WriteLine("/// " + method.Comments);
                            writer.WriteLine(method.GetDeclarationLine());
                            writer.WriteLine("{");
                            writer.Indent();
                            foreach (string line in method.BodyLines)
                            {
                                writer.WriteLine(line);
                            }

                            writer.Unindent();
                            writer.WriteLine("}");
                        }
                    }
                    
                    writer.Unindent();
                    writer.WriteLine("#endregion");
                }

                writer.Unindent();
                writer.WriteLine("}");
                writer.Unindent();
                writer.WriteLine("}");
            }
            writer.Dispose();
            
            #if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
            #endif
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