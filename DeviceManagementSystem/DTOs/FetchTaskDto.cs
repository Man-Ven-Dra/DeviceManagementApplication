namespace DeviceManagementAPI.DTO
{
    public class FetchTaskDto
    {
        public Guid DeviceId {get; set;}
        public string Operation {get; set;} = string.Empty;
        public string Username {get; set;} = string.Empty;
    }
}