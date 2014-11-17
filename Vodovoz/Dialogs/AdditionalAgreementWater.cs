using System;
using QSOrmProject;

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
			
		public AdditionalAgreementWater(OrmParentReference parentReference)
		{
			this.Build();
			ParentReference = parentReference;
			subject = new WaterSalesAgreement();
			AgreementOwner.AdditionalAgreements.Add (subject);
			ConfigureDlg();
		}

		public AdditionalAgreementWater(OrmParentReference parentReference, WaterSalesAgreement sub)
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

		public override bool Save ()
		{
			if (entryAgreementNumber.Text == String.Empty) {
				logger.Warn("В доп. соглашении не заполнен номер - не сохраняем.");
				return false;
			}
			OrmMain.DelayedNotifyObjectUpdated(ParentReference.ParentObject, subject);
			return true;
		}
	}
}

