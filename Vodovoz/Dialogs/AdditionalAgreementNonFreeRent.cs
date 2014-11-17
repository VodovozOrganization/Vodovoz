using System;
using QSOrmProject;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementNonFreeRent : AdditionalAgreementBase
	{
		private NonfreeRentAgreement subject;
		public override object Subject {
			get {
				return subject;
			}
			set {
				if (value is NonfreeRentAgreement)
					subject = value as NonfreeRentAgreement;
			}
		}
			
		public AdditionalAgreementNonFreeRent(OrmParentReference parentReference)
		{
			this.Build();
			ParentReference = parentReference;
			subject = new NonfreeRentAgreement();
			AgreementOwner.AdditionalAgreements.Add (subject);
			ConfigureDlg();
		}

		public AdditionalAgreementNonFreeRent(OrmParentReference parentReference, NonfreeRentAgreement sub)
		{
			this.Build();
			ParentReference = parentReference;
			subject = sub;
			TabName = subject.AgreementType + " " + subject.AgreementNumber;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			adaptor.Target = subject;
			datatable1.DataSource = adaptor;
			entryAgreementNumber.IsEditable = true;
		}
	}
}

