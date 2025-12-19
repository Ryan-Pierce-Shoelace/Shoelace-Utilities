namespace ShoelaceStudios.Utilities.Editor.ScriptGeneration
{
    public enum ParamModifier
    {
        None,
        Ref,
        Out,
        In
    }

    public class ParamSpec
    {
        public string Type;
        public string Name;
        public ParamModifier Modifier = ParamModifier.None;
        public string DefaultValue; // optional

        public ParamSpec(string type, string name)
        {
            Type = type;
            Name = name;
        }
        public bool Validate()
        {
            return !string.IsNullOrEmpty(Type) && !string.IsNullOrEmpty(Name);
        }
        public string GetDeclarationString()
        {
            if (!Validate()) return string.Empty;

            string modifierString = Modifier != ParamModifier.None ? Modifier.ToString().ToLower() + " " : "";
            string defaultString = !string.IsNullOrEmpty(DefaultValue) ? $" = {DefaultValue}" : "";

            return $"{modifierString}{Type} {Name}{defaultString}";
        }
    }
}