using System;
using QSOrmProject;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubject("Договоры контрагента")]
	public class CounterpartyContract : IDomainObject, IAdditionalAgreementOwner
	{
		private IList<AdditionalAgreement> agreements { get; set; }

		#region IAdditionalAgreementOwner implementation

		public virtual IList<AdditionalAgreement> AdditionalAgreements {
			get {
				return agreements;
			}
			set {
				agreements = value;
			}
		}
		#endregion

		public virtual int Id { get; set; }
		public virtual int MaxDelay { get; set; }
		public virtual bool IsArchive { get; set; }
		public virtual bool OnCancellation { get; set; }
		[Required(ErrorMessage = "Название контрагента должно быть заполнено.")]
		public virtual string Number { get; set; }
		public virtual DateTime IssueDate { get; set; }
		public virtual Organization Organization { get; set; }

		public CounterpartyContract ()
		{
			Number = String.Empty;
		}
	}

	public interface IAdditionalAgreementOwner
	{
		IList<AdditionalAgreement> AdditionalAgreements { get; set;}
	}
}

