using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QSOrmProject;
using Vodovoz.Domain.Orders;
using Vodovoz.Repository;
using Gamma.Utilities;

namespace Vodovoz.Domain.Service
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "заявки на обслуживание",
		Nominative = "заявка на обслуживание")]
	public class ServiceClaim: BusinessObjectBase<ServiceClaim>, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		public virtual string Title { 
			get { return String.Format ("Заявка на обслуживание №{0}", Id); }
		}

		ServiceClaimType serviceClaimType;

		public virtual ServiceClaimType ServiceClaimType {
			get { return serviceClaimType; }
			set { SetField (ref serviceClaimType, value, () => ServiceClaimType); }
		}

		Order initialOrder;

		public virtual Order InitialOrder {
			get { return initialOrder; }
			set { SetField (ref initialOrder, value, () => InitialOrder); }
		}

		Order finalOrder;

		public virtual Order FinalOrder {
			get { return finalOrder; }
			set { SetField (ref finalOrder, value, () => FinalOrder); }
		}

		ServiceClaimStatus status;

		public virtual ServiceClaimStatus Status { 
			get { return status; } 
			private set { SetField (ref status, value, () => Status); }
		}

		Nomenclature nomenclature;

		public virtual Nomenclature Nomenclature { 
			get { return nomenclature; } 
			set	{ SetField (ref nomenclature, value, () => Nomenclature); }
		}

		Equipment equipment;

		public virtual Equipment Equipment { 
			get { return equipment; } 
			set	{ SetField (ref equipment, value, () => Equipment); }
		}

		Equipment replacementEquipment;

		public virtual Equipment ReplacementEquipment{
			get{ return replacementEquipment; }
			set{ SetField (ref replacementEquipment, value, () => ReplacementEquipment); }
		}

		Counterparty counterparty;

		public virtual Counterparty Counterparty { 
			get { return counterparty; } 
			set	{ SetField (ref counterparty, value, () => Counterparty); }
		}

		DeliveryPoint deliveryPoint;

		public virtual DeliveryPoint DeliveryPoint { 
			get { return deliveryPoint; } 
			set	{ SetField (ref deliveryPoint, value, () => DeliveryPoint); }
		}

		PaymentType payment;

		public virtual PaymentType Payment { 
			get { return payment; } 
			set	{ SetField (ref payment, value, () => Payment); }
		}

		bool repeatedService;

		public virtual bool RepeatedService { 
			get { return repeatedService; } 
			set	{ SetField (ref repeatedService, value, () => RepeatedService); }
		}

		DateTime serviceStartDate = DateTime.Today;

		public virtual DateTime ServiceStartDate { 
			get { return serviceStartDate; } 
			set	{ SetField (ref serviceStartDate, value, () => ServiceStartDate); }
		}

		string kit;

		public virtual string Kit { 
			get { return kit; } 
			set	{ SetField (ref kit, value, () => Kit); }
		}

		string reason;

		public virtual string Reason { 
			get { return reason; } 
			set	{ SetField (ref reason, value, () => Reason); }
		}

		string diagnosticsResult;

		public virtual string DiagnosticsResult { 
			get { return diagnosticsResult; } 
			set	{ SetField (ref diagnosticsResult, value, () => DiagnosticsResult); }
		}

		Employee engineer;

		public virtual Employee Engineer { 
			get { return engineer; } 
			set	{ SetField (ref engineer, value, () => Engineer); }
		}

		decimal totalPrice;

		public virtual decimal TotalPrice { 
			get { return totalPrice; } 
			set	{ SetField (ref totalPrice, value, () => TotalPrice); }
		}

		public string RowColor { get { return (repeatedService ? "red" : "black"); } }

		IList<ServiceClaimItem> serviceClaimItems = new List<ServiceClaimItem> ();

		[Display (Name = "Список запчастей и работ")]
		public virtual IList<ServiceClaimItem> ServiceClaimItems {
			get { return serviceClaimItems; }
			set {
				if (SetField (ref serviceClaimItems, value, () => ServiceClaimItems))
					observableServiceClaimItems = null;
			}
		}

		GenericObservableList<ServiceClaimItem> observableServiceClaimItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public GenericObservableList<ServiceClaimItem> ObservableServiceClaimItems {
			get {
				if (observableServiceClaimItems == null)
				{
					observableServiceClaimItems = new GenericObservableList<ServiceClaimItem> (ServiceClaimItems);
					ObservableServiceClaimItems.ElementAdded += (aList, aIdx) => UpdateTotalPrice ();
					ObservableServiceClaimItems.ElementChanged += (aList, aIdx) => UpdateTotalPrice ();
					ObservableServiceClaimItems.ElementRemoved += (aList, aIdx, aObject) => UpdateTotalPrice ();
				}
				return observableServiceClaimItems;
			}
		}

		IList<ServiceClaimHistory> serviceClaimHistory = new List<ServiceClaimHistory> ();

		[Display (Name = "История заявки")]
		public virtual IList<ServiceClaimHistory> ServiceClaimHistory {
			get { return serviceClaimHistory; }
			set {
				
				if (SetField (ref serviceClaimHistory, value, () => ServiceClaimHistory))
					observableServiceClaimHistory = null;
			}
		}

		GenericObservableList<ServiceClaimHistory> observableServiceClaimHistory;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public GenericObservableList<ServiceClaimHistory> ObservableServiceClaimHistory {
			get {
				if (observableServiceClaimHistory == null)
					observableServiceClaimHistory = new GenericObservableList<ServiceClaimHistory> (ServiceClaimHistory);
				return observableServiceClaimHistory;
			}
		}


		public void UpdateTotalPrice ()
		{
			TotalPrice = 0;
			foreach (ServiceClaimItem sci in ObservableServiceClaimItems) {
				TotalPrice += sci.Total;
			}
		}

		public void AddHistoryRecord (ServiceClaimStatus status, string comment)
		{
			if(Status != status)
			{
				if (!GetAvailableNextStatusList ().Contains (status))
					throw new InvalidOperationException (String.Format ("Невозможно перейти из статуса {0} в статус {1}", Status, status));
				Status = status;
			}
			ObservableServiceClaimHistory.Add (new ServiceClaimHistory { 
				Date = DateTime.Now,
				Status = status,
				Employee = EmployeeRepository.GetEmployeeForCurrentUser (UoW),
				Comment = comment,
				ServiceClaim = this
			});
		}

		public void RemoveItem(ServiceClaimItem item)
		{
			if(Status == ServiceClaimStatus.ClosedAsOurCooler
				&& Status == ServiceClaimStatus.DeliveredToWarehouse
				&& Status == ServiceClaimStatus.PickUp
				&& Status == ServiceClaimStatus.SendedToClient
				&& Status == ServiceClaimStatus.Ready
			)
				throw new InvalidOperationException (String.Format ("Нельзя удалять позиции когда заявка находится в статусе «{0}»", Status.GetEnumTitle ()));
			ObservableServiceClaimItems.Remove (item);
		}

		#region IValidatableObject implementation

		public IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Nomenclature == null)
				yield return new ValidationResult ("Необходимо заполнить модель",
					new[] { this.GetPropertyName (o => o.Nomenclature) });
			if (Counterparty == null)
				yield return new ValidationResult ("Необходимо заполнить поле «клиент».",
					new[] { this.GetPropertyName (o => o.Counterparty) });
			if (ServiceClaimType != ServiceClaimType.JustService && DeliveryPoint == null)
				yield return new ValidationResult ("Необходимо заполнить точку доставки.", 
					new[] { this.GetPropertyName (o => o.DeliveryPoint) });
			if (String.IsNullOrWhiteSpace (Reason))
				yield return new ValidationResult ("Необходимо заполнить причину заявки.",
					new[] { this.GetPropertyName (o => o.Reason) });
			if (ServiceStartDate == default(DateTime))
				yield return new ValidationResult ("Необходимо указать дату заявки.",
					new[] { this.GetPropertyName (o => o.Reason) });
		}

		#endregion

		public List<ServiceClaimStatus> GetAvailableNextStatusList ()
		{
			List<ServiceClaimStatus> enumList = new List<ServiceClaimStatus> ();
			switch (Status) {
			case ServiceClaimStatus.DeliveredToWarehouse:
				enumList.Add (ServiceClaimStatus.Diagnostics);
				break;
			case ServiceClaimStatus.Diagnostics:
				enumList.Add (ServiceClaimStatus.Negotiation);
				enumList.Add (ServiceClaimStatus.PaymentPending);
				enumList.Add (ServiceClaimStatus.Service);
				enumList.Add (ServiceClaimStatus.SendedToSC);
				break;
			case ServiceClaimStatus.Negotiation:
				enumList.Add (ServiceClaimStatus.PaymentPending);
				enumList.Add (ServiceClaimStatus.Service);
				enumList.Add (ServiceClaimStatus.SendedToSC);
				break;
			case ServiceClaimStatus.PaymentPending:
				enumList.Add (ServiceClaimStatus.SendedToSC);
				enumList.Add (ServiceClaimStatus.Service);
				break;
			case ServiceClaimStatus.PickUpFromSC: 
				enumList.Add (ServiceClaimStatus.Ready);
				enumList.Add (ServiceClaimStatus.Negotiation);
				break;
			case ServiceClaimStatus.Ready:
				enumList.Add (ServiceClaimStatus.SendedToClient);
				enumList.Add (ServiceClaimStatus.ClosedAsOurCooler);
				break;
			case ServiceClaimStatus.SendedToSC: 
				enumList.Add (ServiceClaimStatus.PickUpFromSC);
				break;
			case ServiceClaimStatus.Service:
				enumList.Add (ServiceClaimStatus.Ready);
				enumList.Add (ServiceClaimStatus.Negotiation);
				break;
			}
			return enumList;
		}

		public ServiceClaim (ServiceClaimType type) : this()
		{
			ServiceClaimType = type;
			switch (type) {
			case ServiceClaimType.JustService:
				Status = ServiceClaimStatus.DeliveredToWarehouse;
				break;
			case ServiceClaimType.RegularService:
				Status = ServiceClaimStatus.PickUp;
				break;
			case ServiceClaimType.RepairmanCall:
				Status = ServiceClaimStatus.Diagnostics;
				break;
			}

		}

		public ServiceClaim (Order order) : this(ServiceClaimType.RegularService)
		{
			InitialOrder = order;
			Counterparty = order.Client;
			DeliveryPoint = order.DeliveryPoint;
			Status = ServiceClaimStatus.PickUp;
			ServiceStartDate = order.DeliveryDate;
		}

		public ServiceClaim ()
		{
			Reason = String.Empty;
			Kit = String.Empty;
			DiagnosticsResult = String.Empty;
		}
	}

	public enum ServiceClaimStatus
	{
		[Display (Name = "Забрать у клиента")]
		PickUp,
		[Display (Name = "Принят на склад")]
		DeliveredToWarehouse,
		[Display (Name = "На диагностике")]
		Diagnostics,
		[Display (Name = "Согласование")]
		Negotiation,
		[Display (Name = "Ожидается оплата")]
		PaymentPending,
		[Display (Name = "В ремонте")]
		Service,
		[Display (Name = "Отправлен в сервисный центр")]
		SendedToSC,
		[Display (Name = "Забрать из сервисного центра")]
		PickUpFromSC,
		[Display (Name = "Отправлен клиенту")]
		SendedToClient,
		[Display (Name = "Закрыта (Наш кулер)")]
		ClosedAsOurCooler,
		[Display (Name = "Готов")]
		Ready
	}

	public class ServiceClaimStatusStringType : NHibernate.Type.EnumStringType
	{
		public ServiceClaimStatusStringType () : base (typeof(ServiceClaimStatus))
		{
		}
	}

	public enum ServiceClaimType
	{
		[Display (Name = "Сервис (доставка и забор)")]
		RegularService,
		[Display (Name = "Только сервис")]
		JustService,
		[Display (Name = "Выезд мастера")]
		RepairmanCall
	}

	public enum ServiceClaimTypesForAdding
	{
		[Display (Name = "Только сервис")]
		JustService,
		[Display (Name = "Выезд мастера")]
		RepairmanCall
	}

	public class ServiceClaimTypeStringType : NHibernate.Type.EnumStringType
	{
		public ServiceClaimTypeStringType () : base (typeof(ServiceClaimType))
		{
		}
	}
}

