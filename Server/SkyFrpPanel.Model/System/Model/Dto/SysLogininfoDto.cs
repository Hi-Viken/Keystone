namespace SkyFrpPanel.Model.System.Dto
{
    public class SysLogininfoQueryDto : PagerInfo
    {
        public string Status { get; set; }
        public long? UserId { get; set; }
        public string Ipaddr { get; set; } = string.Empty; 
        public string UserName { get; set; }
        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
