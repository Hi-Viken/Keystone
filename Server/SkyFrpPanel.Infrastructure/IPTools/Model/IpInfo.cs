namespace SkyFrpPanel.Infrastructure.IPTools.Model
{
    public class IpInfo
    {
        public string IpAddress { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        //public string CountryCode { get; set; }

        public string Province { get; set; } = string.Empty;
        //public string ProvinceCode { get; set; }

        public string City { get; set; } = string.Empty;

        //public string PostCode { get; set; }

        public string NetworkOperator { get; set; } = string.Empty;

        //public double? Latitude { get; set; } = 0d;

        //public double? Longitude { get; set; } = 0d;
        //public int? AccuracyRadius { get; set; }
    }
}
