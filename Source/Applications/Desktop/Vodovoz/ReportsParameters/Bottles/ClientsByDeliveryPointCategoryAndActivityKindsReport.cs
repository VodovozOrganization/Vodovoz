using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gamma.Widgets;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Client;
using QS.Dialog.GtkUI;
using QS.Project.Services;

namespace Vodovoz.ReportsParameters.Bottles
{
	public partial class ClientsByDeliveryPointCategoryAndActivityKindsReport : SingleUoWWidgetBase, IParametersWidget
	{
		DeliveryPointCategory category;
		PaymentType? paymentType;
		CounterpartyActivityKind activityKind;
		GenericObservableList<SubstringToSearch> substringsToSearch;

		public ClientsByDeliveryPointCategoryAndActivityKindsReport()
		{
			this.Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			dtrngPeriod.StartDate = DateTime.Today;
			dtrngPeriod.EndDate = DateTime.Today;
			specCmbDeliveryPointCategory.ItemsList = UoW.Session.QueryOver<DeliveryPointCategory>().List();
			specCmbDeliveryPointCategory.ItemSelected += (s, e) => category = specCmbDeliveryPointCategory.SelectedItem as DeliveryPointCategory;
			enumCmbPaymentType.ItemsEnum = typeof(PaymentType);
			enumCmbPaymentType.EnumItemSelected += (s, e) => paymentType = (PaymentType?)enumCmbPaymentType.SelectedItem;
			specCmbSubstring.ItemsList = UoW.Session.QueryOver<CounterpartyActivityKind>().List();
			specCmbSubstring.SetRenderTextFunc<CounterpartyActivityKind>(x => x.Name);
			specCmbSubstring.ItemSelected += LstCmbSubstring_ItemSelected;

			SetAccessibility();
			yTreeSubstrings.ColumnsConfig = columnsConfig;
		}

		void LstCmbSubstring_ItemSelected(object sender, ItemSelectedEventArgs e)
		{
			activityKind = specCmbSubstring.SelectedItem as CounterpartyActivityKind;
			if(activityKind != null) {
				substringsToSearch = new GenericObservableList<SubstringToSearch>(activityKind.GetListOfSubstrings());
				yTreeSubstrings.ItemsDataSource = substringsToSearch;
			} else {
				yTreeSubstrings.ItemsDataSource = substringsToSearch = null;
			}
			SetAccessibility();
		}

		void SetAccessibility()
		{
			srlWinSubstrings.Visible = specCmbSubstring.SelectedItem != null;
			yEntSubstring.Visible = specCmbSubstring.SelectedItem == null;
		}

		IColumnsConfig columnsConfig = ColumnsConfigFactory.Create<SubstringToSearch>()
														   .AddColumn("Выбрать")
																.AddToggleRenderer(n => n.Selected).Editing()
														   .AddColumn("Название")
																.AddTextRenderer(n => n.Substring)
														   .Finish();


		#region IParametersWidget implementation

		public string Title => "Клиенты по типам объектов и видам деятельности";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion IParametersWidget implementation

		void OnUpdate(bool hide = false) => LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));

		protected void OnButtonRunClicked(object sender, EventArgs e) => OnUpdate(true);

		string[] GetSubstrings(IEnumerable<string> strings)
		{
			return strings.Any() ? strings.ToArray() : new[] { "ALL" };
		}

		ReportInfo GetReportInfo()
		{
			string[] substrings = { "ALL" };
			if(activityKind == null && !string.IsNullOrEmpty(yEntSubstring.Text))
				substrings = new[] { yEntSubstring.Text };
			if(activityKind != null)
				substrings = GetSubstrings(substringsToSearch.Where(s => s.Selected).Select(s => s.Substring));
			var repInfo = new ReportInfo {
				Identifier = "Bottles.ClientsByDeliveryPointCategoryAndActivityKindsReport",
				Parameters = new Dictionary<string, object> {
					{ "start_date", dtrngPeriod.StartDate },
					{ "end_date", dtrngPeriod.EndDate },
					{ "category_id", category?.Id ?? 0},
					{ "payment_type", paymentType?.ToString() ?? "ALL" },
					{ "substrings", substrings}
				}
			};

			return repInfo;
		}
	}
}
