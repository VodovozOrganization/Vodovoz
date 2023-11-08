using System;
using QS.DomainModel.UoW;
using QS.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;

namespace Vodovoz.Infrastructure.Services
{
	public class EmployeeService : IEmployeeService
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IUserService _userService;

		public EmployeeService(IUnitOfWorkFactory uowFactory, IUserService userService)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
		}

		public Employee GetEmployeeForCurrentUser()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				return GetEmployeeForCurrentUser(uow);
			}
		}

		public Employee GetEmployeeForCurrentUser(IUnitOfWork uow)
		{
			User userAlias = null;
			return uow.Session.QueryOver<Employee>()
				.JoinAlias(e => e.User, () => userAlias)
				.Where(() => userAlias.Id == _userService.CurrentUserId)
				.SingleOrDefault();
		}

		public Employee GetEmployeeForUser(IUnitOfWork uow, int userId)
		{
			User userAlias = null;
			return uow.Session.QueryOver<Employee>()
				.JoinAlias(e => e.User, () => userAlias)
				.Where(() => userAlias.Id == userId)
				.SingleOrDefault();
		}
	}
}
