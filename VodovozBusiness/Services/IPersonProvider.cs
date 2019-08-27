using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Services
{
	public interface IPersonProvider
	{
		Employee GetDefaultEmployeeForCallTask(IUnitOfWork uow);

		Employee GetDefaultEmployeeForDepositReturnTask(IUnitOfWork uow);
	}
}
