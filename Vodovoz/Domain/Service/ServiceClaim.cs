using QSOrmProject;
using System;
using System.Data.Bindings;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Service
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "заявки на обслуживание",
		Nominative = "заявка на обслуживание")]
	public class ServiceClaim: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

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
			set	{ SetField (ref status, value, () => Status); }
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

		Payment payment;

		public virtual Payment Payment { 
			get { return payment; } 
			set	{ SetField (ref payment, value, () => Payment); }
		}

		bool repeatedService;

		public virtual bool RepeatedService { 
			get { return repeatedService; } 
			set	{ SetField (ref repeatedService, value, () => RepeatedService); }
		}

		DateTime serviceStartDate;

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

		string comment;

		public virtual string Comment { 
			get { return comment; } 
			set	{ SetField (ref comment, value, () => Comment); }
		}

		Employee engineer;

		public virtual Employee Engineer { 
			get { return engineer; } 
			set	{ SetField (ref engineer, value, () => Engineer); }
		}

		double totalPrice;

		public virtual double TotalPrice { 
			get { return totalPrice; } 
			set	{ SetField (ref totalPrice, value, () => TotalPrice); }
		}

		#region IValidatableObject implementation

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Nomenclature == null)
				yield return new ValidationResult ("Необходимо заполнить модель",
					new[] { this.GetPropertyName (o => o.Nomenclature) });
			if (Counterparty == null)
				yield return new ValidationResult ("Необходимо заполнить поле \"клиент\".",
					new[] { this.GetPropertyName (o => o.Counterparty) });
			if (DeliveryPoint == null)
				yield return new ValidationResult ("Необходимо заполнить точку доставки.", 
					new[] { this.GetPropertyName (o => o.DeliveryPoint) });
			if (String.IsNullOrWhiteSpace (Reason))
				yield return new ValidationResult ("Необходимо заполнить причину заявки.",
					new[] { this.GetPropertyName (o => o.Reason) });
		}

		#endregion

		public static IUnitOfWorkGeneric<ServiceClaim> Create (Order order)
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<ServiceClaim> ();
			uow.Root.InitialOrder = order;
			return uow;
		}
	}

	public enum ServiceClaimStatus
	{
		[ItemTitleAttribute ("Забрать у клиента")]
		PickUp,
		[ItemTitleAttribute ("На диагностике")]
		Diagnostics,
		[ItemTitleAttribute ("Ожидается оплата")]
		PaymentPending,
		[ItemTitleAttribute ("Согласование")]
		Negotiation,
		[ItemTitleAttribute ("В сервисе")]
		Service,
		[ItemTitleAttribute ("Готов")]
		Ready,
		[ItemTitleAttribute ("Отправлен клиенту")]
		SendedToClient,
		[ItemTitleAttribute ("Отправлен в сервисный центр")]
		SendedToSC,
		[ItemTitleAttribute ("Забрать из сервисного центра")]
		PickUpFromSC
	}

	public class ServiceClaimStatusStringType : NHibernate.Type.EnumStringType
	{
		public ServiceClaimStatusStringType () : base (typeof(ServiceClaimStatus))
		{
		}
	}
}

