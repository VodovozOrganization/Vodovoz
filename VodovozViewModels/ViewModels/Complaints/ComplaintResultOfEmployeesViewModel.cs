using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintResultOfEmployeesViewModel : EntityTabViewModelBase<ComplaintResultOfEmployees>
	{
		public ComplaintResultOfEmployeesViewModel(
			IEntityUoWBuilder uoWBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			
		}
	}
}
