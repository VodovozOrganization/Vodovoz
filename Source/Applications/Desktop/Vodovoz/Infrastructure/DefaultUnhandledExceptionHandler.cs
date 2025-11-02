using System;
using System.Collections.Generic;
using System.Threading;
using Gtk;
using QS.Dialog;
using QS.Project.Domain;
using QS.Project.Versioning;
using Vodovoz.Views;
using Vodovoz.Tools;
using QS.Services;
using Microsoft.Extensions.Logging;

namespace Vodovoz.Infrastructure
{
	/// <summary>
	/// Делегат для перехвата и отдельной обработки некоторых ошибок.
	/// Метод должне возвращать true, если ошибку он обработал сам 
	/// и ее больше не надо передавать вниз по списку зарегистированных обработчиков,
	/// вплодь до стандартного диалога отправки отчета об ошибке.
	/// </summary>
	public delegate bool CustomErrorHandler(Exception exception, IApplicationInfo application, UserBase user, IInteractiveService interactiveMessage);
	/// <summary>
	/// Класс помогает сформировать отправку отчета о падении программы.
	/// Для работы необходимо предварительно сконфигурировать модуль
	/// GuiThread - указать поток Gui, нужно для корректной обработки эксепшенов в других потоках.
	/// InteractiveMessage - Класс позволяющий обработчикам выдать сообщение пользователю.
	/// IApplicationInfo - Класс с информацией о текущем приложении
	/// IDataBaseInfo - Класс с информацией о текущей базе данных
	/// UserBase - Текущий пользователь
	/// IErrorMessageModelFactory - Фабрика для создания модели диалога отправки ошибок
	/// </summary>
	public class DefaultUnhandledExceptionHandler
	{
		private static ILogger<DefaultUnhandledExceptionHandler> _logger;

		public DefaultUnhandledExceptionHandler(
			ILogger<DefaultUnhandledExceptionHandler> logger,
			IErrorMessageModelFactory errorMessageModelFactory,
			IApplicationInfo applicationInfo, 
			IInteractiveService interactiveService = null
		){
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			ErrorMessageModelFactory = errorMessageModelFactory ?? throw new ArgumentNullException(nameof(errorMessageModelFactory));
			ApplicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
			InteractiveService = interactiveService;
		}

		#region Внешние настройки модуля

		public Thread GuiThread { get; set; }
		public IApplicationInfo ApplicationInfo { get; set; }
		public IInteractiveService InteractiveService { get; set; }
		public IErrorMessageModelFactory ErrorMessageModelFactory { get; set; }
		public UserBase User { get; set; }

		/// <summary>
		/// В список можно добавить собственные обработчики ошибкок. Внимание! Порядок добавления обрабочиков важен,
		/// так как если ошибку обработает первый обработчик ко второму она уже не попадет.
		/// </summary>
		public readonly List<CustomErrorHandler> CustomErrorHandlers = new List<CustomErrorHandler>();

		#endregion

		public void SubscribeToUnhandledExceptions()
		{
			AppDomain.CurrentDomain.UnhandledException -= OnApplcationException;
			AppDomain.CurrentDomain.UnhandledException += OnApplcationException;
			GLib.ExceptionManager.UnhandledException -= OnGtkException;
			GLib.ExceptionManager.UnhandledException += OnGtkException;
		}

		public void ErrorMessage(Exception ex)
		{
			if(GuiThread == Thread.CurrentThread) {
				RealErrorMessage(ex);
			} else {
				_logger.LogDebug("From Another Thread");
				Gtk.Application.Invoke(delegate {
					RealErrorMessage(ex);
				});
			}
		}

		private ErrorMessageViewModel errorMessageViewModel;
		private void RealErrorMessage(Exception exception)
		{
			if(InteractiveService != null) {
				foreach(var handler in CustomErrorHandlers) {
					try {
						if(handler(exception, ApplicationInfo, User, InteractiveService)) {
							return;
						}
					}
					catch(Exception ex) {
						_logger.LogError(ex, "Ошибка в CustomErrorHandler");
					}
				}
			}
			if(errorMessageViewModel != null) {
				_logger.LogDebug("Добавляем исключение в уже созданное окно.");
				errorMessageViewModel.AddException(exception);
			} else {
				_logger.LogDebug("Создание окна отправки отчета о падении.");
				errorMessageViewModel = new ErrorMessageViewModel(ErrorMessageModelFactory.GetModel(), InteractiveService);
				errorMessageViewModel.AddException(exception);

				var errView = new ErrorMessageView(errorMessageViewModel);
				errView.ShowAll();
				errView.Run();
				errView.Destroy();
				errorMessageViewModel = null;
				_logger.LogDebug("Окно отправки отчета, уничтожено.");
			}
		}

		private void OnApplcationException(object sender, UnhandledExceptionEventArgs e)
		{
			_logger.LogCritical((Exception)e.ExceptionObject, "Поймано необработаное исключение в Application Domain.");
			ErrorMessage((Exception)e.ExceptionObject);
		}
		private void OnGtkException(GLib.UnhandledExceptionArgs a)
		{
			_logger.LogCritical((Exception)a.ExceptionObject, "Поймано необработаное исключение в GTK.");
			ErrorMessage((Exception)a.ExceptionObject);
		}
	}
}
