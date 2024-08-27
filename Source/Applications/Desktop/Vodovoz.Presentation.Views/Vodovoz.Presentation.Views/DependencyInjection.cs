using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Presentation.ViewModels.AttachedFiles;

namespace Vodovoz.Presentation.Views
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPresentationViews(this IServiceCollection serviceCollection)
			=> serviceCollection.AddScoped<IAttachedFileInformationsViewModelFactory, AttachedFileInformationsViewModelFactory>();
	}
}
