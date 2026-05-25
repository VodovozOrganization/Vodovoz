using System;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Settings.Employee;
using VodovozBusiness.Factories;

namespace Vodovoz.Core.Application.Employees
{
	public class OnlineOrderAuthorFactory : IOnlineOrderAuthorFactory
	{
		private readonly IEmployeeSettings _employeeSettings;
		private readonly IEmployeeRepository _employeeRepository;

		public OnlineOrderAuthorFactory(
			IEmployeeSettings employeeSettings,
			IEmployeeRepository employeeRepository)
		{
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
		}

		public Employee Create(IUnitOfWork uow, Source source)
		{
			var id = GetEmployeeId(source);
			return _employeeRepository.GetEmployee(uow, id);
		}

		public async Task<Employee> CreateAsync(IUnitOfWork uow, Source source, CancellationToken cancellationToken)
		{
			var id = GetEmployeeId(source);
			return await _employeeRepository.GetEmployeeAsync(uow, id, cancellationToken);
		}

		private int GetEmployeeId(Source source)
		{
			switch(source)
			{
				case Source.MobileApp:
					return _employeeSettings.MobileAppEmployee;
				case Source.VodovozWebSite:
					return _employeeSettings.VodovozWebSiteEmployee;
				case Source.KulerSaleWebSite:
					return _employeeSettings.KulerSaleWebSiteEmployee;
				case Source.AiBot:
					return _employeeSettings.AiBotEmployee;
			}
			
			throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестное значение внешнего источника");
		}
	}
}
