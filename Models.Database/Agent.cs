using System.Collections.Generic;

namespace Caribbean.Models.Database
{
    public class Agent
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int AgencyId { get; set; }

        public virtual Agency Agency { get; set; }
        public virtual ICollection<Print> Prints { get; set; }
    }
}