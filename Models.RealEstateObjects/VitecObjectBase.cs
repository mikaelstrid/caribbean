using System;

namespace Caribbean.Models.RealEstateObjects
{
    public abstract class VitecObjectBase
    {
        public string Id { get; set; }
        public string Address { get; set; }
        public int? Price { get; set; }
        public DateTime CreatedTime { get; set; }
        public string CityArea { get; set; }
        public int? SquareMeters { get; set; }
        public double? NumberOfRooms { get; set; }
    }
}