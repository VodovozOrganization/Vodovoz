using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Binding;
using Gamma.ColumnConfig;
using GeoAPI.Geometries;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using Gtk;
using NetTopologySuite.Geometries;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using QS.Tdi;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Dialogs.Logistic
{
	public partial class ScheduleRestrictedDistrictsDlg : QS.Dialog.Gtk.TdiTabBase
	{
		IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();

		readonly GMapOverlay bordersOverlay = new GMapOverlay("district_borders");
		readonly GMapOverlay verticeOverlay = new GMapOverlay("district_vertice");

		IList<PointLatLng> currentBorderVertice, newBorderVertice;
		GenericObservableList<ScheduleRestrictedDistrict> observableRestrictedDistricts;

		ScheduleRestrictedDistrict currentDistrict = new ScheduleRestrictedDistrict();

		ILevelConfig[] levelConfig;
		bool creatingNewBorder = false;

		GeometryFactory gf = new GeometryFactory(new PrecisionModel(), 3857);

		public ScheduleRestrictedDistrictsDlg()
		{
			this.Build();
			Configure();
		}

		void Configure()
		{
			TabName = "Районы с графиками доставки";
			ytreeDistricts.ColumnsConfig = FluentColumnsConfig<ScheduleRestrictedDistrict>.Create()
				.AddColumn("Название").AddTextRenderer(x => x.DistrictName).Editable()
				.AddColumn("Мин. бутылей").AddNumericRenderer(x => x.MinBottles)
				.Adjustment(new Adjustment(1, 0, 1000, 1, 100, 1)).Editing()
				.AddColumn("Ценообразование").AddEnumRenderer(x => x.PriceType).Editing()
				.AddColumn("Цена воды").AddNumericRenderer(x => x.WaterPrice).Digits(2)
				.Adjustment(new Adjustment(0, 0, 100000, 1, 100, 1))
				.AddSetter((c, row) => c.Editable = row.PriceType == DistrictWaterPrice.FixForDistrict)
				.Finish();
			ytreeDistricts.SetItemsSource(ObservableRestrictedDistricts);
			ytreeDistricts.Selection.Changed += OnYTreeDistricts_SelectionChanged;

			ytreeSchedules.Selection.Mode = Gtk.SelectionMode.Single;
			ytreeSchedules.ColumnsConfig = FluentColumnsConfig<DeliverySchedule>.Create()
				.AddColumn("График").AddTextRenderer(x => x.Name)
				.Finish();
			ytreeSchedules.Selection.Changed += OnYTreeSchedules_SelectionChanged;

			ButtonsSensitivity();

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
		}

		private void ObservableItemsField_ListContentChanged(object sender, EventArgs e)
		{
			ytreeSchedules.QueueDraw();
		}
		
		void YenumcomboMapType_ChangedByUser(object sender, EventArgs e)
		{
			gmapWidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders)yenumcomboMapType.SelectedItem);
		}

		protected void OnYTreeDistricts_SelectionChanged(object sender, EventArgs e)
		{
			UpdateCurrentDistrict();
		}

		void UpdateCurrentDistrict()
		{
			currentDistrict = ytreeDistricts.GetSelectedObject() as ScheduleRestrictedDistrict;

			if(currentDistrict != null) {
				btnMonday.Click();
			}

			if(currentDistrict != null && currentDistrict.DistrictBorder != null) {
				currentBorderVertice = GetCurrentBorderVertice();
			} else {
				currentBorderVertice = new List<PointLatLng>();

			}

			ShowBorderVertice(currentBorderVertice);
			ButtonsSensitivity();
		}


		protected void OnYTreeSchedules_SelectionChanged(object sender, EventArgs e)
		{
			ButtonsSensitivity();
		}

		protected void OnButtonAddDistrictClicked(object sender, EventArgs e)
		{
			var district = new ScheduleRestrictedDistrict();
			observableRestrictedDistricts.Add(district);
			UpdateCurrentDistrict();
		}

		protected void OnButtonDeleteDistrictClicked(object sender, EventArgs e)
		{
			currentDistrict.Remove(uow);
			var districtToDelete = bordersOverlay.Polygons.FirstOrDefault(p => (p.Tag as ScheduleRestrictedDistrict) == currentDistrict);
			if(districtToDelete != null)
				districtToDelete.IsVisible = false;
			observableRestrictedDistricts.Remove(currentDistrict);
			UpdateCurrentDistrict();
		}

		protected void OnButtonAddScheduleClicked(object sender, EventArgs e)
		{
			var SelectSchedules = new OrmReference(typeof(DeliverySchedule), uow);
			SelectSchedules.Mode = OrmReferenceMode.MultiSelect;
			SelectSchedules.ObjectSelected += SelectSchedules_ObjectSelected;
			TabParent.AddSlaveTab(this, SelectSchedules);
		}

		void SelectSchedules_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var scheduleList = (ytreeSchedules.ItemsDataSource as GenericObservableList<DeliverySchedule>);
			if(scheduleList == null) {
				return;
			}
			foreach(var item in e.Subjects) {
				var schedule = (item as DeliverySchedule);
				if(schedule != null && !scheduleList.Any(x => x.Id == schedule.Id)) {
					scheduleList.Add(schedule);
				}
			}
		}

		protected void OnButtonDeleteScheduleClicked(object sender, EventArgs e)
		{
			var selectedObj = ytreeSchedules.GetSelectedObject() as DeliverySchedule;
			var scheduleList = (ytreeSchedules.ItemsDataSource as GenericObservableList<DeliverySchedule>);
			if(selectedObj != null && scheduleList != null) {
				scheduleList.Remove(selectedObj);
			}
		}

		public virtual GenericObservableList<ScheduleRestrictedDistrict> ObservableRestrictedDistricts {
			get {
				if(observableRestrictedDistricts == null) {
					observableRestrictedDistricts = new GenericObservableList<ScheduleRestrictedDistrict>(GetAllDistricts());				
				}
				return observableRestrictedDistricts;
			}
		}

		void OnObservableRestrictedDistricts_ElementAdded(object sender,  int[] aIdx)
		{
			ytreeDistricts.SetItemsSource(ObservableRestrictedDistricts);
		}

		void OnObservableRestrictedDistricts_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			ytreeDistricts.SetItemsSource(ObservableRestrictedDistricts);
		}

		void OnObservableRestrictions_ElementAdded(object sender, int[] aIdx)
		{
			UpdateCurrentDistrict();
		}

		void OnObservableRestrictions_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			UpdateCurrentDistrict();
		}

		void ButtonsSensitivity()
		{
			buttonDeleteDistrict.Sensitive = buttonCreateBorder.Sensitive = ytreeDistricts.Selection.CountSelectedRows() == 1;
			buttonRemoveBorder.Sensitive = ytreeDistricts.Selection.CountSelectedRows() == 1 &&  currentDistrict != null && currentDistrict.DistrictBorder != null;
			buttonAddSchedule.Sensitive = currentDistrict != null;
		}

		IList<ScheduleRestrictedDistrict> GetAllDistricts()
		{
			var srdQuery = uow.Session.QueryOver<ScheduleRestrictedDistrict>()
			                  .List<ScheduleRestrictedDistrict>();
			
			return srdQuery;
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			foreach(ScheduleRestrictedDistrict district in observableRestrictedDistricts)
			{
				district.Save(uow);
			}
			uow.Commit();
		}

		protected void OnButtonCreateBorderClicked(object sender, EventArgs e)
		{
			if(!creatingNewBorder)
			{
				creatingNewBorder = true;
				newBorderVertice = new List<PointLatLng>();
			} else
			{
				if(MessageDialogWorks.RunQuestionDialog("Завершить задание границ района?"))
				{
					if(MessageDialogWorks.RunQuestionDialog("Сохранить новые границы района?"))
					{
						var closingPoint = newBorderVertice[0];
						newBorderVertice.Add(closingPoint);
						currentBorderVertice = newBorderVertice;
						currentDistrict.DistrictBorder = gf.CreatePolygon(GetCoordinatesFromPoints());
					}
					creatingNewBorder = false;
					ShowBorders();
					ShowBorderVertice(currentBorderVertice);
				}
			}

			ButtonsSensitivity();
		}

		protected void OnButtonRemoveBorderClicked(object sender, EventArgs e)
		{
			currentDistrict.DistrictBorder = null;
			ShowBorders();
			ShowBorderVertice(GetCurrentBorderVertice());
			ButtonsSensitivity();
		}

		protected void OnButtonAddVertexClicked(object sender, EventArgs e)
		{
		}

		protected void OnButtonMoveVertexClicked(object sender, EventArgs e)
		{
		}

		protected void OnButtonRemoveVertexClicked(object sender, EventArgs e)
		{
		}

		IList<PointLatLng> GetCurrentBorderVertice()
		{
			if(currentDistrict.DistrictBorder == null){
				return null;
			}

			var coords = currentDistrict.DistrictBorder.Coordinates;
			var vertice = new List<PointLatLng>();

			foreach(Coordinate coord in coords)
			{
				vertice.Add(new PointLatLng(){
					Lat = coord.X,
					Lng = coord.Y
				});
			}

			return vertice;
		}

		void ShowBorders()
		{
			bordersOverlay.Clear();

			foreach(ScheduleRestrictedDistrict district in observableRestrictedDistricts)
			{
				if(district.DistrictBorder != null)
				{
					var border = new GMapPolygon(district.DistrictBorder.Coordinates.Select(p => new PointLatLng(p.X, p.Y)).ToList(), district.DistrictName);
					border.Tag = district;
					bordersOverlay.Polygons.Add(border);
				}
			}
		}

		void ShowBorderVertice(IList<PointLatLng> vertice, bool newBorder = false)
		{
			verticeOverlay.Clear();

			if(vertice == null)
			{
				return;
			}

			foreach(PointLatLng vertex in vertice)
			{
				GMapMarker point = new GMarkerGoogle(vertex, newBorder ? GMarkerGoogleType.red : GMarkerGoogleType.blue);

				verticeOverlay.Markers.Add(point);
			}
		}

		protected void OnGmapWidgetButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 1)
			{
				if(creatingNewBorder)
				{
					var point = gmapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
					newBorderVertice.Add(point);
					ShowBorderVertice(newBorderVertice, true);
				}
			}
		}

		Coordinate[] GetCoordinatesFromPoints()
		{
			IList<Coordinate> coords = new List<Coordinate>();

			foreach(PointLatLng point in currentBorderVertice)
			{
				coords.Add(new Coordinate(){
					X = point.Lat,
					Y = point.Lng						
				});
			}

			return coords.ToArray();
		}

		protected void OnBtnMondayClicked(object sender, EventArgs e)
		{
			currentDistrict.CreateScheduleRestriction(WeekDayName.monday);
			ytreeSchedules.ItemsDataSource = currentDistrict.ScheduleRestrictionMonday.ObservableSchedules;
			ytreeSchedules.QueueDraw();
		}

		protected void OnBtnTuesdayClicked(object sender, EventArgs e)
		{
			currentDistrict.CreateScheduleRestriction(WeekDayName.tuesday);
			ytreeSchedules.ItemsDataSource = currentDistrict.ScheduleRestrictionTuesday.ObservableSchedules;
			ytreeSchedules.QueueDraw();
		}

		protected void OnBtnWednesdayClicked(object sender, EventArgs e)
		{
			currentDistrict.CreateScheduleRestriction(WeekDayName.wednesday);
			ytreeSchedules.ItemsDataSource = currentDistrict.ScheduleRestrictionWednesday.ObservableSchedules;
			ytreeSchedules.QueueDraw();
		}

		protected void OnBtnThursdayClicked(object sender, EventArgs e)
		{
			currentDistrict.CreateScheduleRestriction(WeekDayName.thursday);
			ytreeSchedules.ItemsDataSource = currentDistrict.ScheduleRestrictionThursday.ObservableSchedules;
			ytreeSchedules.QueueDraw();
		}

		protected void OnBtnFridayClicked(object sender, EventArgs e)
		{
			currentDistrict.CreateScheduleRestriction(WeekDayName.friday);
			ytreeSchedules.ItemsDataSource = currentDistrict.ScheduleRestrictionFriday.ObservableSchedules;
			ytreeSchedules.QueueDraw();
		}

		protected void OnBtnSaturdayClicked(object sender, EventArgs e)
		{
			currentDistrict.CreateScheduleRestriction(WeekDayName.saturday);
			ytreeSchedules.ItemsDataSource = currentDistrict.ScheduleRestrictionSaturday.ObservableSchedules;
			ytreeSchedules.QueueDraw();
		}

		protected void OnBtnSundayClicked(object sender, EventArgs e)
		{
			currentDistrict.CreateScheduleRestriction(WeekDayName.sunday);
			ytreeSchedules.ItemsDataSource = currentDistrict.ScheduleRestrictionSunday.ObservableSchedules;
			ytreeSchedules.QueueDraw();
		}
	}



}
