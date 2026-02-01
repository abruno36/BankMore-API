using FluentMigrator;

namespace BankMore.Infrastructure.Migrations
{
    [Migration(202412150000)]
    public class InitialDatabase : Migration
    {
        public override void Up()
        {
            Create.Table("ContasCorrentes")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("NumeroConta").AsString(20).NotNullable().Unique()
                .WithColumn("CPFCriptografado").AsString(500).NotNullable()
                .WithColumn("CPFHash").AsString(100).NotNullable()
                .WithColumn("SenhaHash").AsString(500).NotNullable()
                .WithColumn("NomeTitular").AsString(100).NotNullable()
                .WithColumn("Email").AsString(100).NotNullable().Unique()
                .WithColumn("Ativa").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("DataCriacao").AsDateTime().NotNullable()
                .WithColumn("DataInativacao").AsDateTime().Nullable();

            Create.Table("Movimentacoes")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("ContaId").AsGuid().NotNullable()
                .WithColumn("Tipo").AsString(1).NotNullable()
                .WithColumn("Valor").AsDecimal(18, 2).NotNullable()
                .WithColumn("Descricao").AsString(200).Nullable()
                .WithColumn("DataMovimentacao").AsDateTime().NotNullable()
                .WithColumn("ContaDestino").AsString(20).Nullable();

            Create.ForeignKey("FK_Movimentacoes_ContasCorrentes")
                .FromTable("Movimentacoes").ForeignColumn("ContaId")
                .ToTable("ContasCorrentes").PrimaryColumn("Id");
        }

        public override void Down()
        {
            Delete.Table("Movimentacoes");
            Delete.Table("ContasCorrentes");
        }
    }
}