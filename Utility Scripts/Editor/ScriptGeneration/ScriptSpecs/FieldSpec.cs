using System.Collections.Generic;
using System.IO;

namespace ShoelaceStudios.Utilities.Editor.ScriptGeneration
{
    public class FieldSpec
    {
        public Access Access = Access.Private;
        public bool IsStatic;
        public bool IsReadonly;

        public string Type;
        public string Name;
        public string Initializer; // optional

        public bool Validate()
        {
            return !string.IsNullOrEmpty(Type)
                   && !string.IsNullOrEmpty(Name);
        }

        // Return single-line field declaration
        public string GetCodeString()
        {
            var parts = new List<string>
            {
                Access.ToKeyword()
            };

            if (IsStatic) parts.Add("static");
            if (IsReadonly) parts.Add("readonly");

            parts.Add(Type);
            parts.Add(Name);

            string code = string.Join(" ", parts);

            if (!string.IsNullOrEmpty(Initializer))
                code += $" = {Initializer}";

            return code + ";";
        }
    }

    public class PropertySpec
    {
        public Access Access = Access.Public;
        public string Type;
        public string Name;

        public bool HasGetter = true;
        public Access GetterAccess = Access.Public;

        public bool HasSetter;
        public Access SetterAccess = Access.Private;

        public bool Validate()
        {
            return !string.IsNullOrEmpty(Type)
                   && !string.IsNullOrEmpty(Name)
                   && (HasGetter || HasSetter);
        }

        public string GetCodeString()
        {
            if (!Validate()) return string.Empty;

            string access = Access.ToKeyword();
            string type = Type;
            string name = Name;

            List<string> accessorParts = new();

            if (HasGetter)
                accessorParts.Add($"{GetterAccess.ToKeyword()} get;");
            if (HasSetter)
                accessorParts.Add($"{SetterAccess.ToKeyword()} set;");

            string accessors = string.Join(" ", accessorParts);

            return $"{access} {type} {name} {{ {accessors} }}";
        }
    }
}