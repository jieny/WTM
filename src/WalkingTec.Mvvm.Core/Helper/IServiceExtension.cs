using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WalkingTec.Mvvm.Core.Support.FileHandlers;

namespace WalkingTec.Mvvm.Core
{
    public static class IServiceExtension
    {
        public static IServiceCollection AddWtmContextForConsole(this IServiceCollection services, Configs WtmConfigs, Func<IWtmFileHandler, string> fileSubDirSelector = null)
        {
            WtmFileProvider._subDirFunc = fileSubDirSelector;
            services.TryAddScoped<IDataContext, NullContext>();
            IdleBus<IFreeSql> ib = new IdleBus<IFreeSql>(TimeSpan.FromMinutes(10));
            foreach (var item in WtmConfigs?.ConnectionStrings)
            {
                switch (item.DbType)
                {
                    case DBTypeEnum.SqlServer:
                        ib.Register(item.Key, () => new FreeSqlBuilder().UseConnectionString(DataType.SqlServer, item.Value).UseAutoSyncStructure(true).Build());
                        break;
                    case DBTypeEnum.MySql:
                        ib.Register(item.Key, () => new FreeSqlBuilder().UseConnectionString(DataType.MySql, item.Value).Build());
                        break;
                    case DBTypeEnum.PgSql:
                        ib.Register(item.Key, () => new FreeSqlBuilder().UseConnectionString(DataType.PostgreSQL, item.Value).Build());
                        break;
                    case DBTypeEnum.Oracle:
                        ib.Register(item.Key, () => new FreeSqlBuilder().UseConnectionString(DataType.Oracle, item.Value).Build());
                        break;
                    case DBTypeEnum.SQLite:
                        ib.Register(item.Key, () => new FreeSqlBuilder().UseConnectionString(DataType.Sqlite, item.Value).Build());
                        break;
                    default:
                        ib.Register(item.Key, () => new FreeSqlBuilder().UseConnectionString(DataType.SqlServer, item.Value).UseAutoSyncStructure(true).Build());
                        break;
                }
            }
            services.AddSingleton(ib);
            services.AddScoped<WTMContext>();
            services.AddSingleton<WtmFileProvider>();
            services.AddHttpClient();
            if (WtmConfigs.Domains != null)
            {
                foreach (var item in WtmConfigs.Domains)
                {
                    services.AddHttpClient(item.Key, x =>
                    {
                        x.BaseAddress = new Uri(item.Value.Url);
                        x.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
                        x.DefaultRequestHeaders.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)");
                    });
                }
            }
            services.AddDistributedMemoryCache();
            return services;
        }

    }
}
