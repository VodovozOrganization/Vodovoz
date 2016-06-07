using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Orders.Documents
{
	public class OrderAgreement : OrderDocument
	{
		#region implemented abstract members of OrderDocument

		public override OrderDocumentType Type {
			get {
				return OrderDocumentType.AdditionalAgreement;	
			}
		}

		#endregion

		AdditionalAgreement additionalAgreement;

		[Display (Name = "Доп. соглашение")]
		public virtual AdditionalAgreement AdditionalAgreement {
			get { return additionalAgreement; }
			set { SetField (ref additionalAgreement, value, () => AdditionalAgreement); }
		}			

		public override string Name {
			get {
				return String.Format ("Доп. соглашение {0} №{1}", 
					additionalAgreement.AgreementTypeTitle, 
					additionalAgreement.AgreementNumber);
			}
		}

		public override string DocumentDate {
			get { return AdditionalAgreement.DocumentDate; }
		}
			
	}
}

