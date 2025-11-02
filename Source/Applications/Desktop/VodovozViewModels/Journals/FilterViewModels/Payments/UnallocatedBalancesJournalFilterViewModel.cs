using Autofac;
using Gamma.Utilities;
using QS.Commands;
using QS.Dialog;
using QS.Navigation;
using QS.Project.Filter;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.Organizations;

namespace Vodovoz.Filters.ViewModels
{
	public class UnallocatedBalancesJournalFilterViewModel : FilterViewModelBase<UnallocatedBalancesJournalFilterViewModel>
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
			ViewModelEEVMBuilder<Organization> organizationViewModelEEVMBuilder,
			params Action<UnallocatedBalancesJournalFilterViewModel>[] filterParams)
		{
			if(organizationViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(organizationViewModelEEVMBuilder));
			}

			Scope = scope ?? throw new ArgumentNullException(nameof(scope));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			JournalTab = journalTab ?? throw new ArgumentNullException(nameof(journalTab));

			OrganizationViewModel = organizationViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(journalTab as DialogViewModelBase)
				.ForProperty(this, x => x.Organization)
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();

			Refilter(filterParams);
		}

		public ILifetimeScope Scope { get; }
		public INavigationManager NavigationManager { get; }
		public ITdiTab JournalTab { get; }
		public IEntityEntryViewModel OrganizationViewModel { get; }
		public override bool IsShow { get; set; } = true;

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
