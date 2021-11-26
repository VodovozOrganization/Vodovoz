using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintResultOfCounterpartyViewModel : EntityTabViewModelBase<ComplaintResultOfCounterparty>
	{
		public ComplaintResultOfCounterpartyViewModel(
			IEntityUoWBuilder uoWBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			
		}
	}
}
