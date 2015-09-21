namespace Caribbean.DataContexts.Application.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPrintStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Prints", "Status", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Prints", "Status");
        }
    }
}
