using System.Data;

namespace ShoelaceStudios.Utilities.Editor.ScriptGeneration
{
    public class MethodBuilder
    {
        private readonly MethodSpec method;

        internal MethodBuilder(MethodSpec method)
        {
            this.method = method;
        }

        public MethodBuilder Comment(string comment)
        {
            method.Comments = comment;
            return this;
        }
        public MethodBuilder Param(string type, string name)
        {
            method.Parameters.Add(new ParamSpec(type, name));
            return this;
        }
        public MethodBuilder Line(string code)
        {
            method.BodyLines.Add(code);
            return this;
        }
        public MethodBuilder BlankLine()
        {
            method.BodyLines.Add(string.Empty);
            return this;
        }
        public MethodBuilder Block(string header, System.Action<MethodBuilder> inner, string comment = "")
        {
            method.BodyLines.Add($"// {comment}");
            method.BodyLines.Add($"{header}");
            method.BodyLines.Add("{");

            inner(this);

            method.BodyLines.Add("}");
            return this;
        }
        
        public MethodBuilder If(string condition, System.Action<MethodBuilder> body, string comment = "")
        {
            return Block($"if ({condition})", body, comment);
        }

        public MethodBuilder ForEach(string var, string collection, System.Action<MethodBuilder> body, string comment = "")
        {
            return Block($"foreach (var {var} in {collection})", body, comment);
        }

        public MethodBuilder Try(System.Action<MethodBuilder> body, System.Action<MethodBuilder> @catch = null, string tryComment = "", string catchComment = "")
        {
            Block("try", body, tryComment);

            if (@catch != null)
                Block("catch (System.Exception e)", @catch, catchComment);

            return this;
        }
        public MethodBuilder Switch(string expression, System.Action<SwitchBuilder> body, string comment = "")
        {
            if (!string.IsNullOrEmpty(comment))
                method.BodyLines.Add($"// {comment}");

            method.BodyLines.Add($"switch ({expression})");
            method.BodyLines.Add("{");

            var switchBuilder = new SwitchBuilder(this);
            body?.Invoke(switchBuilder);

            method.BodyLines.Add("}");
            return this;
        }
        
        public class SwitchBuilder
        {
            private readonly MethodBuilder method;

            internal SwitchBuilder(MethodBuilder method)
            {
                this.method = method;
            }

            public SwitchBuilder Case(
                string value,
                System.Action<MethodBuilder> body,
                string comment = "",
                string terminator = null)
            {
                if (!string.IsNullOrEmpty(comment))
                    method.Line($"    // {comment}");

                method.Line($"    case {value}:");

                body?.Invoke(method);

                method.Line($"        {terminator ?? "break;"}");

                return this;
            }

            public SwitchBuilder Default(
                System.Action<MethodBuilder> body,
                string comment = "",
                string terminator = null)
            {
                if (!string.IsNullOrEmpty(comment))
                    method.Line($"    // {comment}");

                method.Line("    default:");

                body?.Invoke(method);

                method.Line($"        {terminator ?? "break;"}");

                return this;
            }
        }
    }
}