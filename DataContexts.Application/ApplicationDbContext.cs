using System.Data.Entity;
using Caribbean.Models.Database;
using Caribbean.Models.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Caribbean.DataContexts.Application
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Agent> Agents { get; set; }
        public DbSet<Print> Prints { get; set; }
        public DbSet<PageTemplatePlaceholderMapping> PageTemplatePlaceholderMappings { get; set; }
    }
}