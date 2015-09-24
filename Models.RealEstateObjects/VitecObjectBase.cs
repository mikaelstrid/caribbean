using System;

namespace Caribbean.Models.RealEstateObjects
{
    public abstract class VitecObjectBase
    {
        public string Id { get; set; }
        public string Address { get; set; }
        public int? Price { get; set; }
        public DateTime ModifiedTime { get; set; }
    }
}