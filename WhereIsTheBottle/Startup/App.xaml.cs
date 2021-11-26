using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using Autofac;
using NLog;
using QS.ErrorReporting;
using WhereIsTheBottle.Infrastructure;

namespace WhereIsTheBottle.Startup
{
	public partial class App : Application
	{
		private static IContainer _container;
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		protected override void OnStartup(StartupEventArgs e)
		{
			Main main;
			ILifetimeScope startupScope;

			try
			{
#if DEBUG
				AttachConsole(-1);
#endif
				CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");
				_container = CreateContainer();

				var unhandledExceptionHandler = _container.Resolve<IUnhandledExceptionHandler>();
				unhandledExceptionHandler.SubscribeToUnhandledExceptions();

				startupScope = _container.BeginLifetimeScope();
				main = startupScope.Resolve<Main>();
			}
			catch(Exception ex)
			{
				_logger.Error(ex, "Ошибка в стартовой конфигурации приложения");
				throw new TerminatingException(ex);
			}

			main.StartApplication();
			startupScope.Dispose();
			Current.Shutdown();
		}

		[DllImport("kernel32.dll")]
		private static extern bool AttachConsole(int dwProcessId);
	}
}
