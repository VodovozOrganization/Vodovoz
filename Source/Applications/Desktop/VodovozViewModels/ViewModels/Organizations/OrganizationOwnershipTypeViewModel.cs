using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Organizations;
using QS.DomainModel.UoW;

namespace Vodovoz.ViewModels.ViewModels.Organizations
{
	public class OrganizationOwnershipTypeViewModel : EntityTabViewModelBase<OrganizationOwnershipType>
	{
		public OrganizationOwnershipTypeViewModel(IEntityUoWBuilder uoWBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Тип формы собственности";
		}
	}
}
