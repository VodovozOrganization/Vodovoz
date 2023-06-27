using DriverAPI.Library.Helpers;
using DriverAPI.Library.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using ActionTimeHelper = DriverAPI.Library.Helpers.ActionTimeHelper;
using DeprecatedActionTimeHelper = DriverAPI.Library.Deprecated.Helpers.ActionTimeHelper;
using DeprecatedOrderModel = DriverAPI.Library.Deprecated.Models.OrderModel;
using DeprecatedTrackPointsModel = DriverAPI.Library.Deprecated.Models.TrackPointsModel;
using IActionTimeHelper = DriverAPI.Library.Helpers.IActionTimeHelper;
using IDeprecatedActionTimeHelper = DriverAPI.Library.Deprecated.Helpers.IActionTimeHelper;
using IDeprecatedOrderModel = DriverAPI.Library.Deprecated.Models.IOrderModel;
using IDeprecatedTrackPointsModel = DriverAPI.Library.Deprecated.Models.ITrackPointsModel;
using IOrderModel = DriverAPI.Library.Models.IOrderModel;
using ITrackPointsModel = DriverAPI.Library.Models.ITrackPointsModel;
using OrderModel = DriverAPI.Library.Models.OrderModel;
using TrackPointsModel = DriverAPI.Library.Models.TrackPointsModel;
using IDeprecated2OrderModel = DriverAPI.Library.Deprecated2.Models.IOrderModel;
using Deprecated2OrderModel = DriverAPI.Library.Deprecated2.Models.OrderModel;
using IDeprecated2RouteListModel = DriverAPI.Library.Deprecated2.Models.IRouteListModel;
using Deprecated2RouteListModel = DriverAPI.Library.Deprecated2.Models.RouteListModel;

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
			services.AddScoped<IDeprecatedTrackPointsModel, DeprecatedTrackPointsModel>();
			services.AddScoped<IDeprecatedActionTimeHelper, DeprecatedActionTimeHelper>();

			// Deprecated2
			services.AddScoped<IDeprecated2OrderModel, Deprecated2OrderModel>();
			services.AddScoped<IDeprecated2RouteListModel, Deprecated2RouteListModel>();

			return services;
		}
	}
}
