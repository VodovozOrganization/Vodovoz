using System;
using QS.DomainModel.UoW;
using QS.Services;

namespace Vodovoz.Tools
{
	public class DefaultErrorMessageModelFactory : IErrorMessageModelFactory
	{
		IErrorReporter errorReporter;
		IUserService userService;
		IUnitOfWorkFactory unitOfWorkFactory;

		/// <param name="userService">Если null, то модель не определяет текущего пользователя</param>
		/// <param name="unitOfWorkFactory">>Если null, то модель не определяет текущего пользователя</param>
		public DefaultErrorMessageModelFactory(
			IErrorReporter errorReporter, 
			IUserService userService = null,
			IUnitOfWorkFactory unitOfWorkFactory = null
			)
		{
			this.errorReporter = errorReporter ?? throw new ArgumentNullException(nameof(errorReporter));
			this.userService = userService;
			this.unitOfWorkFactory = unitOfWorkFactory;
		}

		public ErrorMessageModelBase GetModel()
		{
			return new DefaultErrorMessageModel(
				errorReporter,
				userService, 
				unitOfWorkFactory
			);
		}
	}
}
