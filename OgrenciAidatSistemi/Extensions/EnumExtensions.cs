using System.ComponentModel;
using System.Reflection;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        Type t = value.GetType();
        if (!t.IsEnum && t == null)
            throw new ArgumentException("EnumerationValue must be of Enum type", "value");
        string strvalue = value.ToString();
        if (string.IsNullOrEmpty(strvalue))
            return string.Empty;

        FieldInfo? field = t.GetField(strvalue);
        if (field == null)
            return string.Empty;

        DescriptionAttribute? attribute = (DescriptionAttribute?)
            Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));

        return attribute == null ? value.ToString() : attribute.Description;
    }
}
