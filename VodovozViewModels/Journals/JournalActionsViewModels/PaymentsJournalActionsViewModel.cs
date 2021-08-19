using System;
using System.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Payments;
using Vodovoz.Repositories.Payments;

namespace Vodovoz.Journals.JournalActionsViewModels
{
	public class PaymentsJournalActionsViewModel : EntitiesJournalActionsViewModel, IDisposable
	{
		private readonly IUnitOfWork _uow;
		private DelegateCommand _completeAllocationCommand;
		
		public PaymentsJournalActionsViewModel(
			IInteractiveService interactiveService,
			IUnitOfWorkFactory unitOfWorkFactory) : base(interactiveService)
		{
			if(unitOfWorkFactory == null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}
			
			_uow = unitOfWorkFactory.CreateWithoutRoot();
		}
		
		public DelegateCommand CompleteAllocationCommand => _completeAllocationCommand ?? (_completeAllocationCommand = new DelegateCommand(
				() =>
				{
					var distributedPayments = PaymentsRepository.GetAllDistributedPayments(_uow);

					if(distributedPayments.Any()) 
					{
						foreach(var payment in distributedPayments) 
						{
							payment.Status = PaymentState.completed;
							_uow.Save(payment);
						}

						_uow.Commit();
					}
				},
				() => true
			)
		);

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}