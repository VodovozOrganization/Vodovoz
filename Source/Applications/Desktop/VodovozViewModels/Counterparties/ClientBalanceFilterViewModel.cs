using Autofac;
using QS.Project.Filter;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Counterparties
{
	public class ClientBalanceFilterViewModel : FilterViewModelBase<ClientBalanceFilterViewModel>
	{
		private ILifetimeScope _lifetimeScope;
		private Nomenclature _restrictNomenclature;
		private Nomenclature _nomenclature;
		private bool _canChangeNomenclature;
		private Counterparty _restrictCounterparty;
		private DeliveryPoint _restrictDeliveryPoint;
		private bool _restrictIncludeSold;
		private ClientEquipmentBalanceJournalViewModel _journal;
		private bool _canChangeDeliveryPoint;
		private DeliveryPoint _deliveryPoint;

		public ClientBalanceFilterViewModel(
			ILifetimeScope lifetimeScope,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory)
		{
			_lifetimeScope = lifetimeScope
				?? throw new System.ArgumentNullException(nameof(lifetimeScope));
			CounterpartyJournalFactory = counterpartyJournalFactory
				?? throw new System.ArgumentNullException(nameof(counterpartyJournalFactory));
			DeliveryPointJournalFactory = deliveryPointJournalFactory
				?? throw new System.ArgumentNullException(nameof(deliveryPointJournalFactory));
		}

		public Counterparty RestrictCounterparty
		{
			get => _restrictCounterparty;
			set => UpdateFilterField(ref _restrictCounterparty, value);
		}

		public Nomenclature RestrictNomenclature
		{
			get => _restrictNomenclature;
			set
			{
				_restrictNomenclature = value;
				Nomenclature = value;
				CanChangeNomenclature = value is null;
			}
		}

		public Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => UpdateFilterField(ref _nomenclature, value);
		}

		public bool CanChangeNomenclature
		{
			get => _canChangeNomenclature;
			set => UpdateFilterField(ref _canChangeNomenclature, value);
		}

		public DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => UpdateFilterField(ref _deliveryPoint, value);
		}

		public DeliveryPoint RestrictDeliveryPoint
		{
			get => _restrictDeliveryPoint;
			set
			{
				_restrictDeliveryPoint = value;
				DeliveryPoint = value;
				CanChangeDeliveryPoint = value is null;
			}
		}

		public bool CanChangeDeliveryPoint
		{
			get => _canChangeDeliveryPoint;
			set => UpdateFilterField(ref _canChangeDeliveryPoint, value);
		}

		public bool RestrictIncludeSold
		{
			get => _restrictIncludeSold;
			set => UpdateFilterField(ref _restrictIncludeSold, value);
		}

		public IEntityEntryViewModel NomenclatureViewModel { get; private set; }

		public ClientEquipmentBalanceJournalViewModel Journal
		{
			get => _journal;
			set
			{
				if(_journal != null)
				{
					return;
				}

				_journal = value;

				NomenclatureViewModel = new CommonEEVMBuilderFactory<ClientBalanceFilterViewModel>(_journal, this, UoW, _journal.NavigationManager, _lifetimeScope)
					.ForProperty(x => x.Nomenclature)
					.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel>()
					.UseViewModelDialog<NomenclatureViewModel>()
					.Finish();
			}
		}

		public ICounterpartyJournalFactory CounterpartyJournalFactory { get; }
		public IDeliveryPointJournalFactory DeliveryPointJournalFactory { get; }

		public ILifetimeScope LifetimeScope => _lifetimeScope;

		public override void Dispose()
		{
			_journal = null;
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
