using System.Collections.Generic;
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

        public ScriptBuilder SetAbstract(bool isAbstract)
        {
            spec.IsAbstract = isAbstract;
            return this;
        }

        public ScriptBuilder SetPartial(bool isPartial)
        {
            spec.IsPartial = isPartial;
            return this;
        }

        public ScriptBuilder Using(string usingName)
        {
            spec.AddUsing(usingName);
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

            //Usings
            foreach (string use in script.Usings)
            {
                writer.WriteLine($"using {use};");
            }

            if (!string.IsNullOrEmpty(script.Namespace))
            {
                //Namespace
                writer.WriteLine($"namespace {script.Namespace}");
                writer.WriteLine("{");
                writer.Indent();
            }

            if (!string.IsNullOrEmpty(script.Attribute))
            {
                writer.WriteLine("[" + script.Attribute + "]");
            }

            List<string> declarationFront = new List<string>
            {
                "public"
            };

            if (script.IsPartial)
            {
                declarationFront.Add("partial");
            }

            if (script.IsAbstract)
            {
                declarationFront.Add("abstract");
            }
            
            declarationFront.Add("class");
            
            string classFront = string.Join(" ", declarationFront);
            
            //Class Declaration
            bool hasBaseClass = !string.IsNullOrEmpty(script.BaseClass);
            bool hasInterfaces = script.Interfaces.Count > 0;
            if (!hasBaseClass && !hasInterfaces)
            {
                writer.WriteLine($"{classFront} {script.ClassName}");
            }
            else
            {
                string interfaces = hasInterfaces ? string.Join(", ", script.Interfaces) : "";
                if (hasBaseClass)
                {
                    if (hasInterfaces)
                    {
                        writer.WriteLine($"{classFront} {script.ClassName} : {script.BaseClass}, {interfaces}");
                    }
                    else
                    {
                        writer.WriteLine($"{classFront} {script.ClassName} : {script.BaseClass}");
                    }
                }
                else
                {
                    writer.WriteLine($"{classFront} {script.ClassName} : {interfaces}");
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
                    if (region.Fields.Count > 1)
                        writer.WriteLine("// Fields");
                    foreach (FieldSpec field in region.Fields)
                    {
                        writer.WriteLine(field.GetCodeString());
                    }
                }

                if (region.Enums.Count > 0)
                {
                    if (region.Enums.Count > 1)
                        writer.WriteLine("// Enums");
                    foreach (EnumSpec enumSpec in region.Enums)
                    {
                        writer.WriteLine(enumSpec.GetCodeString());
                    }
                }

                if (region.Properties.Count > 0 || region.Accessors.Count > 0)
                {
                    if (region.Properties.Count > 1 || region.Accessors.Count > 1)
                        writer.WriteLine("// Properties");
                    foreach (PropertySpec property in region.Properties)
                    {
                        writer.WriteLine(property.GetCodeString());
                    }

                    foreach (AccessorSpec accessor in region.Accessors)
                    {
                        writer.WriteLine(accessor.GetCodeString());
                    }
                }


                if (region.Events.Count > 0)
                {
                    if (region.Events.Count > 1)
                        writer.WriteLine("// Events");
                    foreach (EventSpec rEvent in region.Events)
                    {
                        writer.WriteLine(rEvent.GetCodeString());
                    }
                }


                if (region.Methods.Count > 0)
                {
                    if (region.Methods.Count > 1)
                        writer.WriteLine("// Methods");
                    foreach (MethodSpec method in region.Methods)
                    {
                        if (!string.IsNullOrEmpty(method.Comments))
                        {
                            writer.WriteLine("/// " + method.Comments);
                        }

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
            
            writer.WriteLine("}");
            writer.Unindent();

            if (!string.IsNullOrEmpty(script.Namespace))
            {
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

                if (!region.Validate(out string error))
                    throw new System.InvalidOperationException("Region Specs invalid :: " + error);
            }
        }
    }
}