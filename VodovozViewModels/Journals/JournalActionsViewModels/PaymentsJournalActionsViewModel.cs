using System;
using System.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Journal.Actions.ViewModels;
using QS.ViewModels;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Payments;

namespace Vodovoz.Journals.JournalActionsViewModels
{
	public class PaymentsJournalActionsViewModel : EntitiesJournalActionsViewModel, IDisposable
	{
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly IUnitOfWork _uow;
		private DelegateCommand _completeAllocationCommand;
		
		public PaymentsJournalActionsViewModel(
			IInteractiveService interactiveService,
			IUnitOfWorkFactory unitOfWorkFactory,
			IPaymentsRepository paymentsRepository) : base(interactiveService)
		{
			if(unitOfWorkFactory == null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));

			_uow = unitOfWorkFactory.CreateWithoutRoot();
		}
		
		public DelegateCommand CompleteAllocationCommand => _completeAllocationCommand ?? (_completeAllocationCommand = new DelegateCommand(
				() =>
				{
					var distributedPayments = _paymentsRepository.GetAllDistributedPayments(_uow);

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