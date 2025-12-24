using System.Collections.Generic;

namespace ShoelaceStudios.Utilities.Editor.ScriptGeneration
{
    public class RegionBuilder
    {
        private readonly RegionSpec region;

        internal RegionBuilder(RegionSpec region)
        {
            this.region = region;
        }

        public RegionBuilder Field(
            string type,
            string name,
            Access access = Access.Private,
            string attribute = null,
            bool isReadonly = false,
            string initializer = null
            )
        {
            region.Fields.Add(new FieldSpec
            {
                Attribute = attribute,
                Type = type,
                Name = name,
                Access = access,
                IsReadonly = isReadonly,
                Initializer = initializer
            });

            return this;
        }
        public RegionBuilder Property(
            string type,
            string name,
            Access access = Access.Public,
            Access setterAccess = Access.Private)
        {
            region.Properties.Add(new PropertySpec
            {
                Type = type,
                Name = name,
                Access = access,
                HasSetter = true,
                SetterAccess = setterAccess
            });

            return this;
        }

        public RegionBuilder Accessor(string type, string name, string target)
        {
            region.Accessors.Add(new AccessorSpec
            {
                Type = type,
                Name = name,
                Target = target
            });
            return this;
        }
        
        public RegionBuilder Event(string type, string name, Access access = Access.Public)
        {
            region.Events.Add(new EventSpec
            {
                Type = type,
                Name = name,
                Access = access
            });
            return this;
        }
        
        public RegionBuilder UnityEvent(string name, Access access = Access.Public)
        {
            region.Events.Add(new EventSpec
            {
                Type = "UnityEngine.Events.UnityAction",
                Name = name,
                Access = access,
                DefaultAssignment = "delegate { }"
            });
            return this;
        }
        public RegionBuilder UnityEvent(string name, string type, Access access = Access.Public)
        {
            region.Events.Add(new EventSpec
            {
                Type = $"UnityEngine.Events.UnityAction<{type}>",
                Name = name,
                Access = access,
                DefaultAssignment = "delegate { }"
            });
            return this;
        }
        public RegionBuilder CustomEvent(string type, string name, string defaultAssignment = null, Access access = Access.Public)
        {
            region.Events.Add(new EventSpec
            {
                Type = type,
                Name = name,
                Access = access,
                DefaultAssignment = defaultAssignment
            });
            return this;
        }

        public RegionBuilder Method(string returnType, string signature, Access access = Access.Private,
            System.Action<MethodBuilder> build = null)
        {
            var method = new MethodSpec
            {
                ReturnType = returnType,
                Signature = signature,
                Access = access
            };

            if (build != null)
            {
                var mb = new MethodBuilder(method);
                build(mb);
            }

            region.Methods.Add(method);
            return this;
        }

        public void Enum(string name, List<string> types, Access access = Access.Public)
        {
            region.Enums.Add(
                new EnumSpec
                {
                    Access = access,
                    Name = name,
                    Types = types
                });
        }
    }
}