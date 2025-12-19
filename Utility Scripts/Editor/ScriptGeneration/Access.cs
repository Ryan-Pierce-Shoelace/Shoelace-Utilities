namespace ShoelaceStudios.Utilities.Editor.ScriptGeneration
{
    public enum Access
    {
        Public,
        Private,
        Protected
    };
    public static class AccessExtensions
    {
        public static string ToKeyword(this Access access)
        {
            return access switch
            {
                Access.Private => "private",
                Access.Protected => "protected",
                _ => "public"
            };
        }
    }
}