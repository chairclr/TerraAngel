using System;

namespace TerraAngel.Client.Config;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class DefaultConfigValueAttribute : Attribute
{
    public readonly string FieldName;

    public DefaultConfigValueAttribute(string fieldName)
    {
        FieldName = fieldName;
    }
}
