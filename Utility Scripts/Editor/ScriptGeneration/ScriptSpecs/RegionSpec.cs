using System.Collections.Generic;

namespace ShoelaceStudios.Utilities.Editor.ScriptGeneration
{
    public class RegionSpec
    {
        public string Name;
        public List<FieldSpec> Fields = new();
        public List<PropertySpec> Properties = new();
        public List<AccessorSpec> Accessors = new();
        public List<EventSpec> Events = new();
        public List<MethodSpec> Methods = new();
        public List<EnumSpec> Enums = new();

        public bool Validate(out string error)
        {
            error = string.Empty;

            // Nothing to validate
            if (Fields.Count == 0 &&
                Properties.Count == 0 &&
                Accessors.Count == 0 &&
                Events.Count == 0 &&
                Methods.Count == 0 &&
                Enums.Count == 0)
            {
                error = "Region Spec is empty";
                return false;
            }

            foreach (FieldSpec field in Fields)
            {
                if (!field.Validate())
                {
                    error = $"Error with field '{field.Name}'";
                    return false;
                }
            }

            foreach (PropertySpec property in Properties)
            {
                if (!property.Validate())
                {
                    error = $"Error with property '{property.Name}'";
                    return false;
                }
            }

            foreach (AccessorSpec accessor in Accessors)
            {
                if (!accessor.Validate())
                {
                    error = $"Error with accessor '{accessor.Name}'";
                    return false;
                }
            }

            foreach (EventSpec evt in Events)
            {
                if (!evt.Validate())
                {
                    error = $"Error with event '{evt.Name}'";
                    return false;
                }
            }

            foreach (EnumSpec enumSpec in Enums)
            {
                if (!enumSpec.Validate())
                {
                    error = $"Error with enum '{enumSpec.Name}'";
                    return false;
                }
            }

            foreach (MethodSpec method in Methods)
            {
                if (!method.Validate())
                {
                    error = $"Error with method '{method.Signature}'";
                    return false;
                }
            }

            return true;
        }
    }
}
