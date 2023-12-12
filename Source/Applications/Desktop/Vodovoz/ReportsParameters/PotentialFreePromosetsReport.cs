using Gamma.ColumnConfig;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ReportsParameters
{
	[ToolboxItem(true)]
	public partial class PotentialFreePromosetsReport : SingleUoWWidgetBase, IParametersWidget
	{
		IEnumerable<PromosetReportNode> _promotionalSets;

		public PotentialFreePromosetsReport()
		{
			Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();

			buttonCreateReport.Clicked += (sender, e) => OnUpdate(false);

			ytreeview1.ColumnsConfig = FluentColumnsConfig<PromosetReportNode>.Create()
				.AddColumn("Выбрать").AddToggleRenderer(x => x.Active)
				.AddColumn("Промонабор").AddTextRenderer(x => x.Name)
				.Finish();

			_promotionalSets = (from ps in UoW.GetAll<PromotionalSet>()
								   select new PromosetReportNode
								   {
									   Id = ps.Id,
									   Name = ps.Name,
									   Active = !ps.CanBeReorderedWithoutRestriction,
								   })
								   .ToList();

			ytreeview1.ItemsDataSource = _promotionalSets;
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по потенциальным халявщикам";

		private int[] GetSelectedPromotionalSets()
		{
			if(_promotionalSets.Any(x => x.Active))
			{
				return _promotionalSets.Where(x => x.Active).Select(x => x.Id).ToArray();
			}

			//если ни один промосет не выбран, необходимо выбрать все
			return _promotionalSets.Select(x => x.Id).ToArray();
		}

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>();

			parameters.Add("start_date", dateperiodpicker.StartDate);
			parameters.Add("end_date", dateperiodpicker.EndDate);
			parameters.Add("promosets", GetSelectedPromotionalSets());

			return new ReportInfo
			{
				Identifier = "Client.PotentialFreePromosets",
				UseUserVariables = true,
				Parameters = parameters
			};
		}

		void OnUpdate(bool hide = false)
		{
			if(dateperiodpicker.StartDateOrNull == null || dateperiodpicker.EndDateOrNull == null)
			{
				MessageDialogHelper.RunWarningDialog("Необходимо ввести полный период");

				return;
			}

			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}
	}

	public class PromosetReportNode : PropertyChangedBase
	{
		private bool _active;
		public virtual bool Active
		{
			get => _active;
			set => SetField(ref _active, value);
		}

		public int Id { get; set; }

		public string Name { get; set; }
	}
}
