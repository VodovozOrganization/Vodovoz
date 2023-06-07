using System;
using System.Linq;
using Autofac;
using QS.Navigation;
using QS.Project.Filter;
using QS.Project.Journal;
using QS.Tdi;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;
using QS.Commands;
using QS.Dialog;
using Gamma.Utilities;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Filters.ViewModels
{
	public class UnallocatedBalancesJournalFilterViewModel : FilterViewModelBase<UnallocatedBalancesJournalFilterViewModel>, IJournalFilterViewModel
	{
		private readonly IInteractiveMessage _interactiveMessage;
		private Counterparty _counterparty;
		private Organization _organization;
		private DelegateCommand _helpCommand;

		public UnallocatedBalancesJournalFilterViewModel(
			ILifetimeScope scope,
			INavigationManager navigationManager,
			IInteractiveMessage interactiveMessage,
			ITdiTab journalTab,
			params Action<UnallocatedBalancesJournalFilterViewModel>[] filterParams)
		{
			Scope = scope ?? throw new ArgumentNullException(nameof(scope));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			JournalTab = journalTab ?? throw new ArgumentNullException(nameof(journalTab));
			Refilter(filterParams);
		}

		public ILifetimeScope Scope { get; }
		public INavigationManager NavigationManager { get; }
		public ITdiTab JournalTab { get; }

		public bool IsShow { get; set; } = true;

		public Counterparty Counterparty
		{
			get => _counterparty;
			set => UpdateFilterField(ref _counterparty, value);
		}

		public Organization Organization
		{
			get => _organization;
			set => UpdateFilterField(ref _organization, value);
		}

		public DelegateCommand HelpCommand => _helpCommand ?? (_helpCommand = new DelegateCommand(
				() =>
				{
					_interactiveMessage.ShowMessage(
						ImportanceLevel.Info,
						"В журнал попадают клиенты у которых есть нераспределенный баланс и сумма долга больше 0\n" +
						"Сумма долга рассчитывается по заказам у которых:\n" +
						$"- форма оплаты {PaymentType.Cashless.GetEnumTitle()}\n" +
						$"- статус оплаты отличен от {OrderPaymentStatus.Paid.GetEnumTitle()}\n" +
						"- сумма заказа больше 0\n" +
						"Сортировка по убыванию суммы баланса клиента");
				}
			)
		);

		private void Refilter(Action<UnallocatedBalancesJournalFilterViewModel>[] filterParams)
		{
			if(filterParams.Any())
			{
				SetAndRefilterAtOnce(filterParams);
			}
		}
	}
}
