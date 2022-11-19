namespace Core.Enums
{
    public enum OrderDetailStatus
    {
        Reserved = -3,
        Overcharged = -2,
        Cancelled = -1,
        Received = 0,
        Processing = 1,
        Served = 2,
        ReadyToServe = 3
    }
}
