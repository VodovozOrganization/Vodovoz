using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using GeoAPI.Geometries;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using GMap.NET.MapProviders;
using Gtk;
using NetTopologySuite.Geometries;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Dialogs.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ScheduleRestrictedDistrictsDlg : Gtk.Bin, ITdiTab
	{
		IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();

		readonly GMapOverlay bordersOverlay = new GMapOverlay("district_borders");
		readonly GMapOverlay verticeOverlay = new GMapOverlay("district_vertice");

		IList<PointLatLng> currentBorderVertice, newBorderVertice;
		GenericObservableList<ScheduleRestrictedDistrict> observableRestrictedDistricts;

		ScheduleRestrictedDistrict currentDistrict = new ScheduleRestrictedDistrict();
		ScheduleRestriction currentSchedule = new ScheduleRestriction();

		bool creatingNewBorder = false;

		GeometryFactory gf = new GeometryFactory(new PrecisionModel(), 3857);

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public HandleSwitchIn HandleSwitchIn { get; set; }

		public HandleSwitchOut HandleSwitchOut { get; set; }

		public string TabName { get { return "Районы с графиками доставки"; } set { return; } }

		public ITdiTabParent TabParent { get; set; }

		public bool FailInitialize { get; set; }

		public ScheduleRestrictedDistrictsDlg()
		{
			this.Build();
			Configure();
		}

		void Configure()
		{
			currentDistrict.ObservableScheduleRestrictions.ElementAdded += OnObservableRestrictions_ElementAdded;
			currentDistrict.ObservableScheduleRestrictions.ElementRemoved += OnObservableRestrictions_ElementRemoved;

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

			ytreeSchedules.ColumnsConfig = FluentColumnsConfig<ScheduleRestriction>.Create()
				.AddColumn("День недели").AddEnumRenderer(x => x.WeekDay).Editing()
				.AddColumn("График").AddComboRenderer(x => x.Schedule)
				.SetDisplayFunc(x => x.Name)
				.FillItems(uow.GetAll<DeliverySchedule>().ToList()).Editing()
				.Finish();
			ytreeSchedules.Selection.Changed += OnYTreeSchedules_SelectionChanged;

			ButtonsSensitivity();

			// Пока кнопочки всё равно не работают.
			buttonAddVertex.Sensitive = buttonAddVertex.Visible 
				= buttonMoveVertex.Sensitive = buttonMoveVertex.Visible 
				= buttonRemoveVertex.Sensitive = buttonRemoveVertex.Visible 
				= false;

			gmapWidget.MapProvider = GMapProviders.OpenStreetMap;
			gmapWidget.Position = new PointLatLng(59.93900, 30.31646);
			gmapWidget.HeightRequest = 150;
			gmapWidget.HasFrame = true;
			gmapWidget.Overlays.Add(bordersOverlay);
			gmapWidget.Overlays.Add(verticeOverlay);
			ShowBorders();
		}

		protected void OnYTreeDistricts_SelectionChanged(object sender, EventArgs e)
		{
			ButtonsSensitivity();
			currentDistrict = ytreeDistricts.GetSelectedObject() as ScheduleRestrictedDistrict;

			if(currentDistrict != null && currentDistrict.ObservableScheduleRestrictions != null)
			{
				var schedules = currentDistrict.ObservableScheduleRestrictions;
				ytreeSchedules.SetItemsSource(schedules);
			}

			if(currentDistrict != null && currentDistrict.DistrictBorder != null)
			{
				currentBorderVertice = GetCurrentBorderVertice();
			} 
			else
			{
				currentBorderVertice = new List<PointLatLng>();

			}

			ShowBorderVertice(currentBorderVertice);

		}

		protected void OnYTreeSchedules_SelectionChanged(object sender, EventArgs e)
		{
			currentSchedule = ytreeSchedules.GetSelectedObject() as ScheduleRestriction;
			ButtonsSensitivity();
		}

		protected void OnButtonAddDistrictClicked(object sender, EventArgs e)
		{
			var district = new ScheduleRestrictedDistrict();
			observableRestrictedDistricts.Add(district);		
		}

		protected void OnButtonDeleteDistrictClicked(object sender, EventArgs e)
		{
			currentDistrict.Remove(uow);
			observableRestrictedDistricts.Remove(currentDistrict);
		}

		protected void OnButtonAddScheduleClicked(object sender, EventArgs e)
		{
			currentDistrict.AddSchedule(uow);
		}

		protected void OnButtonDeleteDistrict1Clicked(object sender, EventArgs e)
		{
			currentSchedule.Remove(uow);
			currentDistrict.ObservableScheduleRestrictions.Remove(currentSchedule);
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
			ytreeSchedules.SetItemsSource((ytreeDistricts.GetSelectedObject() as ScheduleRestrictedDistrict).ObservableScheduleRestrictions);
		}

		void OnObservableRestrictions_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			ytreeSchedules.SetItemsSource((ytreeDistricts.GetSelectedObject() as ScheduleRestrictedDistrict).ObservableScheduleRestrictions);
		}

		public bool CompareHashName(string hashName)
		{
			throw new NotImplementedException();
		}

		void ButtonsSensitivity()
		{
			buttonDeleteDistrict.Sensitive = buttonAddSchedule.Sensitive = buttonCreateBorder.Sensitive = ytreeDistricts.Selection.CountSelectedRows() == 1;
			buttonDeleteSchedule.Sensitive = ytreeSchedules.Selection.CountSelectedRows() == 1;
			buttonRemoveBorder.Sensitive = ytreeDistricts.Selection.CountSelectedRows() == 1 && currentDistrict.DistrictBorder != null;
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
	}
}
