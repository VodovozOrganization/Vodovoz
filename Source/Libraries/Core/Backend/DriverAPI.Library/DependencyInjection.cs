using DriverAPI.Library.Helpers;
using DriverAPI.Library.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using IDeprecatedOrderModel = DriverAPI.Library.Deprecated.Models.IOrderModel;
using DeprecatedOrderModel = DriverAPI.Library.Deprecated.Models.OrderModel;

namespace DriverAPI.Library
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddDriverApiLibrary(this IServiceCollection services)
		{
			// Конвертеры
			foreach(var type in typeof(DependencyInjection)
				.Assembly
				.GetTypes()
				.Where(type => type.IsClass)
				.Where(type => type.Name.EndsWith("Converter"))
				.ToList())
			{
				services.AddScoped(type);
			}

			// Хелперы
			services.AddScoped<ISmsPaymentServiceAPIHelper, SmsPaymentServiceAPIHelper>();
			services.AddScoped<IFCMAPIHelper, FCMAPIHelper>();
			services.AddScoped<IActionTimeHelper, ActionTimeHelper>();

			// DAL обертки
			services.AddScoped<ITrackPointsModel, TrackPointsModel>();
			services.AddScoped<IDriverMobileAppActionRecordModel, DriverMobileAppActionRecordModel>();
			services.AddScoped<IRouteListModel, RouteListModel>();
			services.AddScoped<IOrderModel, OrderModel>();
			services.AddScoped<IEmployeeModel, EmployeeModel>();
			services.AddScoped<ISmsPaymentModel, SmsPaymentModel>();
			services.AddScoped<IDriverComplaintModel, DriverComplaintModel>();
			services.AddScoped<IFastPaymentModel, FastPaymentModel>();

			// Deprecated
			services.AddScoped<IDeprecatedOrderModel, DeprecatedOrderModel>();

			return services;
		}
	}
}
