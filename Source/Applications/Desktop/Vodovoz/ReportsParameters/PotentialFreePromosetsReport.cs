using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.Report;
using QSReport;
using Gamma.ColumnConfig;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Orders;
using NHibernate.Transform;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Services;
using Vodovoz.Core.DataService;
using Vodovoz.Parameters;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PotentialFreePromosetsReport : SingleUoWWidgetBase, IParametersWidget
	{
		IEnumerable<PromosetReportNode> promotionalSets;

		public PotentialFreePromosetsReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();

			buttonCreateReport.Clicked += (sender, e) => OnUpdate(false);
			IPotentialFreePromosetsReportDefaultsProvider defaultsValuesProvider = new BaseParametersProvider(new ParametersProvider());

			ytreeview1.ColumnsConfig = FluentColumnsConfig<PromosetReportNode>.Create()
				.AddColumn("Выбрать").AddToggleRenderer(x => x.Active)
				.AddColumn("Промонабор").AddTextRenderer(x => x.Name)
				.Finish();

			PromosetReportNode resultAlias = null;
			promotionalSets = UoW.Session.QueryOver<PromotionalSet>()
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
				)
				.TransformUsing(Transformers.AliasToBean<PromosetReportNode>())
				.List<PromosetReportNode>();

			var defaultValues = GetDefaultActivePromosets();
			if(defaultValues.Any()) {
				foreach(var ps in promotionalSets) {
					if(defaultValues.Contains(ps.Id)) {
						ps.Active = true;
					}
				}
			}

			ytreeview1.ItemsDataSource = promotionalSets;

		}

		private IEnumerable<int> GetDefaultActivePromosets()
		{
			var promosetIds = UoW.GetAll<PromotionalSet>()
				.Where(p => !p.CanBeReorderedWithoutRestriction)
				.Select(p => p.Id);

			return promosetIds;
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчет по потенциальным халявщикам";
			}
		}

		private int[] GetSelectedPromotionalSets()
		{
			if(promotionalSets.Any(x => x.Active)) {
				return promotionalSets.Where(x => x.Active).Select(x => x.Id).ToArray();
			}
			//если ни один промосет не выбран, необходимо выбрать все
			return promotionalSets.Select(x => x.Id).ToArray();
		}

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>();

			parameters.Add("start_date", dateperiodpicker.StartDate);
			parameters.Add("end_date", dateperiodpicker.EndDate);
			parameters.Add("promosets", GetSelectedPromotionalSets());

			return new ReportInfo {
				Identifier = "Client.PotentialFreePromosets",
				UseUserVariables = true,
				Parameters = parameters
			};
		}

		void OnUpdate(bool hide = false)
		{
			if(dateperiodpicker.StartDateOrNull == null || dateperiodpicker.EndDateOrNull == null) {
				MessageDialogHelper.RunWarningDialog("Необходимо ввести полный период");
				return;
			}
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}
	}

	public class PromosetReportNode : PropertyChangedBase
	{
		private bool active;
		public virtual bool Active {
			get => active;
			set => SetField(ref active, value, () => Active);
		}

		public int Id { get; set; }

		public string Name { get; set; }
	}
}
