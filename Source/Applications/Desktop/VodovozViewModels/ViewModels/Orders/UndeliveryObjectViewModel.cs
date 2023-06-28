using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class UndeliveryObjectViewModel : EntityTabViewModelBase<UndeliveryObject>
	{
		public UndeliveryObjectViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory uowFactory, ICommonServices commonServices)
			: base(uowBuilder, uowFactory, commonServices)
		{
		}
	}
}
