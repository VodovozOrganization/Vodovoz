using System;
using QSOrmProject;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubject ("Договоры контрагента")]
	public class CounterpartyContract : PropertyChangedBase, IDomainObject, IAdditionalAgreementOwner
	{
		private IList<AdditionalAgreement> agreements { get; set; }

		#region IAdditionalAgreementOwner implementation

		public virtual IList<AdditionalAgreement> AdditionalAgreements {
			get { return agreements; }
			set { agreements = value; }
		}

		#endregion

		public virtual int Id { get; set; }

		int maxDelay;

		public virtual int MaxDelay {
			get { return maxDelay; }
			set { SetField (ref maxDelay, value, () => MaxDelay); }
		}

		bool isArchive;

		public virtual bool IsArchive {
			get { return isArchive; }
			set { SetField (ref isArchive, value, () => IsArchive); }
		}

		bool onCancellation;

		public virtual bool OnCancellation {
			get { return onCancellation; }
			set { SetField (ref onCancellation, value, () => OnCancellation); }
		}

		string number;

		[Required (ErrorMessage = "Название контрагента должно быть заполнено.")]
		public virtual string Number {
			get { return number; }
			set { SetField (ref number, value, () => Number); }
		}

		DateTime issueDate;

		public virtual DateTime IssueDate {
			get { return issueDate; }
			set { SetField (ref issueDate, value, () => IssueDate); }
		}

		Organization organization;

		public virtual Organization Organization {
			get { return organization; }
			set { SetField (ref organization, value, () => Organization); }
		}

		Counterparty counterparty;

		public virtual Counterparty Counterparty {
			get { return counterparty; }
			set { SetField (ref counterparty, value, () => Counterparty); }
		}

		public CounterpartyContract ()
		{
			Number = String.Empty;
		}
	}

	public interface IAdditionalAgreementOwner
	{
		IList<AdditionalAgreement> AdditionalAgreements { get; set; }
	}
}

