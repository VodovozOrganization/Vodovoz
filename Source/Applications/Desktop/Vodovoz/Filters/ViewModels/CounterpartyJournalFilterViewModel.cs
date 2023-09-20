using NHibernate.Transform;
using QS.Project.Filter;
using QS.RepresentationModel.GtkUI;
using System;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Retail;
using Vodovoz.Representations;
using Vodovoz.ViewModels.Widgets.Search;

namespace Vodovoz.Filters.ViewModels
{
	public class CounterpartyJournalFilterViewModel : FilterViewModelBase<CounterpartyJournalFilterViewModel>
	{
		private CounterpartyType? _counterpartyType;
		private bool _restrictIncludeArchive;
		private Tag _tag;
		private IRepresentationModel _tagVM;
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
		private string _deliveryPointAddressLike;
		private bool _showLiquidating;
		private readonly CompositeSearchViewModel _searchByAddressViewModel;

		public CounterpartyJournalFilterViewModel()
		{
			_searchByAddressViewModel = new CompositeSearchViewModel();
			_searchByAddressViewModel.OnSearch += OnSearchByAddressViewModel;

			UpdateWith(
				x => x.CounterpartyType,
				x => x.ReasonForLeaving,
				x => x.RestrictIncludeArchive,
				x => x.ShowLiquidating,
				x => x.Tag,
				x => x.IsNeedToSendBillByEdo);
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

		public virtual IRepresentationModel TagVM
		{
			get
			{
				if(_tagVM == null)
				{
					_tagVM = new TagVM(UoW);
				}
				return _tagVM;
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
			_searchByAddressViewModel.OnSearch -= OnSearchByAddressViewModel;
			base.Dispose();
		}
	}
}
