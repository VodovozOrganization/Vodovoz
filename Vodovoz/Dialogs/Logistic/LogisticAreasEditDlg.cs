using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using GeoAPI.Geometries;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using Gtk;
using NetTopologySuite.Geometries;
using NHibernate.Util;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Repositories;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Dialogs.Logistic
{
	public partial class LogisticAreasEditDlg : QS.Dialog.Gtk.TdiTabBase
	{
		IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();

		readonly GMapOverlay bordersOverlay = new GMapOverlay("district_borders");
		readonly GMapOverlay verticeOverlay = new GMapOverlay("district_vertice");

		IList<PointLatLng> currentBorderVertice, newBorderVertice;
		GenericObservableList<LogisticsArea> observableLogisticAreas;

		LogisticsArea currentDistrict;

		bool creatingNewBorder = false;

		GeometryFactory gf = new GeometryFactory(new PrecisionModel(), 3857);

		public LogisticAreasEditDlg()
		{
			if(!UserPermissionRepository.CurrentUserPresetPermissions["can_edit_logistic_areas"]) {
				MessageDialogHelper.RunWarningDialog("Доступ запрещён!", "У вас недостаточно прав для доступа к этой вкладке. Обратитесь к своему руководителю.", Gtk.ButtonsType.Ok);
				FailInitialize = true;
				return;
			}
			this.Build();
			Configure();
		}

		void Configure()
		{
			TabName = "Логистические районы";
			ytreeDistricts.ColumnsConfig = FluentColumnsConfig<LogisticsArea>.Create()
				.AddColumn("Название").AddTextRenderer(x => x.Name).Editable()
				.AddColumn("Район города?").AddToggleRenderer(x => x.IsCity).Editing()
				.Finish();
			ytreeDistricts.SetItemsSource(ObservableLogisticAreas);
			ytreeDistricts.Selection.Changed += OnYTreeDistricts_SelectionChanged;

			ControlsAccessibility();

			// Пока кнопочки всё равно не работают.
			buttonAddVertex.Sensitive = buttonAddVertex.Visible
				= buttonMoveVertex.Sensitive = buttonMoveVertex.Visible
				= buttonRemoveVertex.Sensitive = buttonRemoveVertex.Visible
				= false;

			yenumcomboMapType.ItemsEnum = typeof(MapProviders);
			yenumcomboMapType.SelectedItem = MapProviders.YandexMap;
			yenumcomboMapType.ChangedByUser += YenumcomboMapType_ChangedByUser;
			YenumcomboMapType_ChangedByUser(null, null);

			gmapWidget.Position = new PointLatLng(59.93900, 30.31646);
			gmapWidget.HeightRequest = 150;
			gmapWidget.HasFrame = true;
			gmapWidget.Overlays.Add(bordersOverlay);
			gmapWidget.Overlays.Add(verticeOverlay);
			ShowBorders();
			ControlsAccessibility();
		}

		void YenumcomboMapType_ChangedByUser(object sender, EventArgs e)
		{
			gmapWidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders)yenumcomboMapType.SelectedItem);
		}

		protected void OnYTreeDistricts_SelectionChanged(object sender, EventArgs e)
		{
			UpdateCurrentArea();
		}

		void UpdateCurrentArea()
		{
			currentDistrict = ytreeDistricts.GetSelectedObject() as LogisticsArea;

			if(currentDistrict != null && currentDistrict.Geometry != null)
				currentBorderVertice = GetCurrentBorderVertice();
			else
				currentBorderVertice = new List<PointLatLng>();

			ShowBorderVertice(currentBorderVertice);
			ControlsAccessibility();
		}


		protected void OnYTreeSchedules_SelectionChanged(object sender, EventArgs e)
		{
			ControlsAccessibility();
		}

		protected void OnButtonAddDistrictClicked(object sender, EventArgs e)
		{
			var district = new LogisticsArea();
			observableLogisticAreas.Add(district);
			UpdateCurrentArea();
		}

		protected void OnButtonDeleteDistrictClicked(object sender, EventArgs e)
		{
			uow.Delete(currentDistrict);
			var districtToDelete = bordersOverlay.Polygons.FirstOrDefault(p => (p.Tag as LogisticsArea) == currentDistrict);
			if(districtToDelete != null)
				districtToDelete.IsVisible = false;
			observableLogisticAreas.Remove(currentDistrict);
			UpdateCurrentArea();
		}

		public virtual GenericObservableList<LogisticsArea> ObservableLogisticAreas {
			get {
				if(observableLogisticAreas == null)
					observableLogisticAreas = new GenericObservableList<LogisticsArea>(GetAllLogisticAreas());

				return observableLogisticAreas;
			}
		}

		void ControlsAccessibility()
		{
			buttonDeleteDistrict.Sensitive = buttonCreateBorder.Sensitive = ytreeDistricts.Selection.CountSelectedRows() == 1;
			buttonRemoveBorder.Sensitive = ytreeDistricts.Selection.CountSelectedRows() == 1 && currentDistrict != null && currentDistrict.Geometry != null;
		}

		IList<LogisticsArea> GetAllLogisticAreas()
		{
			var srdQuery = uow.Session.QueryOver<LogisticsArea>()
							  .List<LogisticsArea>();

			return srdQuery;
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			if(!observableLogisticAreas.Any(a => String.IsNullOrEmpty(a.Name))) {
				foreach(LogisticsArea area in observableLogisticAreas)
					uow.Save(area);

				uow.Commit();
			} else {
				MessageDialogHelper.RunWarningDialog("Нет названия", "Для района должно быть указано его название", ButtonsType.Ok);
			}
		}

		protected void OnButtonCreateBorderClicked(object sender, EventArgs e)
		{
			if(!creatingNewBorder) {
				creatingNewBorder = true;
				newBorderVertice = new List<PointLatLng>();
			} else {
				if(MessageDialogHelper.RunQuestionDialog("Завершить задание границ района?")) {
					if(MessageDialogHelper.RunQuestionDialog("Сохранить новые границы района?")) {
						var closingPoint = newBorderVertice[0];
						newBorderVertice.Add(closingPoint);
						currentBorderVertice = newBorderVertice;
						currentDistrict.Geometry = gf.CreatePolygon(GetCoordinatesFromPoints());
					}
					creatingNewBorder = false;
					ShowBorders();
					ShowBorderVertice(currentBorderVertice);
				}
			}

			ControlsAccessibility();
		}

		protected void OnButtonRemoveBorderClicked(object sender, EventArgs e)
		{
			currentDistrict.Geometry = null;
			ShowBorders();
			ShowBorderVertice(GetCurrentBorderVertice());
			ControlsAccessibility();
		}

		protected void OnButtonAddVertexClicked(object sender, EventArgs e) { }

		protected void OnButtonMoveVertexClicked(object sender, EventArgs e) { }

		protected void OnButtonRemoveVertexClicked(object sender, EventArgs e) { }

		IList<PointLatLng> GetCurrentBorderVertice()
		{
			if(currentDistrict.Geometry == null)
				return null;

			var coords = currentDistrict.Geometry.Coordinates;
			var vertice = new List<PointLatLng>();

			foreach(Coordinate coord in coords) {
				vertice.Add(new PointLatLng {
					Lat = coord.X,
					Lng = coord.Y
				});
			}

			return vertice;
		}

		void ShowBorders()
		{
			bordersOverlay.Clear();

			foreach(LogisticsArea area in observableLogisticAreas) {
				if(area.Geometry != null) {
					var border = new GMapPolygon(area.Geometry.Coordinates.Select(p => new PointLatLng(p.X, p.Y)).ToList(), area.Name);
					border.Tag = area;
					bordersOverlay.Polygons.Add(border);
				}
			}
		}

		void ShowBorderVertice(IList<PointLatLng> vertice, bool newBorder = false)
		{
			verticeOverlay.Clear();

			if(vertice == null)
				return;

			foreach(PointLatLng vertex in vertice) {
				GMapMarker point = new GMarkerGoogle(vertex, newBorder ? GMarkerGoogleType.red : GMarkerGoogleType.blue);

				verticeOverlay.Markers.Add(point);
			}
		}

		protected void OnGmapWidgetButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 1) {
				if(creatingNewBorder) {
					var point = gmapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
					newBorderVertice.Add(point);
					ShowBorderVertice(newBorderVertice, true);
				}
			}
		}

		Coordinate[] GetCoordinatesFromPoints()
		{
			IList<Coordinate> coords = new List<Coordinate>();

			foreach(PointLatLng point in currentBorderVertice) {
				coords.Add(new Coordinate {
					X = point.Lat,
					Y = point.Lng
				});
			}

			return coords.ToArray();
		}
	}
}