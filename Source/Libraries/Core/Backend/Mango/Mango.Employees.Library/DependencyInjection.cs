using Mango.Employees.Library.Options;
using Mango.Employees.Library.Services;
using Mango.Vpbx.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Mango.Employees.Library
{
	public static class DependencyInjection
	{
		/// <summary>
		/// Регистрирует сервисы работы с сотрудниками Манго для водителей
		/// </summary>
		public static IServiceCollection AddMangoEmployeesServices(this IServiceCollection services)
		{
			services.ConfigureOptions<ConfigureDriverMangoEmployeeRegistrationOptions>();
			services.ConfigureOptions<ConfigureDriverMangoEmployeeDeactivationOptions>();

			services.AddMangoVpbxClientServices();

			services.AddScoped<DriverMangoEmployeeRegistrationService>();
			services.AddScoped<DriverMangoEmployeeDeactivationService>();

			return services;
		}
	}
}
