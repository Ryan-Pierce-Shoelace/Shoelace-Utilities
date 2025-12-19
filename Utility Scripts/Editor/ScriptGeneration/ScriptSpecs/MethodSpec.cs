using System;
using System.Collections.Generic;
using System.Linq;

namespace ShoelaceStudios.Utilities.Editor.ScriptGeneration
{
    public class MethodSpec
    {
        public Access Access = Access.Private;
        public string ReturnType;
        public string Signature;
        public List<ParamSpec> Parameters = new();

        public List<string> BodyLines = new();

        public bool Validate()
        {
            if(string.IsNullOrEmpty(ReturnType))
                return false;
            if (string.IsNullOrEmpty(Signature))
                return false;

            foreach (var p in Parameters)
            {
                if(!p.Validate())
                    return false;
            }
            
            return true;
        }

        public string GetDeclarationLine()
        {
            string parameters = "";
            if (Parameters.Count > 0)
            {
                IEnumerable<ParamSpec> orderedParams = Parameters
                    .OrderBy(p => !string.IsNullOrEmpty(p.DefaultValue));
                
                List<string> parameterStrings = new();
                foreach (ParamSpec parm in orderedParams)
                {
                    parameterStrings.Add(parm.GetDeclarationString());
                }
                parameters = string.Join(", ", parameterStrings);
            }
            return $"{Access.ToKeyword()} {ReturnType} {Signature} ({parameters})";
        }
    }
}