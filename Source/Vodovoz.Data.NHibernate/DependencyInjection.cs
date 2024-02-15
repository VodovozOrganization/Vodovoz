using Autofac;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using QS.Project.Core;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Services;
using System;
using System.Linq;

namespace Vodovoz.Data.NHibernate
{
	public static class DependencyInjection
	{
		[Obsolete("Удалить после очистки сущностей от зависимостей")]
		public static IServiceCollection AddStaticScopeForEntity(this IServiceCollection services)
		{
			services.AddSingleton<OnDatabaseInitialization>((provider) =>
			{
				ScopeProvider.Scope = provider.GetRequiredService<ILifetimeScope>();
				return new OnDatabaseInitialization();
			});
			return services;
		}
	}
}
