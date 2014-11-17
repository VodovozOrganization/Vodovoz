using System;

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

		public AdditionalAgreementFreeRent()
		{
			this.Build();
			subject = new FreeRentAgreement();
			Session.Persist (subject);
			ConfigureDlg();
		}

		public AdditionalAgreementFreeRent(int id)
		{
			this.Build();
			subject = Session.Load<FreeRentAgreement>(id);
			TabName = subject.AgreementNumber;
			ConfigureDlg();
		}

		public AdditionalAgreementFreeRent(FreeRentAgreement sub)
		{
			this.Build();
			subject = Session.Load<FreeRentAgreement>(sub.Id);
			TabName = subject.AgreementNumber;
			ConfigureDlg();
		}

		private void ConfigureDlg(){}
	}
}

