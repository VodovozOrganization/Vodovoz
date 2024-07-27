using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Application.BankStatements;
using Vodovoz.Presentation.ViewModels.Organisations;
using Vodovoz.ViewModels.Factories;

namespace Vodovoz.ViewModels
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddVodovozViewModels(this IServiceCollection services) =>
			services.AddScoped<IDialogSettingsFactory, DialogSettingsFactory>()
				.AddScoped<BankStatementHandler>()
				.AddScoped<BankStatementParser>();
	}
}
