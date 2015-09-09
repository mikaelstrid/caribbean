namespace Caribbean.DataContexts.Application.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Agents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(),
                        AgencyId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Agencies", t => t.AgencyId, cascadeDelete: true)
                .Index(t => t.AgencyId);
            
            CreateTable(
                "dbo.Agencies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        VitecCustomerId = c.String(),
                        Slug = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Prints",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PrintVariantSlug = c.String(),
                        ObjectId = c.String(),
                        CreationTimeUtc = c.DateTime(nullable: false),
                        ModifiedTimeUtc = c.DateTime(nullable: false),
                        PdfName = c.String(),
                        PdfUrl = c.String(),
                        AgentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Agents", t => t.AgentId, cascadeDelete: true)
                .Index(t => t.AgentId);
            
            CreateTable(
                "dbo.Pages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Position = c.Int(nullable: false),
                        PageTemplateSlug = c.String(),
                        ThumbnailName = c.String(),
                        ThumbnailUrl = c.String(),
                        PdfName = c.String(),
                        PdfUrl = c.String(),
                        PrintId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Prints", t => t.PrintId, cascadeDelete: true)
                .Index(t => t.PrintId);
            
            CreateTable(
                "dbo.FieldValues",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.String(),
                        FieldName = c.String(),
                        PageId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Pages", t => t.PageId, cascadeDelete: true)
                .Index(t => t.PageId);
            
            CreateTable(
                "dbo.PageTemplatePlaceholderMappings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TemplateType = c.String(),
                        TemplateVersion = c.String(),
                        Name = c.String(),
                        Path = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Prints", "AgentId", "dbo.Agents");
            DropForeignKey("dbo.Pages", "PrintId", "dbo.Prints");
            DropForeignKey("dbo.FieldValues", "PageId", "dbo.Pages");
            DropForeignKey("dbo.Agents", "AgencyId", "dbo.Agencies");
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.FieldValues", new[] { "PageId" });
            DropIndex("dbo.Pages", new[] { "PrintId" });
            DropIndex("dbo.Prints", new[] { "AgentId" });
            DropIndex("dbo.Agents", new[] { "AgencyId" });
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.PageTemplatePlaceholderMappings");
            DropTable("dbo.FieldValues");
            DropTable("dbo.Pages");
            DropTable("dbo.Prints");
            DropTable("dbo.Agencies");
            DropTable("dbo.Agents");
        }
    }
}
