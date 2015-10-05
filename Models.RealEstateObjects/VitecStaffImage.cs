namespace Caribbean.Models.RealEstateObjects
{
    public class VitecStaffImage : RealEstateImageBase
    {
        public string ImageUrl { get; set; }

        public override string GetImageUrl(int? width = null, int? height = null)
        {
            return ImageUrl;
        }

        public override string GetThumbnailUrl(int? width = null, int? height = null)
        {
            return ImageUrl;
        }
    }
}
