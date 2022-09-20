using System.ComponentModel;

namespace Core.Enums
{
    public enum TableStatus
    {
        [Description("Reserved")]
        Reserved = -1,
        [Description("Available")]
        Available = 0,
        [Description("Occupied")]
        Occupied = 1
    }
}
