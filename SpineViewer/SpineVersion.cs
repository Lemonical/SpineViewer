using System.Runtime.Serialization;

namespace SpineViewer;

public enum SpineVersion
{
    [EnumMember(Value = "3.8")]
    Spine38,
    [EnumMember(Value = "4.1")]
    Spine41
}
