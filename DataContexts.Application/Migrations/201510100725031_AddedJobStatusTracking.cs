namespace Caribbean.DataContexts.Application.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedJobStatusTracking : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Pages", "ThumbnailJobId", c => c.Guid(nullable: false));
            AddColumn("dbo.Pages", "ThumbnailJobStatus", c => c.Int(nullable: false));
            AddColumn("dbo.Pages", "ThumbnailJobDurationMs", c => c.Long(nullable: false));
            AddColumn("dbo.Pages", "PdfJobId", c => c.Guid(nullable: false));
            AddColumn("dbo.Pages", "PdfJobStatus", c => c.Int(nullable: false));
            AddColumn("dbo.Pages", "PdfJobDurationMs", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Pages", "PdfJobDurationMs");
            DropColumn("dbo.Pages", "PdfJobStatus");
            DropColumn("dbo.Pages", "PdfJobId");
            DropColumn("dbo.Pages", "ThumbnailJobDurationMs");
            DropColumn("dbo.Pages", "ThumbnailJobStatus");
            DropColumn("dbo.Pages", "ThumbnailJobId");
        }
    }
}
