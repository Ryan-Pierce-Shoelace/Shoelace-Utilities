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
        public string Attribute;

        public bool Validate()
        {
            return !string.IsNullOrEmpty(Type)
                   && !string.IsNullOrEmpty(Name);
        }

        // Return single-line field declaration
        public string GetCodeString()
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(Attribute))
            {
                parts.Add($"[{Attribute}]");
            }
            parts.Add(Access.ToKeyword());
            
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
            {
                accessorParts.Add(GetterAccess == Access ? "get;" : $"{GetterAccess.ToKeyword()} get;");
            }
            if (HasSetter)
            {
                accessorParts.Add(SetterAccess == Access ? "set;" : $"{SetterAccess.ToKeyword()} set;");
            }
            
            string accessors = string.Join(" ", accessorParts);

            return $"{access} {type} {name} {{ {accessors} }}";
        }
    }

    public class AccessorSpec
    {
        public string Type;
        public string Name;
        public string Target;

        public bool Validate()
        {
            return !string.IsNullOrEmpty(Type)
                && !string.IsNullOrEmpty(Name)
                && !string.IsNullOrEmpty(Target);
        }

        public string GetCodeString()
        {
            return $"public {Type} {Name} => {Target};";
        }
    }

    public class EnumSpec
    {
        public string Name;
        public List<string> Types;
        public Access Access = Access.Public;

        public bool Validate()
        {
            return Types is { Count: > 0 } && !string.IsNullOrEmpty(Name);
        }

        public string GetCodeString()
        {
            string enumDeclaration = $"{Access.ToKeyword()} enum {Name} ";

            enumDeclaration += "{";
            enumDeclaration += string.Join(", ", Types);
            enumDeclaration += "}";

            return enumDeclaration;
        }
    }
}