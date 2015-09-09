using System.Collections.Generic;
using Caribbean.Models.Database;
using Caribbean.Models.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Caribbean.DataContexts.Application.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Caribbean.DataContexts.Application.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            // Make sure all roles are seeded in the database
            //var roleStore = new RoleStore<IdentityRole>(context);
            //var roleManager = new RoleManager<IdentityRole>(roleStore);

            //if (!context.Roles.Any(r => r.Name == "Users"))
            //{
            //    roleManager.Create(new IdentityRole { Name = "Users" });
            //}

            // Make sure all users are seeded in the database
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            if (!context.Users.Any(u => u.UserName == "mikael@pixeldigitalbyra.se"))
            {
                var user1 = AddUser(userManager, "mikael@pixeldigitalbyra.se", "Pixel123!");
                var user2 = AddUser(userManager, "per.rosenberg@kustreklam.se", "Kust123!");

                var agency = new Agency { Slug = "fastighetsbyran-storangsgatan", VitecCustomerId = "26301" };

                context.Agents.Add(new Agent { Agency = agency, UserId = user1.Id });
                context.Agents.Add(new Agent { Agency = agency, UserId = user2.Id });

                context.SaveChanges();
            }

            if (!context.PageTemplatePlaceholderMappings.Any())
            {
                var mappings = new List<Tuple<string, string>>
                    {
                        new Tuple<string, string>("objekt_gata", "/OBJEKT/msadress"),
                        new Tuple<string, string>("objekt_rum", "/OBJEKT/rum"),
                        new Tuple<string, string>("objekt_storlek", "/OBJEKT/boarea"),
                        new Tuple<string, string>("kontorets_adress", "/OBJEKT/Firma/faBesokAdr"),
                        new Tuple<string, string>("kontorets_tel", "/OBJEKT/Firma/faTel1"),
                        new Tuple<string, string>("kontorets_www", "/OBJEKT/Firma/faHemsida"),
                        new Tuple<string, string>("ansvarig_maklare", "/OBJEKT/Maklare/maNamn"),
                        new Tuple<string, string>("ansvarig_maklare_tel", "/OBJEKT/Maklare/maDirektnr"),
                        new Tuple<string, string>("ansvarig_maklare_mobil", "/OBJEKT/Maklare/maMobilnr"),
                        new Tuple<string, string>("ansvarig_maklare_mail", "/OBJEKT/Maklare/maEmail"),
                        new Tuple<string, string>("extra_kontaktperson", "/OBJEKT/Extrakontaktperson/ekNamn"),
                        new Tuple<string, string>("extra_kontaktperson_tel", "/OBJEKT/Extrakontaktperson/ekDirektnr"),
                        new Tuple<string, string>("extra_kontaktperson_mobil", "/OBJEKT/Extrakontaktperson/ekMobilnr"),
                        new Tuple<string, string>("fritext", "/OBJEKT/beskrivning"),
                        new Tuple<string, string>("2_3", "/OBJEKT/pictures/picture[1]"),
                        new Tuple<string, string>("3_2", "/OBJEKT/pictures/picture[2]")
                    };

                foreach (var t in mappings)
                {
                    context.PageTemplatePlaceholderMappings.Add(new PageTemplatePlaceholderMapping
                    {
                        TemplateType = "VitecMäklarsystem",
                        TemplateVersion = "1.0",
                        Name = t.Item1,
                        Path = t.Item2
                    });
                }

                context.SaveChanges();
            }

            //// Make sure all user-role associations are seeded in the database
            //if (!userManager.IsInRole(context.Users.SingleOrDefault(u => u.UserName == "mikael@pixeldigitalbyra.se").Id, "Users"))
            //{
            //    AddUserToRoles(context, userManager, "mikael@pixeldigitalbyra.se", new[] { "Users" });
            //}
        }

        private static ApplicationUser AddUser(UserManager<ApplicationUser, string> userManager, string email, string password)
        {
            var user = new ApplicationUser { UserName = email, Email = email };
            userManager.Create(user, password);
            return user;
        }

        //private void AddUserToRoles(ApplicationDb context, UserManager<ApplicationUser> userManager, string userName, IEnumerable<string> roles)
        //{
        //    var user = context.Users.FirstOrDefault(u => u.UserName == userName);
        //    if (user != null)
        //    {
        //        foreach (var role in roles)
        //        {
        //            userManager.AddToRole(user.Id, role);
        //        }
        //    }
        //}
    }
}
