using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Presentation.ViewModels.Factories;
using Vodovoz.ViewModels.Factories;

namespace Vodovoz.ViewModels
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddVodovozViewModels(this IServiceCollection services) =>
			services.AddScoped<IDialogSettingsFactory, DialogSettingsFactory>();
	}
}
