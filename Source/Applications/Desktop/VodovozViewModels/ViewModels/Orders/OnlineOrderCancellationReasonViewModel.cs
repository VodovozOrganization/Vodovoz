using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class OnlineOrderCancellationReasonViewModel : EntityTabViewModelBase<OnlineOrderCancellationReason>
	{
		public OnlineOrderCancellationReasonViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			CreatePropertyChangeRelations();
		}

		public bool CanShowId => Entity.Id > 0;
		public string IdToString => Entity.Id.ToString();

		private void CreatePropertyChangeRelations()
		{
			SetPropertyChangeRelation(
				e => e.Id,
				() => CanShowId,
				() => IdToString);
		}
	}
}
