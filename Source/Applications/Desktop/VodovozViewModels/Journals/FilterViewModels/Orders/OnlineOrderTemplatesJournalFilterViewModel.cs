using System;
using Autofac;
using QS.Navigation;
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
		private bool? _archive;
		private string _contactPhone;
		private Counterparty _counterparty;
		private DeliveryPoint _deliveryPoint;
		private OnlineOrderPaymentType? _paymentType;
		private OnlineOrderTemplateStatus? _templateStatus;

		public OnlineOrderTemplatesJournalFilterViewModel(
			ILifetimeScope lifetimeScope,
			INavigationManager navigationManager,
			ViewModelEEVMBuilder<DeliveryPoint> deliveryPointViewModelBuilder,
			DeliveryPointJournalFilterViewModel deliveryPointJournalFilterViewModel
			)
		{
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_deliveryPointViewModelBuilder = deliveryPointViewModelBuilder ?? throw new ArgumentNullException(nameof(deliveryPointViewModelBuilder));
			_deliveryPointJournalFilterViewModel =
				deliveryPointJournalFilterViewModel ?? throw new ArgumentNullException(nameof(deliveryPointJournalFilterViewModel));

			Initialize();
		}

		private void Initialize()
		{
			//PaymentTypes = (OnlineOrderPaymentType[])Enum.GetValues(typeof(OnlineOrderPaymentType));
			//Statuses = (OnlineOrderTemplateStatus[])Enum.GetValues(typeof(OnlineOrderTemplateStatus));
		}

		public JournalViewModelBase Journal { get; private set; }
		public ILifetimeScope LifetimeScope { get; private set; }
		//public IEnumerable<OnlineOrderPaymentType> PaymentTypes { get; private set; }
		//public IEnumerable<OnlineOrderTemplateStatus> Statuses { get; private set; }
		public INavigationManager NavigationManager { get; }

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
				if(SetField(ref _counterparty, value))
				{
					_deliveryPointJournalFilterViewModel.Counterparty = value;

					if(value is null)
					{
						Update();
						return;
					}

					if(DeliveryPoint != null && DeliveryPoint.Counterparty.Id != Counterparty.Id)
					{
						DeliveryPoint = null;
					}
					else
					{
						Update();
					}
				}
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
		public string ContactPhone
		{
			get => _contactPhone;
			set => SetField(ref _contactPhone, value);
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
		public OnlineOrderTemplateStatus? TemplateStatus
		{
			get => _templateStatus;
			set => UpdateFilterField(ref _templateStatus, value);
		}
		
		/// <summary>
		/// Архивные
		/// </summary>
		public bool? Archive
		{
			get => _archive;
			set => UpdateFilterField(ref _archive, value);
		}
		
		/// <summary>
		/// Вью модель поля с ТД
		/// </summary>
		public IEntityEntryViewModel DeliveryPointViewModel { get; private set; }

		/// <summary>
		/// Установка родительского журнала
		/// </summary>
		/// <param name="journal"></param>
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
			LifetimeScope = null;
			base.Dispose();
		}
	}
}
