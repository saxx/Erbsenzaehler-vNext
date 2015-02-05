using System.Data.Entity.Migrations;

namespace Erbsenzaehler.Migrations
{
    public partial class Two : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Accounts", "Name", c => c.String(nullable: false));
        }


        public override void Down()
        {
            AlterColumn("dbo.Accounts", "Name", c => c.String());
        }
    }
}