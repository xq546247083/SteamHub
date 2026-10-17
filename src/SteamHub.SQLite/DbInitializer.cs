using FreeSql;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace SteamHub;

/// <summary>
/// 数据库初始化与版本迁移管理器
/// 基于成熟开源组件 FreeSql 的 CodeFirst 自动同步能力进行表结构对比与升级，保持 EF Core 继续负责所有数据查询与操作
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// 初始化数据库
    /// </summary>
    public static void Initialize()
    {
        UpgradeDatabase();
    }

    public static void UpgradeDatabase()
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.DbFileName);

        using var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.Sqlite, $"Data Source={dbPath}")
            .UseAutoSyncStructure(true)
            .Build();

        using var context = new SteamHubDbContext();
        var entityTypes = context.Model.GetEntityTypes().ToList();
        foreach (var entityType in entityTypes)
        {
            var tableName = entityType.GetTableName();
            if (string.IsNullOrEmpty(tableName))
            {
                continue;
            }

            fsql.CodeFirst.ConfigEntity(entityType.ClrType, a =>
            {
                a.Name(tableName);
                foreach (var index in entityType.GetIndexes())
                {
                    var indexName = index.GetDatabaseName();
                    var columns = string.Join(", ", index.Properties.Select(p => p.GetColumnName()));
                    if (!string.IsNullOrEmpty(indexName) && !string.IsNullOrEmpty(columns))
                    {
                        a.Index(indexName, columns, index.IsUnique);
                    }
                }
            });
        }
        fsql.CodeFirst.SyncStructure([.. entityTypes.Select(e => e.ClrType)]);
    }
}