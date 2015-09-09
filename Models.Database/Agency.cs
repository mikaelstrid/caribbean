using System.Collections.Generic;

namespace Caribbean.Models.Database
{
    public class Agency
    {
        public int Id { get; set; }
        public string VitecCustomerId { get; set; }
        public string Slug { get; set; }

        public virtual ICollection<Agent> Agents { get; set; }
    }
}