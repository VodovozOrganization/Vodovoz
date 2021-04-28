using DriverAPI.Data;
using DriverAPI.Library.Converters;
using DriverAPI.Library.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySql.Data.MySqlClient;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.DB;
using QSProjectsLib;
using System;
using System.Text;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.NhibernateExtensions;
using Vodovoz.Tools;

namespace DriverAPI
{
    public class Startup
    {
        private ILogger<Startup> logger;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(logging => logging.AddConsole());

            logger = new Logger<Startup>(LoggerFactory.Create(logging => logging.AddConsole()));

            // Подключение к БД

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySQL(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();

            // Конфигурация Nhibernate

            try {
                CreateBaseConfig();
            } catch (Exception e)
            {
                logger.LogCritical(e, e.Message);
                throw;
            }

            // Сервисы для контроллеров

            // Unit Of Work
            services.AddScoped<IUnitOfWork>((sp) => UnitOfWorkFactory.CreateWithoutRoot());

            // Репозитории водовоза
            services.AddScoped<IRouteListRepository, RouteListRepository>();
            services.AddScoped<IOrderRepository, OrderSingletonRepository>((sp) => OrderSingletonRepository.GetInstance());

            // Конвертеры
            services.AddScoped<DeliveryPointConverter>();
            services.AddScoped<RouteListConverter>();
            services.AddScoped<OrderConverter>();
            services.AddScoped<SmsPaymentConverter>();

            // DAL обертки
            services.AddScoped<IAPIRouteListData, APIRouteListData>();
            services.AddScoped<IAPIOrderData, APIOrderData>();
            services.AddScoped<IAPISmsPaymentData, APISmsPaymentData>();
            services.AddScoped<IAPIDriverComplaintData, APIDriverComplaintData>();

            // Аутентификация

            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(cfg =>
                {
                    cfg.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = false,
                        ValidIssuer = Configuration["Security:Tokens:Issuer"],
                        ValidateAudience = false,
                        ValidAudience = Configuration["Security:Tokens:Audience"],
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Security:Tokens:Key"])),
                    };
                });

            // Регистрация контроллеров

            services.AddControllersWithViews();
            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DriverAPI", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DriverAPI v1"));
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }

        void CreateBaseConfig()
        {
            logger.LogInformation("Настройка параметров базы...");
            //Увеличиваем таймоут

            var conStrBuilder = new MySqlConnectionStringBuilder();
            conStrBuilder.Server = Configuration["Nhibernate:Server"];
            conStrBuilder.Port = uint.Parse(Configuration["Nhibernate:Port"]);
            conStrBuilder.Database = Configuration["Nhibernate:Database"];
            conStrBuilder.UserID = Configuration["Nhibernate:UserID"];
            conStrBuilder.Password = Configuration["Nhibernate:Password"];
            conStrBuilder.SslMode = MySqlSslMode.None;

            QSMain.ConnectionString = conStrBuilder.GetConnectionString(true);

            var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
                .Dialect<MySQL57SpatialExtendedDialect>()
                .ConnectionString(QSMain.ConnectionString)
                .AdoNetBatchSize(100)
                .Driver<LoggedMySqlClientDriver>();

            // Настройка ORM
            OrmConfig.ConfigureOrm(
                db_config,
                new System.Reflection.Assembly[] {
                    System.Reflection.Assembly.GetAssembly (typeof(QS.Project.HibernateMapping.UserBaseMap)),
                    System.Reflection.Assembly.GetAssembly (typeof(Vodovoz.HibernateMapping.OrganizationMap)),
                    System.Reflection.Assembly.GetAssembly (typeof(Bank)),
                    System.Reflection.Assembly.GetAssembly (typeof(HistoryMain)),
                }
            );

            HistoryMain.Enable();
        }
    }
}
