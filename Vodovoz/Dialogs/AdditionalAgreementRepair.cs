using System;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementRepair : AdditionalAgreementBase
	{
		private RepairAgreement subject;
		public override object Subject {
			get {
				return subject;
			}
			set {
				if (value is RepairAgreement)
					subject = value as RepairAgreement;
			}
		}

		public AdditionalAgreementRepair()
		{
			this.Build();
			subject = new RepairAgreement();
			Session.Persist (subject);
			ConfigureDlg();
		}

		public AdditionalAgreementRepair(int id)
		{
			this.Build();
			subject = Session.Load<RepairAgreement>(id);
			TabName = subject.AgreementNumber;
			ConfigureDlg();
		}

		public AdditionalAgreementRepair(RepairAgreement sub)
		{
			this.Build();
			subject = Session.Load<RepairAgreement>(sub.Id);
			TabName = subject.AgreementNumber;
			ConfigureDlg();
		}

		private void ConfigureDlg(){}
	}
}

