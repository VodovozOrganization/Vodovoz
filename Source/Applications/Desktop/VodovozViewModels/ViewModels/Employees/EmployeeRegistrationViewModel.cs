using QS.ViewModels;
using Vodovoz.Domain.Employees;
using QS.Project.Domain;
using QS.DomainModel.UoW;
using QS.Services;

namespace Vodovoz.ViewModels.Employees
{
	public class EmployeeRegistrationViewModel : EntityTabViewModelBase<EmployeeRegistration>
	{
		public EmployeeRegistrationViewModel(
			IEntityUoWBuilder entityUoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices) : base(entityUoWBuilder, unitOfWorkFactory, commonServices)
		{
			
		}
	}
}
