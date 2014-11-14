using System;
using QSOrmProject;
using System.Collections.Generic;

namespace Vodovoz
{
	[OrmSubjectAttibutes ("Договоры контрагента")]
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
		public virtual string Number { get; set; }
		public virtual DateTime IssueDate { get; set; }
		public virtual Organization Organization { get; set; }

		public CounterpartyContract ()
		{
			Number = String.Empty;
		}
	}
}

