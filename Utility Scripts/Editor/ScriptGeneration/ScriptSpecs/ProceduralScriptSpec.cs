using System.Collections.Generic;
namespace ShoelaceStudios.Utilities.Editor.ScriptGeneration
{
    public class ProceduralScriptSpec
    {
        public string Attribute;
        public string Namespace;
        public string ClassName;
        public string BaseClass;
        public List<string> Usings = new List<string>();
        public List<string> Interfaces = new List<string>();
        public List<RegionSpec> Regions = new();

        public bool IsAbstract;
        public bool IsPartial;
        

        public void AddInterface(string iname)
        {
            if (!Interfaces.Contains(iname))
            {
                Interfaces.Add(iname);
            }
        }
        public void AddUsing(string usingName)
        {
            if (!Usings.Contains(usingName))
            {
                Usings.Add(usingName);
            }
        }
    }
}

