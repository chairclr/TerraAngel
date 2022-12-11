using System;

namespace TerraAngel.Config;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class DefaultConfigValueAttribute : Attribute
{
    public readonly string FieldName;

    public DefaultConfigValueAttribute(string fieldName)
    {
        FieldName = fieldName;
    }
}
