using System;
using System.Reflection;
using Autofac;
using NHibernate;
using NHibernate.Cfg;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.ErrorReporting;
using QS.MachineConfig.Configuration;
using QS.Project.DB;
using QS.Project.DB.EntityMappingConfig;
using QS.Project.Versioning;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Store;
using WhereIsTheBottle.Database;
using WhereIsTheBottle.Infrastructure;
using WhereIsTheBottle.Infrastructure.Connections;
using WhereIsTheBottle.Models;
using WhereIsTheBottle.Models.MainContent;
using WhereIsTheBottle.Services;
using WhereIsTheBottle.ViewModels;
using WhereIsTheBottle.ViewModels.MainContent;
using WhereIsTheBottle.Views;

namespace WhereIsTheBottle.Startup
{
	public partial class App
	{
		private static IContainer CreateContainer()
		{
			var builder = new ContainerBuilder();

			builder.RegisterType<WarehouseRepository>().As<IWarehouseRepository>().SingleInstance();

			builder.RegisterType<DefaultOrmConfig>().As<IOrmConfig>().SingleInstance();
			builder.RegisterType<BaseConfigurator>().As<IBaseConnector>().SingleInstance();
			builder.RegisterType<ApplicationVersionInfo>().As<IApplicationInfo>().SingleInstance();
			builder.RegisterType<WpfInteractiveService>().As<IInteractiveService>().SingleInstance();

			builder.RegisterType<WPFUnhandledExceptionHandler>().As<IUnhandledExceptionHandler>();
			builder.RegisterType<DefaultConnectionSettings>().As<IDefaultConnectionSettings>();
			builder.RegisterType<ConnectionManager>().As<IConnectionManager>();
			builder.RegisterType<Main>();
			builder.RegisterType<EditConnectionModel>().As<IEditConnectionModel>();
			builder.RegisterType<EditConnectionViewModel>();
			builder.RegisterType<EditConnectionView>();

			builder.RegisterType<LoginView>().InstancePerLifetimeScope();
			builder.RegisterType<LoginViewModel>().InstancePerLifetimeScope();
			builder.RegisterType<LoginModel>().As<ILoginModel>().InstancePerLifetimeScope();

			builder.Register(_ => new JsonConfigurationManager("VodovozConfig.json") { ConfigSubFolder = "Vodovoz" })
				.As<IConfigurationManager>();
			builder.Register(_ => _.Resolve<IOrmConfig>().SessionFactory).As<ISessionFactory>();
			builder.Register(_ => _.Resolve<IOrmConfig>().NhConfig).As<Configuration>();

			return builder.Build();
		}

		public static void RegisterStartupComponents(ContainerBuilder builder)
		{
			builder.RegisterAssemblyTypes(
					Assembly.GetAssembly(typeof(CounterpartyContractRepository))
					?? throw new InvalidOperationException("CounterpartyContractRepository assembly cannot be null"))
				.Where(t => t.Name.EndsWith("Repository"))
				.AsImplementedInterfaces()
				.SingleInstance();

			builder.RegisterType<SessionProvider>().As<ISessionProvider>().InstancePerLifetimeScope();
			builder.RegisterType<MainContentViewModelFactory>().As<IMainContentViewModelFactory>().InstancePerLifetimeScope();

			builder.RegisterType<DefaultUnitOfWorkFactory>().As<IUnitOfWorkFactory>().InstancePerLifetimeScope();
			builder.RegisterType<EntityMappingConfigProvider>().As<IEntityMappingConfigProvider>().InstancePerLifetimeScope();

			builder.RegisterType<BottleAnalyticsModel>().InstancePerLifetimeScope();
			builder.RegisterType<BottleAnalyticsViewModel>().InstancePerLifetimeScope();
			builder.RegisterType<BottleAnalyticsView>().InstancePerLifetimeScope();

			builder.RegisterType<GeneralSummaryViewModel>();
			builder.RegisterType<GeneralSummaryModel>();

			builder.RegisterType<GeneralDeltaViewModel>();
			builder.RegisterType<GeneralDeltaModel>();

			builder.RegisterType<GeneralAssetViewModel>();
			builder.RegisterType<GeneralAssetModel>();

			builder.RegisterType<DeltaLossViewModel>();
			builder.RegisterType<DeltaLossModel>();

			builder.RegisterType<DeltaShabbyViewModel>();
			builder.RegisterType<DeltaShabbyModel>();

			builder.RegisterType<DeltaDefectiveModel>();
			builder.RegisterType<DeltaDefectiveViewModel>();

			builder.RegisterType<AssetDriversViewModel>();

			builder.RegisterType<AssetWarehouseViewModel>();
			builder.RegisterType<AssetWarehouseModel>();
		}
	}
}
