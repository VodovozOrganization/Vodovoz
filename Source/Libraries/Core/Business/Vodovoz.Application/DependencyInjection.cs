using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Application.Pacs;
using Vodovoz.Core.Application;

namespace Vodovoz.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication(this IServiceCollection services) => services
			.AddCoreApplication()
			.AddSingleton<OperatorService>()
			;
	}
}
