namespace ShoelaceStudios.Utilities.Editor.ScriptGeneration
{
    public class EventSpec
    {
        public Access Access = Access.Public;
        public string Name;
        public string Type; // C# type (e.g., UnityAction, Action<float>, etc.)
        public string DefaultAssignment; // optional initializer

        public bool Validate()
        {
            return !string.IsNullOrEmpty(Type) && !string.IsNullOrEmpty(Name);
        }

        public string GetCodeString()
        {
            string defaultAssignment = string.IsNullOrEmpty(DefaultAssignment) ? "" : " = " + DefaultAssignment;
            return $"{Access.ToKeyword()} event {Type} {Name}{defaultAssignment};";
        }
    }
}