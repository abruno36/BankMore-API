// BankMore.Infrastructure/Migrations/202412161000_AddIdempotencyKeysTable.cs
using FluentMigrator;

namespace BankMore.Infrastructure.Migrations
{
    [Migration(202412161000)]
    public class AddIdempotencyKeysTable : Migration
    {
        public override void Up()
        {
            Create.Table("IdempotencyKeys")
                .WithColumn("Id").AsString(36).PrimaryKey()
                .WithColumn("RequestType").AsString(50).NotNullable()
                .WithColumn("ContaOrigem").AsString(20).Nullable()
                .WithColumn("ContaDestino").AsString(20).Nullable()
                .WithColumn("Valor").AsDecimal(18, 2).Nullable()
                .WithColumn("Status").AsString(20).NotNullable()
                .WithColumn("ResponseData").AsString(int.MaxValue).Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);
        }

        public override void Down()
        {
            Delete.Table("IdempotencyKeys");
        }
    }
}
