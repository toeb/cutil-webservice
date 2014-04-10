namespace CutilPackageManager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class asd3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Packages", "DownloadCount", c => c.Int(nullable: false));
            AddColumn("dbo.Packages", "CurrentVersion", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "Email", c => c.String());
            AddColumn("dbo.AspNetUsers", "IsConfirmed", c => c.Boolean());
            AddColumn("dbo.AspNetUsers", "ConfirmationToken", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "ConfirmationToken");
            DropColumn("dbo.AspNetUsers", "IsConfirmed");
            DropColumn("dbo.AspNetUsers", "Email");
            DropColumn("dbo.Packages", "CurrentVersion");
            DropColumn("dbo.Packages", "DownloadCount");
        }
    }
}
