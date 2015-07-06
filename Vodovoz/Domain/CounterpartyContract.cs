using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using QSProjectsLib;
using System.Data.Bindings.Collections.Generic;
using System.Linq;

namespace Vodovoz.Domain
{
	[OrmSubject (
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "договоры контрагента",
		Nominative = "договор",
		Genitive = " договора",
		Accusative = "договор"
	)]
	public class CounterpartyContract : PropertyChangedBase, IDomainObject
	{
		private IList<AdditionalAgreement> agreements { get; set; }

		#region IAdditionalAgreementOwner implementation

		[Display (Name = "Дополнительные соглашения")]
		public virtual IList<AdditionalAgreement> AdditionalAgreements {
			get { return agreements; }
			set { agreements = value; }
		}

		GenericObservableList<AdditionalAgreement> observableAdditionalAgreements;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<AdditionalAgreement> ObservableAdditionalAgreements {
			get {
				if (observableAdditionalAgreements == null)
					observableAdditionalAgreements = new GenericObservableList<AdditionalAgreement> (AdditionalAgreements);
				return observableAdditionalAgreements;
			}
		}

		#endregion

		public virtual int Id { get; set; }

		int maxDelay;

		[Display (Name = "Максимальный срок отсрочки")]
		public virtual int MaxDelay {
			get { return maxDelay; }
			set { SetField (ref maxDelay, value, () => MaxDelay); }
		}

		bool isArchive;

		[Display (Name = "Архивный")]
		public virtual bool IsArchive {
			get { return isArchive; }
			set { SetField (ref isArchive, value, () => IsArchive); }
		}

		bool onCancellation;

		[Display (Name = "На расторжении")]
		public virtual bool OnCancellation {
			get { return onCancellation; }
			set { SetField (ref onCancellation, value, () => OnCancellation); }
		}

		[Display (Name = "Номер")]
		public virtual string Number { get { return Id > 0 ? Id.ToString () : "Не определен"; } set { } }

		DateTime issueDate;

		[Display (Name = "Дата подписания")]
		public virtual DateTime IssueDate {
			get { return issueDate; }
			set { SetField (ref issueDate, value, () => IssueDate); }
		}

		Organization organization;

		[Required]
		[Display (Name = "Организация")]
		public virtual Organization Organization {
			get { return organization; }
			set { SetField (ref organization, value, () => Organization); }
		}

		Counterparty counterparty;

		[Required]
		[Display (Name = "Контрагент")]
		public virtual Counterparty Counterparty {
			get { return counterparty; }
			protected set { SetField (ref counterparty, value, () => Counterparty); }
		}

		public virtual string Title { 
			get { return String.Format ("Договор №{0} от {1:d}", Id, IssueDate); }
		}

		//Конструкторы
		public static IUnitOfWorkGeneric<CounterpartyContract> Create (Counterparty counterparty)
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<CounterpartyContract> ();
			uow.Root.Counterparty = counterparty;
			return uow;
		}

		/// <summary>
		/// Проверяет, не создано ли уже подобное доп. соглашение.
		/// </summary>
		/// <returns><c>true</c>, если такое доп. соглашение уже существует, <c>false</c> иначе.</returns>
		/// <param name="id">Id доп. соглашения, для исключения совпадения с самим собой.</param>
		/// <param name="deliveryPoint">Точка доставки.</param>
		public bool CheckWaterSalesAgreementExists (int id, DeliveryPoint deliveryPoint)
		{
			if (AdditionalAgreements == null || AdditionalAgreements.Count < 1) {
				return false;
			}
			if (deliveryPoint != null) {
				return AdditionalAgreements.Any (
					a => a.Id != id &&
					a.DeliveryPoint != null &&
					a.DeliveryPoint.Id == deliveryPoint.Id &&
					a.Type == AgreementType.WaterSales &&
					!a.IsCancelled);
			}
			return AdditionalAgreements.Any (
				a => a.Id != id &&
				a.DeliveryPoint == null &&
				a.Type == AgreementType.WaterSales &&
				!a.IsCancelled);
		}

		public WaterSalesAgreement GetWaterSalesAgreement (DeliveryPoint deliveryPoint)
		{
			if (AdditionalAgreements == null || AdditionalAgreements.Count < 1) {
				return null;
			}
			AdditionalAgreement agreement = null;
			agreement = AdditionalAgreements.FirstOrDefault (a => 
				a.DeliveryPoint != null &&
			a.DeliveryPoint.Id == deliveryPoint.Id &&
			a.Type == AgreementType.WaterSales &&
			!a.IsCancelled);
			if (agreement == null) {
				agreement = AdditionalAgreements.FirstOrDefault (a => 
					a.DeliveryPoint == null &&
				a.Type == AgreementType.WaterSales &&
				!a.IsCancelled);
			}
			return agreement != null ? agreement as WaterSalesAgreement : null;
		}

		public bool CheckRepairAgreementExists ()
		{
			if (AdditionalAgreements == null || AdditionalAgreements.Count < 1)
				return false;
			return AdditionalAgreements.Any (a => a.Type == AgreementType.Repair && !a.IsCancelled);
		}
	}

	public interface IContractSaved
	{
		event EventHandler<ContractSavedEventArgs> ContractSaved;
	}

	public class ContractSavedEventArgs : EventArgs
	{
		public CounterpartyContract Contract { get; private set; }

		public ContractSavedEventArgs (CounterpartyContract contract)
		{
			Contract = contract;
		}
	}
}

