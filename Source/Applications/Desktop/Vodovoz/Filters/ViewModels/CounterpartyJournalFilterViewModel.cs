using Autofac;
using Gamma.Widgets;
using NHibernate.Transform;
using QS.Project.Filter;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.Domain.Retail;
using Vodovoz.EntityRepositories;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Counterparties;
using Vodovoz.ViewModels.Widgets.Search;

namespace Vodovoz.Filters.ViewModels
{
	public class CounterpartyJournalFilterViewModel : FilterViewModelBase<CounterpartyJournalFilterViewModel>
	{
		private CounterpartyType? _counterpartyType;
		private bool _restrictIncludeArchive;
		private Tag _tag;
		private bool? _isForRetail;
		private string _counterpartyName;
		private string _deliveryPointPhone;
		private string _counterpartyPhone;
		private GenericObservableList<SalesChannelSelectableNode> _salesChannels;
		private bool? _isForSalesDepartment;
		private ReasonForLeaving? _reasonForLeaving;
		private bool _isNeedToSendBillByEdo;
		private int? _counterpartyId;
		private int? _counterpartyVodovozInternalId;
		private string _counterpartyInn;
		private bool _showLiquidating;
		private CounterpartyCompositeClassification? _counterpartyClassification;
		private JournalViewModelBase _journal;
		private ClientCameFrom _clientCameFrom;
		private bool _clientCameFromIsEmpty;
		private object _selectedCameFrom;
		private readonly CompositeSearchViewModel _searchByAddressViewModel;
		private readonly ILifetimeScope _lifetimeScope;

		public CounterpartyJournalFilterViewModel(
			IGenericRepository<ClientCameFrom> clientCameFromRepository,
			ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			if(clientCameFromRepository is null)
			{
				throw new ArgumentNullException(nameof(clientCameFromRepository));
			}

			ClientCameFromCache = clientCameFromRepository.Get(UoW);

			_searchByAddressViewModel = new CompositeSearchViewModel();
			_searchByAddressViewModel.OnSearch += OnSearchByAddressViewModel;

			UpdateWith(
				x => x.CounterpartyType,
				x => x.ReasonForLeaving,
				x => x.RestrictIncludeArchive,
				x => x.ShowLiquidating,
				x => x.Tag,
				x => x.IsNeedToSendBillByEdo,
				x => x.CounterpartyClassification);
		}

		public CompositeSearchViewModel SearchByAddressViewModel => _searchByAddressViewModel;

		public virtual CounterpartyType? CounterpartyType
		{
			get => _counterpartyType;
			set => SetField(ref _counterpartyType, value);
		}

		public virtual bool RestrictIncludeArchive
		{
			get => _restrictIncludeArchive;
			set => SetField(ref _restrictIncludeArchive, value);
		}

		public bool ShowLiquidating
		{
			get => _showLiquidating;
			set => SetField(ref _showLiquidating, value);
		}

		public virtual Tag Tag
		{
			get => _tag;
			set => SetField(ref _tag, value);
		}

		public IEntityEntryViewModel TagViewModel { get; private set; }

		public JournalViewModelBase Journal
		{
			get => _journal;
			set
			{
				if(SetField(ref _journal, value) && value != null)
				{
					TagViewModel = new CommonEEVMBuilderFactory<CounterpartyJournalFilterViewModel>(_journal, this, _journal.UoW, _journal.NavigationManager, _lifetimeScope)
						.ForProperty(x => x.Tag)
						.UseViewModelJournalAndAutocompleter<TagJournalViewModel>()
						.UseViewModelDialog<TagViewModel>()
						.Finish();
				}
			}
		}

		public bool? IsForRetail
		{
			get => _isForRetail;
			set => SetField(ref _isForRetail, value);
		}

		public bool? IsForSalesDepartment
		{
			get => _isForSalesDepartment;
			set => SetField(ref _isForSalesDepartment, value);
		}

		public CounterpartyCompositeClassification? CounterpartyClassification
		{
			get => _counterpartyClassification;
			set => SetField(ref _counterpartyClassification, value);
		}

		public GenericObservableList<SalesChannelSelectableNode> SalesChannels
		{
			get
			{
				if(_salesChannels == null)
				{
					SalesChannel salesChannelAlias = null;
					SalesChannelSelectableNode salesChannelSelectableNodeAlias = null;

					var list = UoW.Session.QueryOver(() => salesChannelAlias)
						.SelectList(scList => scList
							.SelectGroup(() => salesChannelAlias.Id).WithAlias(() => salesChannelSelectableNodeAlias.Id)
							.Select(() => salesChannelAlias.Name).WithAlias(() => salesChannelSelectableNodeAlias.Name))
						.TransformUsing(Transformers.AliasToBean<SalesChannelSelectableNode>())
						.List<SalesChannelSelectableNode>();

					_salesChannels = new GenericObservableList<SalesChannelSelectableNode>(list);
					SubscribeOnCheckChanged();
				}
				return _salesChannels;
			}
		}

		public string CounterpartyName
		{
			get => _counterpartyName;
			set => SetField(ref _counterpartyName, value);
		}

		public string CounterpartyPhone
		{
			get => _counterpartyPhone;
			set => SetField(ref _counterpartyPhone, value);
		}

		public string DeliveryPointPhone
		{
			get => _deliveryPointPhone;
			set => SetField(ref _deliveryPointPhone, value);
		}

		public ReasonForLeaving? ReasonForLeaving
		{
			get => _reasonForLeaving;
			set => SetField(ref _reasonForLeaving, value);
		}

		public override bool IsShow { get; set; } = true;

		public bool IsNeedToSendBillByEdo
		{
			get => _isNeedToSendBillByEdo;
			set => SetField(ref _isNeedToSendBillByEdo, value);
		}

		public int? CounterpartyId
		{
			get => _counterpartyId;
			set => SetField(ref _counterpartyId, value);
		}

		public int? CounterpartyVodovozInternalId
		{
			get => _counterpartyVodovozInternalId;
			set => SetField(ref _counterpartyVodovozInternalId, value);
		}

		public string CounterpartyInn
		{
			get => _counterpartyInn;
			set => SetField(ref _counterpartyInn, value);
		}

		public IEnumerable<ClientCameFrom> ClientCameFromCache { get; }

		public ClientCameFrom ClientCameFrom
		{
			get => _clientCameFrom;
			set => UpdateFilterField(ref _clientCameFrom, value);
		}

		public bool ClientCameFromIsEmpty
		{
			get => _clientCameFromIsEmpty;
			set => UpdateFilterField(ref _clientCameFromIsEmpty, value);
		}

		private void UnsubscribeOnCheckChanged()
		{
			foreach(SalesChannelSelectableNode selectableSalesChannel in SalesChannels)
			{
				selectableSalesChannel.PropertyChanged -= OnStatusCheckChanged;
			}
		}

		private void SubscribeOnCheckChanged()
		{
			foreach(SalesChannelSelectableNode selectableSalesChannel in SalesChannels)
			{
				selectableSalesChannel.PropertyChanged += OnStatusCheckChanged;
			}
		}

		private void OnSearchByAddressViewModel(object sender, EventArgs e)
		{
			Update();
		}

		private void OnStatusCheckChanged(object sender, PropertyChangedEventArgs e)
		{
			Update();
		}

		public override void Dispose()
		{
			UnsubscribeOnCheckChanged();
			_searchByAddressViewModel.OnSearch -= OnSearchByAddressViewModel;
			base.Dispose();
		}
	}
}
