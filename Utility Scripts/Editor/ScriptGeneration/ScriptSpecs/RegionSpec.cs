using System.Collections.Generic;

namespace ShoelaceStudios.Utilities.Editor.ScriptGeneration
{
    public class RegionSpec
    {
        public string Name;
        public List<FieldSpec> Fields = new();
        public List<PropertySpec> Properties = new();
        public List<EventSpec> Events = new();
        public List<MethodSpec> Methods = new();

        public bool Validate()
        {
            if (Fields.Count > 0 || Events.Count > 0 || Methods.Count > 0)
            {
                for (int i = 0; i < Fields.Count; i++)
                {
                    if(!Fields[i].Validate())
                        return false;
                }
                for (int i = 0; i < Properties.Count; i++)
                {
                    if(!Properties[i].Validate())
                        return false;
                }
                for (int i = 0; i < Events.Count; i++)
                {
                    if(!Events[i].Validate())
                        return false;
                }
                for (int i = 0; i < Methods.Count; i++)
                {
                    if(!Methods[i].Validate())
                        return false;
                }

                return true;
            }

            return false;
        }
    }
}