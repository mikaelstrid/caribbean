using System;
using Caribbean.Models.RealEstateObjects;

namespace Caribbean.Aruba.Web.ViewModels.RealEstateObject
{
    public class ObjectSummaryViewModel
    {
        public string Id { get; set; }
        public string Address { get; set; }
        //public ObjectStatus Status { get; set; }
        public string ThumbailUrl { get; set; }
        public int? Price { get; set; }
        public DateTime CreatedTime { get; set; }
        public string CityArea { get; set; }
        public int? NumberOfRooms { get; set; }
        public int? SquareMeters { get; set; }
        public ObjectStatus Status { get; set; }
    }
}