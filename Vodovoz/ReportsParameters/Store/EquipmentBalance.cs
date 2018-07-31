using System;
using System.Collections.Generic;
using QSOrmProject;
using QSReport;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ReportsParameters.Store
{
	public partial class EquipmentBalance : Gtk.Bin, IParametersWidget
	{
		IUnitOfWork uow;

		public EquipmentBalance()
		{
			this.Build();
			uow = UnitOfWorkFactory.CreateWithoutRoot();
			yEnumCMBType.ItemsEnum = typeof(NomenclatureCategory);
			yEnumCMBType.SelectedItem = NomenclatureCategory.equipment;
			yEnumCMBType.Sensitive = false;
		}

		#region IParametersWidget implementation

		public string Title => "Оборудование на остатках";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			var nomType = yEnumCMBType.SelectedItem.ToString();
			return new ReportInfo {
				Identifier = "Store.EquipmentBalance",
				Parameters = new Dictionary<string, object>
				{
					{ "nom_type", nomType }
				}
			};
		}
	}
}
