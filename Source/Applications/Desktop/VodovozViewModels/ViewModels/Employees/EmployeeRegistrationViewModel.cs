using System;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using QS.Project.Domain;
using QS.DomainModel.UoW;
using QS.Services;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.ViewModels.Employees
{
	public class EmployeeRegistrationViewModel : EntityTabViewModelBase<EmployeeRegistration>
	{
		public EmployeeRegistrationViewModel(
			IEntityUoWBuilder entityUoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeRepository employeeRepository) : base(entityUoWBuilder, unitOfWorkFactory, commonServices)
		{
			ValidationContext.ServiceContainer.AddService(
				typeof(IEmployeeRepository),
				employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository)));
		}
	}
}
