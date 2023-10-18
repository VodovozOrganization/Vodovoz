using DriverAPI.Library.Helpers;
using DriverAPI.Library.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using IDeprecated2OrderModel = DriverAPI.Library.Deprecated2.Models.IOrderModel;
using Deprecated2OrderModel = DriverAPI.Library.Deprecated2.Models.OrderModel;
using IDeprecated2RouteListModel = DriverAPI.Library.Deprecated2.Models.IRouteListModel;
using Deprecated2RouteListModel = DriverAPI.Library.Deprecated2.Models.RouteListModel;
using IDeprecated3RouteListModel = DriverAPI.Library.Deprecated3.Models.IRouteListModel;
using Deprecated3RouteListModel = DriverAPI.Library.Deprecated3.Models.RouteListModel;
using IRouteListModel = DriverAPI.Library.Models.IRouteListModel;
using RouteListModel = DriverAPI.Library.Models.RouteListModel;
using Vodovoz.Application;

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
			services.AddScoped<IDeprecated3RouteListModel, Deprecated3RouteListModel>();
			services.AddScoped<IRouteListModel, RouteListModel>();
			services.AddScoped<IOrderModel, OrderModel>();
			services.AddScoped<IEmployeeModel, EmployeeModel>();
			services.AddScoped<ISmsPaymentModel, SmsPaymentModel>();
			services.AddScoped<IDriverComplaintModel, DriverComplaintModel>();
			services.AddScoped<IFastPaymentModel, FastPaymentModel>();

			// Deprecated2
			services.AddScoped<IDeprecated2OrderModel, Deprecated2OrderModel>();
			services.AddScoped<IDeprecated2RouteListModel, Deprecated2RouteListModel>();

			services.AddApplication();

			return services;
		}
	}
}
