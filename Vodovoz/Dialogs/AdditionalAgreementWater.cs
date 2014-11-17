using System;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementWater : AdditionalAgreementBase
	{
		private WaterSalesAgreement subject;
		public override object Subject {
			get {
				return subject;
			}
			set {
				if (value is WaterSalesAgreement)
					subject = value as WaterSalesAgreement;
			}
		}

		public AdditionalAgreementWater()
		{
			this.Build();
			subject = new WaterSalesAgreement();
			Session.Persist (subject);
			ConfigureDlg();
		}

		public AdditionalAgreementWater(int id)
		{
			this.Build();
			subject = Session.Load<WaterSalesAgreement>(id);
			TabName = subject.AgreementNumber;
			ConfigureDlg();
		}

		public AdditionalAgreementWater(WaterSalesAgreement sub)
		{
			this.Build();
			subject = Session.Load<WaterSalesAgreement>(sub.Id);
			TabName = subject.AgreementNumber;
			ConfigureDlg();
		}

		private void ConfigureDlg(){}
	}
}

