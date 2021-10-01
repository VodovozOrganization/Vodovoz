using Bitrix;
using BitrixIntegration;
using BitrixIntegration.Processors;
using Microsoft.Extensions.Configuration;
using Mono.Unix;
using Mono.Unix.Native;
using MySql.Data.MySqlClient;
using NLog;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QSProjectsLib;
using System;
using System.Threading;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.BasicHandbooks;
using Vodovoz.EntityRepositories.Common;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Factories;
using Vodovoz.Models;
using Vodovoz.Parameters;

namespace VodovozBitrixIntegrationService
{
	class Service
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private static readonly string _configFile = "/etc/vodovoz-bitrix-integration-service.conf"; 

		private static MysqlSettings _mysqlSettings;

		private static BitrixSettings _bitrixSettings;

		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += AppDomain_CurrentDomain_UnhandledException;

			_logger.Info("Чтение конфигурационного файла...");
			ReadServiceConfiguration();

			_logger.Info("Настройка подключения к БД...");
			ConfigureDBConnection();

			try
			{
				_logger.Info("Запуск сервиса...");

				StartService();

				_logger.Info("Сервис запущен.");

				if(Environment.OSVersion.Platform == PlatformID.Unix)
				{
					UnixSignal[] signals = {
						new UnixSignal (Signum.SIGINT),
						new UnixSignal (Signum.SIGHUP),
						new UnixSignal (Signum.SIGTERM)
					};
					UnixSignal.WaitAny(signals);
				}
				else
				{
					Console.ReadLine();
				}
			}
			catch(Exception e)
			{
				_logger.Fatal(e);
			}
			finally
			{
				if(Environment.OSVersion.Platform == PlatformID.Unix)
				{
					Thread.CurrentThread.Abort();
				}

				Environment.Exit(0);
			}
		}

		#region Configure

		private static void ReadServiceConfiguration()
		{
			try
			{
				var builder = new ConfigurationBuilder()
					.AddIniFile(_configFile, optional: false);

				var configuration = builder.Build();

				_mysqlSettings = new MysqlSettings();
				configuration.GetSection(nameof(MysqlSettings)).Bind(_mysqlSettings);

				_bitrixSettings = new BitrixSettings();
				configuration.GetSection(nameof(BitrixSettings)).Bind(_bitrixSettings);

			}
			catch(Exception ex)
			{
				_logger.Fatal(ex, "Ошибка чтения конфигурационного файла.");
				return;
			}
		}

		private static void ConfigureDBConnection()
		{
			try
			{
				var conStrBuilder = new MySqlConnectionStringBuilder
				{
					Server = _mysqlSettings.Hostname,
					Port = _mysqlSettings.Port,
					Database = _mysqlSettings.Database,
					UserID = _mysqlSettings.Username,
					Password = _mysqlSettings.Password,
					SslMode = MySqlSslMode.None
				};

				QSMain.ConnectionString = conStrBuilder.GetConnectionString(true);
				var dbConfig = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
					.ConnectionString(conStrBuilder.GetConnectionString(true));

				OrmConfig.ConfigureOrm(
					dbConfig,
					new[]
					{
						System.Reflection.Assembly.GetAssembly(typeof(Vodovoz.HibernateMapping.OrganizationMap)),
						System.Reflection.Assembly.GetAssembly(typeof(QS.Banks.Domain.Bank)),
						System.Reflection.Assembly.GetAssembly(typeof(QS.HistoryLog.HistoryMain)),
						System.Reflection.Assembly.GetAssembly(typeof(QS.Project.Domain.UserBase))
					});

				QS.HistoryLog.HistoryMain.Enable();
			}
			catch(Exception ex)
			{
				_logger.Fatal(ex, "Ошибка в настройке подключения к БД.");
			}
		}

		#endregion

		#region StartService

		static async void StartService()
		{
			try
			{
				var parametersProvider = new ParametersProvider();
				var uowFactory = new DefaultUnitOfWorkFactory(new DefaultSessionProvider());
				var bitrixClient = new BitrixClient(_bitrixSettings.UserId, _bitrixSettings.ApiKey);

				var orderOrganizationProviderFactory = new OrderOrganizationProviderFactory();
				var orderOrganizationProvider = orderOrganizationProviderFactory.CreateOrderOrganizationProvider();
				var counterpartyContractRepository = new CounterpartyContractRepository(orderOrganizationProvider);
				var counterpartyContractFactory = new CounterpartyContractFactory(orderOrganizationProvider, counterpartyContractRepository);

				var bitrixRepository = new BitrixRepository();
				var dealRegistrator = new DealRegistrator(uowFactory, bitrixRepository, bitrixClient);

				var measurementUnitsRepository = new MeasurementUnitsRepository();
				var bitrixServiceSettings = new BitrixServiceSettings(parametersProvider);
				var orderRepository = new OrderRepository();
				var counterpartyRepository = new CounterpartyRepository();

				var nomenclatureParametersProvider = new NomenclatureParametersProvider(parametersProvider);
				var nomenclatureRepository = new NomenclatureRepository(nomenclatureParametersProvider);

				var deliveryScheduleRepository = new DeliveryScheduleRepository();

				var deliveryPointProcessor = new DeliveryPointProcessor();

				var productGroupProcessor = new ProductGroupProcessor(bitrixClient, nomenclatureRepository);

				var productProcessor = new ProductProcessor(bitrixClient, nomenclatureRepository, productGroupProcessor,
					measurementUnitsRepository);

				var counterpartyProcessor = new CounterpartyProcessor(bitrixClient, counterpartyRepository);

				var dealProcessor = new DealProcessor(
					uowFactory,
					bitrixClient,
					counterpartyContractRepository,
					counterpartyContractFactory,
					dealRegistrator,
					bitrixServiceSettings,
					orderRepository,
					deliveryScheduleRepository,
					deliveryPointProcessor,
					productProcessor,
					counterpartyProcessor
				);

				var dealWorker = new DealWorker(dealProcessor);
				dealWorker.Start();
			}
			catch (Exception e)
			{
				_logger.Fatal($"!Ошибка дошла до самого верхнего уровня! Ошибка: {e.Message}\n\n{e.InnerException?.Message}");
			}
		}

		#endregion StartService

		static void AppDomain_CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			_logger.Fatal((Exception)e.ExceptionObject, "UnhandledException");
		}
	}
}
