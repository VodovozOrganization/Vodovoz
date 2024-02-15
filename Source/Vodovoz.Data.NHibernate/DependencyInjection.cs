﻿using Microsoft.Extensions.DependencyInjection;
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
		public static IServiceCollection AddServiceUser(this IServiceCollection services)
		{
			services.AddSingleton<OnDatabaseInitialization>((provider) =>
			{
				var uowFactory = provider.GetRequiredService<IUnitOfWorkFactory>();
				var connectionSettings = provider.GetRequiredService<IDatabaseConnectionSettings>();
				using(var uow = uowFactory.CreateWithoutRoot())
				{
					var serviceUser = uow.Session.Query<UserBase>()
						.Where(u => u.Login == connectionSettings.UserName)
						.FirstOrDefault();

					if(serviceUser is null)
					{
						throw new InvalidOperationException("Service user not found");
					}

					ServicesConfig.UserService = new UserService(serviceUser);
					QS.Project.Repositories.UserRepository.GetCurrentUserId = () => serviceUser.Id;
				}
				return new OnDatabaseInitialization();
			});
			return services;
		}
	}
}
