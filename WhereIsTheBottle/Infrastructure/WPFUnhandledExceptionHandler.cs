using System;
using System.Windows.Threading;
using QS.Dialog;
using QS.ErrorReporting;
using QS.Services;

namespace WhereIsTheBottle.Infrastructure
{
	public class WPFUnhandledExceptionHandler : IUnhandledExceptionHandler
	{
		private readonly IInteractiveService _interactiveService;

		public WPFUnhandledExceptionHandler(IInteractiveService interactiveService)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}

		public void SubscribeToUnhandledExceptions()
		{
			Dispatcher.CurrentDispatcher.UnhandledException += OnDispatcherUnhandledException;
		}

		private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
		{
			_interactiveService.ShowMessage(ImportanceLevel.Error, args.Exception.ToString(), "Ошибка!");
			args.Handled = args.Exception.FindExceptionTypeInInner<TerminatingException>() == null;
		}
	}
}
