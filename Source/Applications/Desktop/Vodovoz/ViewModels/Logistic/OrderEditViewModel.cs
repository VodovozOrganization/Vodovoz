using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Logistic
{
	public class OrderEditViewModel : EntityTabViewModelBase<Order>
	{
		public OrderEditViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{

		}
	}
}
