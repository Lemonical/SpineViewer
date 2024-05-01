using System.Runtime.Serialization;

namespace SpineViewer;

public enum SpineVersion
{
    [EnumMember(Value = "3.8.95")]
    Spine38,
    [EnumMember(Value = "4.1.00")]
    Spine41
}
