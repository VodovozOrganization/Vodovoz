using System;
using System.Data.Bindings.Utilities;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.MapProviders;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QSProjectsLib;
using QS.Validation;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.Dialogs.Sale
{
	public partial class GeographicGroupDlg : EntityDialogBase<GeoGroup>
	{
		readonly GMapOverlay addressOverlay = new GMapOverlay();
		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;
		GMapMarker addressMarker;
		bool addressMoving;

		public GeographicGroupDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<GeoGroup>();
			ConfigureDlg();
		}

		public GeographicGroupDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<GeoGroup>(id);
			ConfigureDlg();
		}

		public GeographicGroupDlg(GeoGroup sub) : this(sub.Id) { }

		void ConfigureDlg()
		{
			yEntryName.Binding.AddBinding(Entity, x => x.Name, w => w.Text).InitializeFromSource();
			lblCoordinatesValue.Binding.AddBinding(
				Entity,
				x => x.CoordinatesText,
				w => w.Text
			).InitializeFromSource();

			gMapWidget.MapProvider = GMapProviders.GoogleMap;
			gMapWidget.Position = Entity.BaseCoordinatesExist ? new PointLatLng((double)Entity.BaseLatitude.Value, (double)Entity.BaseLongitude.Value) : new PointLatLng(59.93900, 30.31646);
			gMapWidget.MinZoom = 0;
			gMapWidget.MaxZoom = 24;
			gMapWidget.Zoom = 9;
			gMapWidget.HasFrame = true;

			gMapWidget.Overlays.Add(addressOverlay);
			if(Entity.Id == 0 || QSMain.User.Admin) {
				gMapWidget.ButtonPressEvent += GMapWidget_ButtonPressEvent;
				gMapWidget.ButtonReleaseEvent += GMapWidget_ButtonReleaseEvent;
				gMapWidget.MotionNotifyEvent += GMapWidget_MotionNotifyEvent;
			}

			comboMapProvider.TooltipText = "Если карта отображается некорректно или не отображается вовсе - смените тип карты";
			comboMapProvider.ItemsEnum = typeof(MapProviders);
			comboMapProvider.SelectedItem = MapProviders.GoogleMap;
			comboMapProvider.EnumItemSelected += (sender, args) =>
				gMapWidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders)args.SelectedItem);

			Entity.PropertyChanged += Entity_PropertyChanged;
			UpdateAddressOnMap();
		}

		void GMapWidget_MotionNotifyEvent(object o, Gtk.MotionNotifyEventArgs args)
		{
			if(addressMoving)
				addressMarker.Position = gMapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
		}

		void GMapWidget_ButtonReleaseEvent(object o, Gtk.ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 1) {
				addressMoving = false;
				var newPoint = gMapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
				UpdateAddressOnMap();
				Entity.SetСoordinates((decimal)newPoint.Lat, (decimal)newPoint.Lng);
			}
		}

		void GMapWidget_ButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
		{
			if(args.Event.Button == 1) {
				var newPoint = gMapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
				if(addressMarker == null) {
					addressMarker = new PointMarker(
						new PointLatLng(
							newPoint.Lat,
							newPoint.Lng
						),
						PointMarkerType.vodonos,
						PointMarkerShape.custom
					) {
						ToolTipText = Entity.CoordinatesText
					};
					addressOverlay.Markers.Add(addressMarker);
				} else
					addressMarker.Position = newPoint;
				addressMoving = true;
			}
		}

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == Entity.GetPropertyName(x => x.BaseLatitude) || e.PropertyName == Entity.GetPropertyName(x => x.BaseLongitude)) {
				UpdateMapPosition();
				UpdateAddressOnMap();
			}
			CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Entity));
		}

		void UpdateMapPosition()
		{
			if(Entity.BaseCoordinatesExist) {
				var position = new PointLatLng((double)Entity.BaseLatitude.Value, (double)Entity.BaseLongitude.Value);
				if(!gMapWidget.ViewArea.Contains(position)) {
					gMapWidget.Position = position;
					gMapWidget.Zoom = 15;
				}
			} else {
				gMapWidget.Position = new PointLatLng(59.93900, 30.31646);
				gMapWidget.Zoom = 9;
			}
		}

		void UpdateAddressOnMap()
		{
			if(addressMarker != null) {
				addressOverlay.Markers.Clear();
				addressMarker = null;
			}

			if(Entity.BaseCoordinatesExist) {
				addressMarker = new PointMarker(
					new PointLatLng(
						(double)Entity.BaseLatitude.Value,
						(double)Entity.BaseLongitude.Value
					),
					PointMarkerType.vodonos,
					PointMarkerShape.custom
				) {
					ToolTipText = Entity.CoordinatesText
				};

				addressOverlay.Markers.Add(addressMarker);
			}
		}

		public override bool Save()
		{
			var valid = new QSValidator<GeoGroup>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;
			UoWGeneric.Save();
			return true;
		}
	}
}
