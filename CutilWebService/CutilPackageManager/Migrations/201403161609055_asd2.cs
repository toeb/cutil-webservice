namespace CutilPackageManager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class asd2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Packages", "Descriptor", c => c.String());
            AddColumn("dbo.Packages", "Data", c => c.Binary());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Packages", "Data");
            DropColumn("dbo.Packages", "Descriptor");
        }
    }
}
