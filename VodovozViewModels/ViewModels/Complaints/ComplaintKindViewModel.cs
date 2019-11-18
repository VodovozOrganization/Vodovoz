using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintKindViewModel : EntityTabViewModelBase<ComplaintKind>
	{
		public ComplaintKindViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Виды жалоб";
		}
	}
}
