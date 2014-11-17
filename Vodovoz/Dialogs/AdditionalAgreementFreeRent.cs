using System;
using QSOrmProject;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementFreeRent : AdditionalAgreementBase
	{
		private FreeRentAgreement subject;

		public override object Subject {
			get {
				return subject;
			}
			set {
				if (value is FreeRentAgreement)
					subject = value as FreeRentAgreement;
			}
		}

		public AdditionalAgreementFreeRent(OrmParentReference parentReference)
		{
			this.Build();
			ParentReference = parentReference;
			subject = new FreeRentAgreement();
			AgreementOwner.AdditionalAgreements.Add (subject);
			ConfigureDlg();
		}

		public AdditionalAgreementFreeRent(OrmParentReference parentReference, FreeRentAgreement sub)
		{
			this.Build();
			ParentReference = parentReference;
			subject = sub;
			TabName = subject.AgreementNumber;
			ConfigureDlg();
		}

		private void ConfigureDlg(){}
	}
}

