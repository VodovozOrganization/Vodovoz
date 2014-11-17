using System;

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

		public AdditionalAgreementNonFreeRent()
		{
			this.Build();
			subject = new NonfreeRentAgreement();
			Session.Persist (subject);
			ConfigureDlg();
		}

		public AdditionalAgreementNonFreeRent(int id)
		{
			this.Build();
			subject = Session.Load<NonfreeRentAgreement>(id);
			TabName = subject.AgreementNumber;
			ConfigureDlg();
		}

		public AdditionalAgreementNonFreeRent(NonfreeRentAgreement sub)
		{
			this.Build();
			subject = Session.Load<NonfreeRentAgreement>(sub.Id);
			TabName = subject.AgreementNumber;
			ConfigureDlg();
		}

		private void ConfigureDlg(){}
	}
}

