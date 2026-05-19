using System;
using QS.Project.Filter;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.Dialogs.Counterparties;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Orders
{
	/// <summary>
	/// Фильтр журнала шаблонов онлайн заказов
	/// </summary>
	public class OnlineOrderTemplatesJournalFilterViewModel : FilterViewModelBase<OnlineOrderTemplatesJournalFilterViewModel>
	{
		private readonly ViewModelEEVMBuilder<DeliveryPoint> _deliveryPointViewModelBuilder;
		private readonly DeliveryPointJournalFilterViewModel _deliveryPointJournalFilterViewModel;
		private int? _templateId;
		private bool _showArchived;
		private string _phone;
		private Counterparty _counterparty;
		private DeliveryPoint _deliveryPoint;
		private OnlineOrderPaymentType? _paymentType;
		private OnlineOrderTemplateStatus? _status;

		public OnlineOrderTemplatesJournalFilterViewModel(
			ViewModelEEVMBuilder<DeliveryPoint> deliveryPointViewModelBuilder,
			DeliveryPointJournalFilterViewModel deliveryPointJournalFilterViewModel
			)
		{
			_deliveryPointViewModelBuilder = deliveryPointViewModelBuilder ?? throw new ArgumentNullException(nameof(deliveryPointViewModelBuilder));
			_deliveryPointJournalFilterViewModel =
				deliveryPointJournalFilterViewModel ?? throw new ArgumentNullException(nameof(deliveryPointJournalFilterViewModel));
		}
		
		public JournalViewModelBase Journal { get; private set; }

		/// <summary>
		/// Идентификатор шаблона
		/// </summary>
		public int? TemplateId
		{
			get => _templateId;
			set => SetField(ref _templateId, value);
		}
		
		/// <summary>
		/// Клиент
		/// </summary>
		public Counterparty Counterparty
		{
			get => _counterparty;
			set
			{
				UpdateFilterField(ref _counterparty, value);
				//_deliveryPointJournalFilterViewModel.Counterparty = value;
			}
		}

		/// <summary>
		/// Точка доставки
		/// </summary>
		public DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => UpdateFilterField(ref _deliveryPoint, value);
		}
		
		/// <summary>
		/// Телефон
		/// </summary>
		public string Phone
		{
			get => _phone;
			set => SetField(ref _phone, value);
		}
		
		/// <summary>
		/// Форма оплаты
		/// </summary>
		public OnlineOrderPaymentType? PaymentType
		{
			get => _paymentType;
			set => UpdateFilterField(ref _paymentType, value);
		}
		
		/// <summary>
		/// Статус шаблона
		/// </summary>
		public OnlineOrderTemplateStatus? Status
		{
			get => _status;
			set => UpdateFilterField(ref _status, value);
		}
		
		/// <summary>
		/// Показать архивные
		/// </summary>
		public bool ShowArchived
		{
			get => _showArchived;
			set => UpdateFilterField(ref _showArchived, value);
		}
		
		public IEntityEntryViewModel DeliveryPointViewModel { get; private set; }

		public void SetJournal(JournalViewModelBase journal)
		{
			Journal = journal;
			ConfigureEntryViewModels();
		}

		private void ConfigureEntryViewModels()
		{
			if(Counterparty != null)
			{
				_deliveryPointJournalFilterViewModel.Counterparty = Counterparty;
			}

			var deliveryPointViewModel =  _deliveryPointViewModelBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(Journal)
				.ForProperty(this, x => x.DeliveryPoint)
				.UseViewModelJournalAndAutocompleter<DeliveryPointByClientJournalViewModel, DeliveryPointJournalFilterViewModel>(
					_deliveryPointJournalFilterViewModel)
				.UseViewModelDialog<DeliveryPointViewModel>()
				.Finish();

			deliveryPointViewModel.CanViewEntity = false;
			DeliveryPointViewModel = deliveryPointViewModel;
		}

		public override void Dispose()
		{
			Journal = null;
			base.Dispose();
		}
	}
}
