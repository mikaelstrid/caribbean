namespace Caribbean.DataContexts.Application.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTimestampsToPageAndPrintPdfs : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Prints", "PdfCreationTimeUtc", c => c.DateTime(nullable: false));
            AddColumn("dbo.Pages", "ThumbnailJobEnqueueTimeUtc", c => c.DateTime(nullable: false));
            AddColumn("dbo.Pages", "ThumbnailJobCompletionTimeUtc", c => c.DateTime(nullable: false));
            AddColumn("dbo.Pages", "PdfJobEnqueueTimeUtc", c => c.DateTime(nullable: false));
            AddColumn("dbo.Pages", "PdfJobCompletionTimeUtc", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Pages", "PdfJobCompletionTimeUtc");
            DropColumn("dbo.Pages", "PdfJobEnqueueTimeUtc");
            DropColumn("dbo.Pages", "ThumbnailJobCompletionTimeUtc");
            DropColumn("dbo.Pages", "ThumbnailJobEnqueueTimeUtc");
            DropColumn("dbo.Prints", "PdfCreationTimeUtc");
        }
    }
}
