using NHibernate.Transform;
using QS.Project.Filter;
using QS.Project.Journal;
using QS.RepresentationModel.GtkUI;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Retail;
using Vodovoz.Representations;

namespace Vodovoz.Filters.ViewModels
{
	public class CounterpartyJournalFilterViewModel : FilterViewModelBase<CounterpartyJournalFilterViewModel>, IJournalFilter, IJournalFilterViewModel
	{
		private string _counterpartyName;
		private string _deliveryPointPhone;
		private string _counterpartyPhone;
		private GenericObservableList<SalesChannelSelectableNode> _salesChannels;
		private bool? _isForSalesDepartment;
		private ReasonForLeaving? _reasonForLeaving;
		private bool _isNeedToSendBillByEdo;

		public CounterpartyJournalFilterViewModel()
		{
			UpdateWith(
				x => x.CounterpartyType,
				x => x.ReasonForLeaving,
				x => x.RestrictIncludeArchive,
				x => x.Tag,
				x => x.IsNeedToSendBillByEdo
			);
		}

		private CounterpartyType? counterpartyType;
		public virtual CounterpartyType? CounterpartyType {
			get => counterpartyType;
			set => SetField(ref counterpartyType, value, () => CounterpartyType);
		}

		private bool restrictIncludeArchive;
		public virtual bool RestrictIncludeArchive {
			get => restrictIncludeArchive;
			set => SetField(ref restrictIncludeArchive, value, () => RestrictIncludeArchive);
		}

		private Tag tag;
		public virtual Tag Tag {
			get => tag;
			set => SetField(ref tag, value, () => Tag);
		}

		private IRepresentationModel tagVM;
		public virtual IRepresentationModel TagVM {
			get {
				if(tagVM == null) {
					tagVM = new TagVM(UoW);
				}
				return tagVM;
			}
		}

		private bool? isForRetail;

		public bool? IsForRetail
		{
			get => isForRetail;
			set => SetField(ref isForRetail, value);
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
							.Select(() => salesChannelAlias.Name).WithAlias(() => salesChannelSelectableNodeAlias.Name)
						).TransformUsing(Transformers.AliasToBean<SalesChannelSelectableNode>()).List<SalesChannelSelectableNode>();

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
		public bool IsShow { get; set; }

		public bool IsNeedToSendBillByEdo
		{
			get => _isNeedToSendBillByEdo;
			set => SetField(ref _isNeedToSendBillByEdo, value);
		}

		private void UnsubscribeOnCheckChanged()
		{
			foreach (SalesChannelSelectableNode selectableSalesChannel in SalesChannels)
			{
				selectableSalesChannel.PropertyChanged -= OnStatusCheckChanged;
			}
		}

		private void SubscribeOnCheckChanged()
		{
			foreach (SalesChannelSelectableNode selectableSalesChannel in SalesChannels)
			{
				selectableSalesChannel.PropertyChanged += OnStatusCheckChanged;
			}
		}

		private void OnStatusCheckChanged(object sender, PropertyChangedEventArgs e)
		{
			Update();
		}
	}
}
