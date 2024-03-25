using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class OrderRatingReasonViewModel : DialogViewModelBase
	{
		private readonly IUnitOfWorkGeneric<OrderRatingReason> _uowGeneric;
		
		public OrderRatingReasonViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			INavigationManager navigation) : base(navigation)
		{
			if(uowBuilder is null)
			{
				throw new ArgumentNullException(nameof(uowBuilder));
			}
			
			_uowGeneric = uowBuilder.CreateUoW<OrderRatingReason>(uowFactory);
			Title = Entity.ToString();
		}

		public OrderRatingReason Entity => _uowGeneric.Root;
		
		public string IdToString => Entity.Id.ToString();
		
	}
}
