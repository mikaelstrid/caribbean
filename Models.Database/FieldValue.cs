namespace Caribbean.Models.Database
{
    public class FieldValue
    {
        public int Id { get; set; }
        public string Value { get; set; }

        public string FieldName { get; set; }
        public int PageId { get; set; }
    }
}