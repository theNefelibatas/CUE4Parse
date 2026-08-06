using System.ComponentModel;

namespace CUE4Parse_Conversion.Options;

public enum EMaterialJsonFormat
{
    [Description("Properties JSON (WriteJson)")]
    Properties,
    [Description("Compact JSON (parameter view)")]
    Compact
}
