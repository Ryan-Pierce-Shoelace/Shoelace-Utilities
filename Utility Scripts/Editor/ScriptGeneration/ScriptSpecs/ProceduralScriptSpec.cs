using System.Collections.Generic;
namespace ShoelaceStudios.Utilities.Editor.ScriptGeneration
{
    public class ProceduralScriptSpec
    {
        public string Namespace;
        public string ClassName;
        public string BaseClass;
        public List<string> Interfaces = new List<string>();
        public List<RegionSpec> Regions = new();
        public void AddInterface(string iname)
        {
            if (!Interfaces.Contains(iname))
            {
                Interfaces.Add(iname);
            }
        }
    }
}

