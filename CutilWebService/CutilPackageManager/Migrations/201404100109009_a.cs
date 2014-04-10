namespace CutilPackageManager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class a : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "ApiKey", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "ApiKey");
        }
    }
}
