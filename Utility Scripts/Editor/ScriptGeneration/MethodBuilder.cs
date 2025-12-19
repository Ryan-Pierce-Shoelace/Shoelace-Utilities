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
            var switchBuilder = new SwitchBuilder(this, expression);
            body.Invoke(switchBuilder);
            return this;
        }
        
        public class SwitchBuilder
        {
            private MethodBuilder method;
            private string switchExpression;

            public SwitchBuilder(MethodBuilder method, string expression)
            {
                this.method = method;
                this.switchExpression = expression;
                method.Line($"switch ({switchExpression})");
                method.Line("{");
            }

            public SwitchBuilder Case(string value, System.Action<MethodBuilder> body, string comment = "")
            {
                method.Line($"case {value}: {(string.IsNullOrEmpty(comment) ? "" : "// " + comment)}");
                method.Line("{");
                body?.Invoke(method);
                method.Line("    break;");
                method.Line("}");
                return this;
            }

            public SwitchBuilder Default(System.Action<MethodBuilder> body, string comment = "")
            {
                method.Line($"default: {(string.IsNullOrEmpty(comment) ? "" : "// " + comment)}");
                method.Line("{");
                body?.Invoke(method);
                method.Line("    break;");
                method.Line("}");
                return this;
            }
        }
    }
}