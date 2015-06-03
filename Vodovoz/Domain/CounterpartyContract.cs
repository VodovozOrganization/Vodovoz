using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using QSProjectsLib;

namespace Vodovoz.Domain
{
	[OrmSubject (
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "договоры контрагента",
		Nominative = "договор",
		Genitive = " договора",
		Accusative = "договор"
	)]
	public class CounterpartyContract : PropertyChangedBase, IDomainObject, IAdditionalAgreementOwner, ISpecialRowsRender
	{
		private IList<AdditionalAgreement> agreements { get; set; }

		#region IAdditionalAgreementOwner implementation

		[Display(Name = "Дополнительные соглашения")]
		public virtual IList<AdditionalAgreement> AdditionalAgreements {
			get { return agreements; }
			set { agreements = value; }
		}

		#endregion

		public virtual int Id { get; set; }

		int maxDelay;
		[Display(Name = "Максимальный срок отсрочки")]
		public virtual int MaxDelay {
			get { return maxDelay; }
			set { SetField (ref maxDelay, value, () => MaxDelay); }
		}

		bool isNew;

		public virtual bool IsNew {
			get { return isNew; }
			set { SetField (ref isNew, value, () => IsNew); }
		}

		bool isArchive;
		[Display(Name = "Архивный")]
		public virtual bool IsArchive {
			get { return isArchive; }
			set { SetField (ref isArchive, value, () => IsArchive); }
		}

		bool onCancellation;
		[Display(Name = "На расторжении")]
		public virtual bool OnCancellation {
			get { return onCancellation; }
			set { SetField (ref onCancellation, value, () => OnCancellation); }
		}

		[Display(Name = "Номер")]
		public virtual string Number { get { return Id > 0 ? Id.ToString () : "Не определен"; } set { } }

		DateTime issueDate;
		[Display(Name = "Дата подписания")]
		public virtual DateTime IssueDate {
			get { return issueDate; }
			set { SetField (ref issueDate, value, () => IssueDate); }
		}

		Organization organization;
		[Required]
		[Display(Name = "Организация")]
		public virtual Organization Organization {
			get { return organization; }
			set { SetField (ref organization, value, () => Organization); }
		}

		Counterparty counterparty;
		[Required]
		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty {
			get { return counterparty; }
			protected set { SetField (ref counterparty, value, () => Counterparty); }
		}

		#region ISpecialRowsRender implementation

		public string TextColor {
			get {
				if (IsArchive)
					return "grey";
				if (OnCancellation)
					return "blue";
				return "black";
					
			}
		}

		#endregion

		public virtual string Title { 
			get { return String.Format ("Договор №{0} от {1:d}", Id, IssueDate); }
		}

		public virtual string OrganizationTitle { 
			get { return Organization != null ? Organization.FullName : "Не указана"; }
		}

		public virtual string AdditionalAgreementsCount { 
			get { return AdditionalAgreements != null ? AdditionalAgreements.Count.ToString () : "0"; }
		}


		//Конструкторы
		public static IUnitOfWorkGeneric<CounterpartyContract> Create(Counterparty counterparty)
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<CounterpartyContract> ();
			uow.Root.Counterparty = counterparty;
			return uow;
		}
	}

	public interface IAdditionalAgreementOwner
	{
		IList<AdditionalAgreement> AdditionalAgreements { get; set; }
	}
}

