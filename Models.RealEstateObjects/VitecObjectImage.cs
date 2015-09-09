namespace Caribbean.Models.RealEstateObjects
{
    public class VitecObjectImage
    {
        private string _imageUrlWithoutSizeParameters;
        private string _thumbnailBaseUrlWithoutSizeParameters;

        public string Pid { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public string Category { get; set; }

        public string ImageUrlWithoutSizeParameters
        {
            set { _imageUrlWithoutSizeParameters = value; }
        }

        public string GetImageUrl(int? width = 10000, int? height = null)
        {
            return BuildImageUrl(width, height, _imageUrlWithoutSizeParameters);
        }

        public string ThumbnailBaseUrlWithoutSizeParameters
        {
            set { _thumbnailBaseUrlWithoutSizeParameters = value; }
        }
        public string GetThumbnailUrl(int? width = null, int? height = null)
        {
            return BuildImageUrl(width, height, _thumbnailBaseUrlWithoutSizeParameters);
        }

        private static string BuildImageUrl(int? width, int? height, string baseUrl)
        {
            var url = baseUrl;
            if (width.HasValue) url += $"&sizex={width.Value}";
            if (height.HasValue) url += $"&sizey={height.Value}";
            return url;
        }
    }
}