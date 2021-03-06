namespace Caribbean.Models.RealEstateObjects
{
    public abstract class RealEstateImageBase
    {
        public abstract string GetImageUrl(int? width = 10000, int? height = null);
        public abstract string GetThumbnailUrl(int? width = null, int? height = null);
    }
}