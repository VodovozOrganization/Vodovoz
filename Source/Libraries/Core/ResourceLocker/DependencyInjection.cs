using Microsoft.Extensions.DependencyInjection;
using ResourceLocker.Library.Factories;
using ResourceLocker.Library.Mocks;
using ResourceLocker.Library.Providers;
using StackExchange.Redis;
using Vodovoz.Settings.ResourceLocker;

namespace ResourceLocker.Library
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddVodovozDesktopResourceLockerMock(this IServiceCollection services)
		{
			services.AddGarnetResourceLockerFactoryMock();
			services.AddSingleton<IResourceLockerUniqueKeyProvider, VodovozDesktopResourceLockerUniqueKeyProvider>();
			services.AddTransient<IResourceLockerValueProvider, VodovozDesktopResourceLockerValueProvider>();

			return services;
		}
		
		public static IServiceCollection AddVodovozDesktopResourceLocker(this IServiceCollection services)
		{
			services.AddVodovozDesktopLockerConnection();
			services.AddGarnetResourceLockerFactory();
			services.AddSingleton<IResourceLockerUniqueKeyProvider, VodovozDesktopResourceLockerUniqueKeyProvider>();
			services.AddTransient<IResourceLockerValueProvider, VodovozDesktopResourceLockerValueProvider>();

			return services;
		}

		public static IServiceCollection AddVodovozDesktopLockerConnection(this IServiceCollection services)
		{
			services.AddSingleton<IConnectionMultiplexer>(sp =>
			{
				var garnetSettings = sp.GetRequiredService<IGarnetSettings>();
				return ConnectionMultiplexer.Connect(garnetSettings.ConnectionString);
			});

			return services;
		}

		public static IServiceCollection AddGarnetResourceLockerFactory(this IServiceCollection services)
		{
			services.AddSingleton<IResourceLockerFactory, GarnetResourceLockerFactory>();
			return services;
		}
		
		public static IServiceCollection AddGarnetResourceLockerFactoryMock(this IServiceCollection services)
		{
			services.AddSingleton<IResourceLockerFactory, GarnetResourceLockerFactoryMock>();
			return services;
		}
	}
}
