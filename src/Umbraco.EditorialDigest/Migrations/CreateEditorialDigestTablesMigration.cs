using System.Data;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.EditorialDigest.Persistence;

namespace Umbraco.EditorialDigest.Migrations;

public sealed class CreateEditorialDigestTablesMigration : AsyncMigrationBase
{
    public CreateEditorialDigestTablesMigration(IMigrationContext context)
        : base(context)
    {
    }

    protected override Task MigrateAsync()
    {
        CreateTableIfMissing<EditorialDigestConfigSchema>("umbracoEditorialDigestConfig");
        CreateTableIfMissing<EditorialDigestMailingListEntry>("umbracoEditorialDigestMailingList");
        CreateTableIfMissing<EditorialDigestLog>("umbracoEditorialDigestLog");
        CreateTableIfMissing<EditorialDigestGlobalSettingsSchema>("umbracoEditorialDigestGlobalSettings");

        CreateUniqueIndexIfMissing("IX_EditorialDigestConfig_Alias", "umbracoEditorialDigestConfig", "Alias");
        CreateUniqueIndexIfMissing("IX_EditorialDigestMailingList_UnsubscribeToken", "umbracoEditorialDigestMailingList", "UnsubscribeToken");
        CreateIndexIfMissing("IX_EditorialDigestMailingList_ConfigId", "umbracoEditorialDigestMailingList", "ConfigId");
        CreateIndexIfMissing("IX_EditorialDigestLog_ConfigId_SentDate", "umbracoEditorialDigestLog", "ConfigId", "SentDate");
        if (!SqlSyntax.DbProvider.Equals("Microsoft.Data.Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            CreateForeignKey("FK_EditorialDigestMailingList_Config", "umbracoEditorialDigestMailingList");
            CreateForeignKey("FK_EditorialDigestLog_Config", "umbracoEditorialDigestLog");
        }

        return Task.CompletedTask;
    }

    private void CreateTableIfMissing<T>(string tableName)
    {
        if (!TableExists(tableName))
        {
            Create.Table<T>().Do();
        }
    }

    private void CreateUniqueIndexIfMissing(string name, string tableName, string columnName)
    {
        if (!IndexExists(name))
        {
            Create.Index(name).OnTable(tableName).OnColumn(columnName).Ascending().WithOptions().Unique().Do();
        }
    }

    private void CreateIndexIfMissing(string name, string tableName, params string[] columnNames)
    {
        if (IndexExists(name))
        {
            return;
        }

        var indexBuilder = Create.Index(name).OnTable(tableName);
        foreach (var columnName in columnNames)
        {
            indexBuilder = indexBuilder.OnColumn(columnName).Ascending();
        }

        indexBuilder.Do();
    }

    private void CreateForeignKey(string name, string sourceTable)
    {
        Create.ForeignKey(name)
            .FromTable(sourceTable).ForeignColumn("ConfigId")
            .ToTable("umbracoEditorialDigestConfig").PrimaryColumn("Id")
            .OnDelete(Rule.Cascade)
            .Do();
    }
}
