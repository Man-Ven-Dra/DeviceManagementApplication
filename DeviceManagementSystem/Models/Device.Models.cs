namespace DeviceManagementAPI.Models
{
    public class Device
    {
        public Guid Id { get; set; }
        public string DeviceName {get; set;}
        public string MAC { get; set; }
        public string IMEI { get; set; }
        public string IMSI { get; set; }
        public int Battery { get; set; }
        public string PlatformType { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}