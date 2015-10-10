using System;

namespace Caribbean.Aruba.SharedTypes
{
    public class PostAssetApiModel
    {
        public string AgentUserId { get; set; }
        public int PageId { get; set; }
        public Guid JobId { get; set; }
        public long JobDurationMs { get; set; }
        public string AssetName { get; set; }
        public string AssetUrl { get; set; }
    }
}