using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Reactive.Concurrency;
using Vodovoz.Presentation.ViewModels.Documents;

namespace Vodovoz.Presentation.ViewModels
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPresentationViewModels(this IServiceCollection serviceCollection)
		{
			ConfigurePresentationViewModels();

			serviceCollection.AddScoped(typeof(PrintableRdlDocumentViewModel<>));

			return serviceCollection;
		}

		public static void ConfigurePresentationViewModels()
		{
			//Необходимо для указания в каком потоке по умолчанию будут выполнятся реактивные команды
			RxApp.MainThreadScheduler = Scheduler.CurrentThread;
		}
	}
}
