using System;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Payments;
using Vodovoz.Repositories.Payments;

namespace Vodovoz.Journals.JournalActionsViewModels
{
	public class PaymentsJournalActionsViewModel : EntitiesJournalActionsViewModel
	{
		private DelegateCommand _completeAllocationCommand;
		
		public PaymentsJournalActionsViewModel(
			IInteractiveService interactiveService,
			IUnitOfWorkFactory unitOfWorkFactory) : base(interactiveService)
		{
			if(unitOfWorkFactory == null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}
			
			UoW = unitOfWorkFactory.CreateWithoutRoot();
		}
		
		public DelegateCommand CompleteAllocationCommand => _completeAllocationCommand ?? (_completeAllocationCommand = new DelegateCommand(
					() =>
					{
						var distributedPayments = PaymentsRepository.GetAllDistributedPayments(UoW);

						if(distributedPayments.Any()) 
						{
							foreach(var payment in distributedPayments) 
							{
								payment.Status = PaymentState.completed;
								UoW.Save(payment);
							}

							UoW.Commit();
						}
					},
					() => true
				)
		);
	}
}