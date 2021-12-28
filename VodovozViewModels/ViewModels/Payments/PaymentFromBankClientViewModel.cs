using System;
using QS.ViewModels;
using Vodovoz.Domain.Payments;
using QS.Project.Domain;
using QS.Services;
using QS.DomainModel.UoW;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	public class PaymentFromBankClientViewModel : EntityTabViewModelBase<Payment>
	{
		public PaymentFromBankClientViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServicies
		) : base(uowBuilder, uowFactory, commonServicies)
		{

		}
	}
}
