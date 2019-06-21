using System;
using QS.Project.Filter;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using QS.RepresentationModel.GtkUI;
using Vodovoz.ViewModel;

namespace Vodovoz.Filters.ViewModels
{
	public class OrderJournalFilterViewModel : FilterViewModelBase<OrderJournalFilterViewModel>, IJournalFilter
	{
		public OrderJournalFilterViewModel(IInteractiveService interactiveService) : base(interactiveService)
		{
			DaysToBack = -CurrentUserSettings.Settings.JournalDaysToAft;
			DaysToForward = CurrentUserSettings.Settings.JournalDaysToFwd;
		}

		public int DaysToBack { get; }
		public int DaysToForward { get; }

		IRepresentationModel counterpartyRepresentationModel;
		public IRepresentationModel CounterpartyRepresentationModel {
			get {
				if(counterpartyRepresentationModel == null) {
					var filter = new CounterpartyFilter(UoW);
					counterpartyRepresentationModel = new CounterpartyVM(filter);
				}
				return counterpartyRepresentationModel;
			}
		}

		private IRepresentationModel deliveryPointRepresentationModel;
		public virtual IRepresentationModel DeliveryPointRepresentationModel {
			get => deliveryPointRepresentationModel;
			private set => SetField(ref deliveryPointRepresentationModel, value, () => DeliveryPointRepresentationModel);
		}

		private OrderStatus? restrictStatus;
		public virtual OrderStatus? RestrictStatus {
			get => restrictStatus;
			set {
				if(SetField(ref restrictStatus, value, () => RestrictStatus)) {
					Update();
				}
			}
		}

		private object[] hideStatuses;
		public virtual object[] HideStatuses {
			get => hideStatuses;
			set {
				if(SetField(ref hideStatuses, value, () => HideStatuses)) {
					Update();
				}
			}
		}

		private OrderStatus[] allowStatuses;
		public virtual OrderStatus[] AllowStatuses {
			get => allowStatuses;
			set {
				if(SetField(ref allowStatuses, value, () => AllowStatuses)) {
					Update();
				}
			}
		}

		private PaymentType? restrictPaymentType;
		public virtual PaymentType? RestrictPaymentType {
			get => restrictPaymentType;
			set {
				if(SetField(ref restrictPaymentType, value, () => RestrictPaymentType)) {
					Update();
				}
			}
		}

		private PaymentType[] allowPaymentTypes;
		public virtual PaymentType[] AllowPaymentTypes {
			get => allowPaymentTypes;
			set {
				if(SetField(ref allowPaymentTypes, value, () => AllowPaymentTypes)) {
					Update();
				}
			}
		}

		private Counterparty restrictCounterparty;
		public virtual Counterparty RestrictCounterparty {
			get => restrictCounterparty;
			set {
				if(SetField(ref restrictCounterparty, value, () => RestrictCounterparty)) {
					UpdateDeliveryPointRepresentationModel();
					Update();
				}
			}
		}

		private DeliveryPoint restrictDeliveryPoint;
		public virtual DeliveryPoint RestrictDeliveryPoint {
			get => restrictDeliveryPoint;
			set {
				if(SetField(ref restrictDeliveryPoint, value, () => RestrictDeliveryPoint)) {
					Update();
				}
			}
		}

		private DateTime? restrictStartDate;
		public virtual DateTime? RestrictStartDate {
			get => restrictStartDate;
			set {
				if(SetField(ref restrictStartDate, value, () => RestrictStartDate)) {
					Update();
				}
			}
		}

		private DateTime? restrictEndDate;
		public virtual DateTime? RestrictEndDate {
			get => restrictEndDate;
			set {
				if(SetField(ref restrictEndDate, value, () => RestrictEndDate)) {
					Update();
				}
			}
		}

		private bool restrictOnlyWithoutCoodinates;
		public virtual bool RestrictOnlyWithoutCoodinates {
			get => restrictOnlyWithoutCoodinates;
			set {
				if(SetField(ref restrictOnlyWithoutCoodinates, value, () => RestrictOnlyWithoutCoodinates)) {
					Update();
				}
			}
		}

		private bool? restrictLessThreeHours;
		public virtual bool? RestrictLessThreeHours {
			get => restrictLessThreeHours;
			set {
				if(SetField(ref restrictLessThreeHours, value, () => RestrictLessThreeHours)) {
					Update();
				}
			}
		}

		#region Selfdelivery

		private bool? restrictOnlySelfDelivery;
		public virtual bool? RestrictOnlySelfDelivery {
			get => restrictOnlySelfDelivery;
			set {
				if(SetField(ref restrictOnlySelfDelivery, value, () => RestrictOnlySelfDelivery)) {
					if(restrictOnlySelfDelivery.HasValue && restrictOnlySelfDelivery.Value && RestrictWithoutSelfDelivery.HasValue) {
						RestrictWithoutSelfDelivery = false;
					}
					Update();
				}
			}
		}

		private bool? restrictWithoutSelfDelivery;
		public virtual bool? RestrictWithoutSelfDelivery {
			get => restrictWithoutSelfDelivery;
			set {
				if(SetField(ref restrictWithoutSelfDelivery, value, () => RestrictWithoutSelfDelivery)) {
					if(restrictWithoutSelfDelivery.HasValue && restrictWithoutSelfDelivery.Value && RestrictOnlySelfDelivery.HasValue) {
						RestrictOnlySelfDelivery = false;
					}
					Update();
				}
			}
		}

		#endregion	

		#region Services

		private bool? restrictHideService;
		public virtual bool? RestrictHideService {
			get => restrictHideService;
			set {
				if(SetField(ref restrictHideService, value, () => RestrictHideService)) {
					if(restrictHideService.HasValue && restrictHideService.Value && RestrictOnlyService.HasValue) {
						RestrictOnlyService = false;
					}
					Update();
				}
			}
		}

		private bool? restrictOnlyService;
		public virtual bool? RestrictOnlyService {
			get => restrictOnlyService;
			set {
				if(SetField(ref restrictOnlyService, value, () => RestrictOnlyService)) {
					if(restrictOnlyService.HasValue && restrictOnlyService.Value && RestrictHideService.HasValue) {
						RestrictHideService = false;
					}
					Update();
				}
			}
		}

		#endregion

		private void SetDefaultValues()
		{
			RestrictHideService = true;
		}


		private void UpdateDeliveryPointRepresentationModel()
		{
			if(RestrictCounterparty == null) {
				DeliveryPointRepresentationModel = null;
				return;
			}
			DeliveryPointRepresentationModel = new ClientDeliveryPointsVM(UoW, RestrictCounterparty);
		}
	}
}
