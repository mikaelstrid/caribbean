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

                var agency = new Agency { Slug = "fastighetsbyran-storangsgatan", VitecCustomerId = "22998" }; //26301=test

                context.Agents.Add(new Agent { Agency = agency, UserId = user1.Id });
                context.Agents.Add(new Agent { Agency = agency, UserId = user2.Id });

                context.SaveChanges();
            }

            if (!context.Users.Any(u => u.UserName == "bo@storangsgatan.se"))
            {
                var user1 = AddUser(userManager, "bo@storangsgatan.se", "boianggarden");
                var agency = context.Agencies.FirstOrDefault(a => a.Slug == "fastighetsbyran-storangsgatan");
                context.Agents.Add(new Agent { Agency = agency, UserId = user1.Id });
                context.SaveChanges();
            }

            if (!context.PageTemplatePlaceholderMappings.Any())
            {
                var mappings = ConfigurationDataGenerator.GetPlaceholderMappings();
                foreach (var t in mappings)
                {
                    context.PageTemplatePlaceholderMappings.Add(t);
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

    public static class ConfigurationDataGenerator
    {
        internal static IEnumerable<PageTemplatePlaceholderMapping> GetPlaceholderMappings()
        {
            var tuples = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("ansvarig_maklare", "/OBJEKT/Maklare/maNamn"),
                new Tuple<string, string>("ansvarig_maklare_tel", "/OBJEKT/Maklare/maDirektnr"),
                new Tuple<string, string>("ansvarig_maklare_mobil", "/OBJEKT/Maklare/maMobilnr"),
                new Tuple<string, string>("ansvarig_maklare_mail", "/OBJEKT/Maklare/maEmail"),
                new Tuple<string, string>("ansvarig_maklare_titel", "/OBJEKT/Maklare/maTitel"),
                new Tuple<string, string>("ansvarig_maklare_bild", "/OBJEKT/Maklare/maBildUrl"),
                new Tuple<string, string>("extra_kontaktperson", "/OBJEKT/Extrakontaktperson/ekNamn"),
                new Tuple<string, string>("extra_kontaktperson_tel", "/OBJEKT/Extrakontaktperson/ekDirektnr"),
                new Tuple<string, string>("extra_kontaktperson_mobil", "/OBJEKT/Extrakontaktperson/ekMobilnr"),
                new Tuple<string, string>("extra_kontaktperson_mail", "/OBJEKT/Extrakontaktperson/ekEmail"),
                new Tuple<string, string>("extra_kontaktperson_titel", "/OBJEKT/Extrakontaktperson/ekTitel"),
                new Tuple<string, string>("extra_kontaktperson_bild", "/OBJEKT/Extrakontaktperson/ek_BildUrl"),
                new Tuple<string, string>("kontorets_adress", "/OBJEKT/Firma/faBesokAdr"),
                new Tuple<string, string>("kontorets_postnummer", "/OBJEKT/Firma/faPostadress"),
                new Tuple<string, string>("kontorets_tel", "/OBJEKT/Firma/faTel1"),
                new Tuple<string, string>("kontorets_www", "/OBJEKT/Firma/faHemsida"),
                new Tuple<string, string>("foreningen_namn", "/OBJEKT/Forening/forNamn"),
                new Tuple<string, string>("foreningen_beskrivning", "/OBJEKT/Forening/forTxt"),
                new Tuple<string, string>("obj_typ", "/OBJEKT/objekttyptext"),
                new Tuple<string, string>("obj_fastighetsbeteckning", "/OBJEKT/fastbet"),
                new Tuple<string, string>("obj_upplatelseform", "/OBJEKT/upplatformtext"),
                new Tuple<string, string>("obj_boendeform", "/OBJEKT/objekttyptext"),
                new Tuple<string, string>("obj_LghRefNr", "/OBJEKT/lghrefnr"),
                new Tuple<string, string>("obj_gata", "/OBJEKT/msadress"),
                new Tuple<string, string>("obj_postadress", "/OBJEKT/adress2"),
                new Tuple<string, string>("obj_omrade", "/OBJEKT/omrade"),
                new Tuple<string, string>("obj_kommun", "/OBJEKT/kommun"),
                new Tuple<string, string>("obj_vaning", "/OBJEKT/vaning"),
                new Tuple<string, string>("obj_vaning_alla", "/OBJEKT/vaningav"),
                new Tuple<string, string>("obj_pris", "/OBJEKT/mspris"),
                new Tuple<string, string>("obj_pristext", "/OBJEKT/pristext"),
                new Tuple<string, string>("obj_driftskostnad", "/OBJEKT/driftskostnad"),
                new Tuple<string, string>("obj_hisstext", "/OBJEKT/hissfinns"),
                new Tuple<string, string>("obj_tomtbeskrivning", "/OBJEKT/ovrigt"),
                new Tuple<string, string>("obj_boarea", "/OBJEKT/boarea"),
                new Tuple<string, string>("obj_biarea", "/OBJEKT/biarea"),
                new Tuple<string, string>("obj_tomtarea", "/OBJEKT/tomtareal"),
                new Tuple<string, string>("obj_rum", "/OBJEKT/rum"),
                new Tuple<string, string>("obj_saljrubrik", "/OBJEKT/saljrubrik"),
                new Tuple<string, string>("obj_saljbeskrivning", "/OBJEKT/beskrivning"),
                new Tuple<string, string>("obj_avgift", "/OBJEKT/avgift"),
                new Tuple<string, string>("obj_avgift_tillagg", "/OBJEKT/avgifttext"),
                new Tuple<string, string>("obj_byggar", "/OBJEKT/byggnar"),
                new Tuple<string, string>("obj_byggar_tillagg", "/OBJEKT/byggnartext"),
                new Tuple<string, string>("huvudbild", "/OBJEKT/pictures/picture[1]"),
                //new Tuple<string, string>("objektbild", "/OBJEKT/pictures/picture[picgrupp = 'Objektsbeskrivning Bild 1'"),
                new Tuple<string, string>("bild1", "/OBJEKT/pictures/picture[1]"),
                new Tuple<string, string>("bild2", "/OBJEKT/pictures/picture[2]"),
                new Tuple<string, string>("bild3", "/OBJEKT/pictures/picture[3]"),
                new Tuple<string, string>("bild4", "/OBJEKT/pictures/picture[4]"),
                new Tuple<string, string>("bild5", "/OBJEKT/pictures/picture[5]"),
                new Tuple<string, string>("bild6", "/OBJEKT/pictures/picture[6]"),
                new Tuple<string, string>("bild7", "/OBJEKT/pictures/picture[7]"),
                new Tuple<string, string>("bild8", "/OBJEKT/pictures/picture[8]"),
                new Tuple<string, string>("bild9", "/OBJEKT/pictures/picture[9]"),
                new Tuple<string, string>("bild10", "/OBJEKT/pictures/picture[10]")
            };

            return tuples.Select(t => new PageTemplatePlaceholderMapping
            {
                TemplateType = "VitecMäklarsystem",
                TemplateVersion = "1.0",
                Name = t.Item1,
                Path = t.Item2
            });
        }

    }
}
