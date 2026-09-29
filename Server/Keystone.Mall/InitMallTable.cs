using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlSugar.IOC;


namespace Keystone.Mall
{
    /// <summary>
    /// 初始化表
    /// </summary>
    public static class InitMallTable
    {
        public static void InitDb(this IServiceCollection services, IWebHostEnvironment environment)
        {
            var db = DbScoped.SugarScope.GetConnection("1");
            var options = App.OptionsSetting;
            
            if (!options.InitDb) return;

            if (environment.IsDevelopment())
            {
                //db.CodeFirst.InitTables(typeof(Product));
            }
        }
    }
}
