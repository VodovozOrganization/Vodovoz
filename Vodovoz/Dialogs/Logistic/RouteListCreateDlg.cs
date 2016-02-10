using System;
using Vodovoz.Domain;
using QSOrmProject;
using NLog;
using QSValidation;
using Gtk;
using Vodovoz.Domain.Logistic;
using System.Collections.Generic;
using QSProjectsLib;
using Vodovoz.Repository.Logistics;
using System.IO;
using QSReport;
using QSTDI;
using Gamma.Utilities;
using System.Linq;

namespace Vodovoz
{
	public partial class RouteListCreateDlg : OrmGtkDialogBase<RouteList>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		public bool transfer;

		public RouteListCreateDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<RouteList> ();
			UoWGeneric.Root.Logistican = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			if (Entity.Logistican == null) {
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать маршрутные листы, так как некого указывать в качестве логиста.");
				FailInitialize = true;
				return;
			}
			UoWGeneric.Root.Date = DateTime.Now;
			ConfigureDlg ();
		}

		public RouteListCreateDlg (RouteList routeList, IEnumerable<RouteListItem> addresses)
		{
			this.Build();
			transfer = true;
			buttonAccept.Sensitive = false;
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<RouteList>();

			Entity.Forwarder = routeList.Forwarder;
			Entity.Shift = routeList.Shift;
			Entity.Logistican = Repository.EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.Status = routeList.Status;
			for(var address=addresses.GetEnumerator();address.MoveNext();)
				Entity.AddAddressFromOrder(address.Current.Order);
			
			UoWGeneric.Root.Logistican = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			if (Entity.Logistican == null) {
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать маршрутные листы, так как некого указывать в качестве логиста.");
				FailInitialize = true;
				return;
			}
			UoWGeneric.Root.Date = DateTime.Now;
			ConfigureDlg ();
		}

		public RouteListCreateDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList> (id);
			ConfigureDlg ();
		}



		private void ConfigureDlg ()
		{
			subjectAdaptor.Target = UoWGeneric.Root;

			dataRouteList.DataSource = subjectAdaptor;

			referenceCar.SubjectType = typeof(Car);

			referenceDriver.ItemsQuery = Repository.EmployeeRepository.DriversQuery ();
			referenceDriver.PropertyMapping<RouteList> (r => r.Driver);
			referenceDriver.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));

			referenceForwarder.ItemsQuery = Repository.EmployeeRepository.ForwarderQuery ();
			referenceForwarder.PropertyMapping<RouteList> (r => r.Forwarder);
			referenceForwarder.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceForwarder.Changed += (sender, args) =>
			{
				createroutelistitemsview1.OnForwarderChanged();
			};

			referenceLogistican.Sensitive = false;
			referenceLogistican.PropertyMapping<RouteList> (r => r.Logistican);
			referenceForwarder.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));

			speccomboShift.Mappings = Entity.GetPropertyName (r => r.Shift);
			speccomboShift.ColumnMappings = PropertyUtil.GetName<DeliveryShift> (s => s.Name);
			speccomboShift.ItemsDataSource = DeliveryShiftRepository.ActiveShifts (UoW);


			referenceDriver.Sensitive = false;
			buttonPrint.Sensitive = UoWGeneric.Root.Status != RouteListStatus.New;

			createroutelistitemsview1.RouteListUoW = UoWGeneric;

			buttonAccept.Visible = (UoWGeneric.Root.Status == RouteListStatus.New || UoWGeneric.Root.Status == RouteListStatus.Ready);
			if (UoWGeneric.Root.Status == RouteListStatus.Ready) {
				var icon = new Image ();
				icon.Pixbuf = Stetic.IconLoader.LoadIcon (this, "gtk-edit", IconSize.Menu);
				buttonAccept.Image = icon;
				buttonAccept.Label = "Редактировать";
			}
			IsEditable (UoWGeneric.Root.Status == RouteListStatus.New || transfer);
		}

		public override bool Save ()
		{
			var valid = new QSValidator<RouteList> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем маршрутный лист...");
			UoWGeneric.Save ();
			logger.Info ("Ok");
			return true;
		}

		private void IsEditable (bool val = false)
		{
			speccomboShift.Sensitive = val;
			datepickerDate.Sensitive = referenceCar.Sensitive = referenceForwarder.Sensitive = val;
			spinPlannedDistance.Sensitive = val;
			createroutelistitemsview1.IsEditable (val);
		}

		protected void OnButtonAcceptClicked (object sender, EventArgs e)
		{

			if (UoWGeneric.Root.Status == RouteListStatus.New) {
				var valid = new QSValidator<RouteList> (UoWGeneric.Root, 
					            new Dictionary<object, object> {
						{ "NewStatus", RouteListStatus.Ready }
					});
				if (valid.RunDlgIfNotValid ((Window)this.Toplevel))
					return;

				UoWGeneric.Root.Status = RouteListStatus.Ready;
				Save();
				IsEditable (transfer);
				var icon = new Image ();
				icon.Pixbuf = Stetic.IconLoader.LoadIcon (this, "gtk-edit", IconSize.Menu);
				buttonAccept.Image = icon;
				buttonPrint.Sensitive = true;
				buttonAccept.Label = "Редактировать";
				return;
			}
			if (UoWGeneric.Root.Status == RouteListStatus.Ready) {
				UoWGeneric.Root.Status = RouteListStatus.New;
				IsEditable (true);
				var icon = new Image ();
				icon.Pixbuf = Stetic.IconLoader.LoadIcon (this, "gtk-edit", IconSize.Menu);
				buttonAccept.Image = icon;
				buttonPrint.Sensitive = false;
				buttonAccept.Label = "Подтвердить";
				return;
			}
		}

		protected void OnButtonPrintClicked (object sender, EventArgs e)
		{
			var RouteColumns = RouteColumnRepository.ActiveColumns (UoW);

			if (RouteColumns.Count < 1)
				throw new Exception ("В справочниках не заполнены колонки маршрутного листа. Заполните данные и повторите попытку.");

			string RdlText = String.Empty;
			using (var rdr = new StreamReader (System.IO.Path.Combine (Environment.CurrentDirectory, "Reports/RouteList.rdl"))) {
				RdlText = rdr.ReadToEnd ();
			}
			//Для уникальности номеров Textbox.
			int TextBoxNumber = 100;

			//Шаблон стандартной ячейки
			const string CellTemplate = "<TableCell><ReportItems>" +
			                            "<Textbox Name=\"Textbox{0}\">" +
			                            "<Value>{1}</Value>" +
			                            "<Style xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition\">" +
			                            "<BorderStyle><Default>Solid</Default></BorderStyle><BorderColor /><BorderWidth />" +
			                            "<TextAlign>Center</TextAlign></Style></Textbox></ReportItems></TableCell>";
			
			//Расширяем требуемые колонки на нужную ширину
			RdlText = RdlText.Replace ("<!--colspan-->", String.Format ("<ColSpan>{0}</ColSpan>", RouteColumns.Count));

			//Расширяем таблицу
			string columnsXml = "<TableColumn><Width>20pt</Width></TableColumn>";
			string columns = String.Empty;
			for (int i = 0; i < RouteColumns.Count; i++) {
				columns += columnsXml;
			}
			RdlText = RdlText.Replace ("<!--table_column-->", columns);

			//Создаем колонки, дополняем запрос и тд.
			string CellColumnHeader = String.Empty;
			string CellColumnValue = String.Empty;
			string CellColumnStock = String.Empty;
			string CellColumnTotal = String.Empty;
			string SqlSelect = String.Empty;
			string SqlSelectSubquery = String.Empty;
			string Fields = String.Empty;
			string TotalSum = "= 0";
			foreach (var column in RouteColumns) {
				//Заголовки колонок
				CellColumnHeader += String.Format (
					"<TableCell><ReportItems>" +
					"<Textbox Name=\"Textbox{0}\">" +
					"<Value>{1}</Value>" +
					"<Style xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition\">" +
					"<BorderStyle><Default>Solid</Default><Top>Solid</Top><Bottom>Solid</Bottom></BorderStyle>" +
					"<BorderColor /><BorderWidth /><FontSize>8pt</FontSize><TextAlign>Center</TextAlign></Style>" +
					"<CanGrow>true</CanGrow></Textbox></ReportItems></TableCell>", 
					TextBoxNumber++, column.Name);
				//Формула для колонки с водой для информации из запроса
				CellColumnValue += String.Format (CellTemplate,
					TextBoxNumber++, "={Water" + column.Id.ToString () + "}");
				//Ячейка с запасом. Пока там единицы.
				CellColumnStock += String.Format (CellTemplate,
					TextBoxNumber++, 1);
				//Ячейка с суммой по бутылям + запасы.
				CellColumnTotal += String.Format (CellTemplate,
					TextBoxNumber++, "=Sum({Water" + column.Id.ToString () + "}) + 1");
				//Запрос..
				SqlSelect += String.Format (", wt_qry.Water{0}", column.Id.ToString ());
				SqlSelectSubquery += String.Format (", SUM(IF(nomenclature_route_column.id = {0}, order_items.count, 0)) AS {1}",
					column.Id, "Water" + column.Id.ToString ());
				//Линкуем запрос на переменные RDL
				Fields += String.Format ("" +
				"<Field Name=\"{0}\">" +
				"<DataField>{0}</DataField>" +
				"<TypeName>System.String</TypeName>" +
				"</Field>", "Water" + column.Id.ToString ());
				//Формула итоговой суммы по всем бутялым.
				TotalSum += "+ Sum({Water" + column.Id.ToString () + "})";
			}
			RdlText = RdlText.Replace ("<!--table_cell_name-->", CellColumnHeader);
			RdlText = RdlText.Replace ("<!--table_cell_value-->", CellColumnValue);
			RdlText = RdlText.Replace ("<!--table_cell_stock-->", CellColumnStock);
			RdlText = RdlText.Replace ("<!--table_cell_total-->", CellColumnTotal);
			RdlText = RdlText.Replace ("<!--sql_select-->", SqlSelect);
			RdlText = RdlText.Replace ("<!--sql_select_subquery-->", SqlSelectSubquery);
			RdlText = RdlText.Replace ("<!--fields-->", Fields);
			RdlText = RdlText.Replace ("<!--table_cell_total_without_stock-->", TotalSum);

			var TempFile = System.IO.Path.GetTempFileName ();
			using (StreamWriter sw = new StreamWriter (TempFile)) {
				sw.Write (RdlText);
			}
			ReportInfo info = new ReportInfo ();
			info.Parameters = new Dictionary<string, object> ();
			info.Parameters.Add ("RouteListId", UoWGeneric.Root.Id);
			info.Title = "Маршрутный лист";
			info.Path = TempFile;
			var report = new QSReport.ReportViewDlg (info);
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;
			mytab.TabParent.AddSlaveTab (mytab, report);
		}
	}
}