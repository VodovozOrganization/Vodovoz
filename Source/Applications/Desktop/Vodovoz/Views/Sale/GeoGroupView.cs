using Gamma.ColumnConfig;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.MapProviders;
using QS.Osrm;
using QS.Views.GtkUI;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModels.Dialogs.Sales;

namespace Vodovoz.Views.Sale
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GeoGroupView : EntityTabViewBase<GeoGroupViewModel, GeoGroup>
	{
		private readonly GMapOverlay _addressOverlay = new GMapOverlay();
		private bool _addressMoving;
		private GMapMarker _addressMarker;

		public GeoGroupView(GeoGroupViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			yEntryName.Binding.AddBinding(Entity, x => x.Name, w => w.Text).InitializeFromSource();

			ycheckbuttonArchived.Binding.AddBinding(Entity, e => e.IsArchived, w => w.Active).InitializeFromSource();

			ytreeviewVersions.ColumnsConfig = FluentColumnsConfig<GeoGroupVersionViewModel>.Create()
				.AddColumn("Код").AddNumericRenderer(x => x.Id)
				.AddColumn("Статус").AddTextRenderer(x => x.StatusTitle)
				.AddColumn("Автор").AddTextRenderer(x => x.Author)
				.AddColumn("Дата создания").AddTextRenderer(x => x.CreationDate)
				.AddColumn("Дата активации").AddTextRenderer(x => x.ActivationDate)
				.AddColumn("Дата закрытия").AddTextRenderer(x => x.ClosingDate)
				.Finish();
			ytreeviewVersions.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Versions, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedVersion, w => w.SelectedRow)
				.InitializeFromSource();
			ytreeviewVersions.Selection.Mode = Gtk.SelectionMode.Single;

			labelCoordinatesValue.WidthRequest = 220;

			ybuttonCreate.Clicked += (s, e) => ViewModel.CreateVersionCommand.Execute();
			ViewModel.CreateVersionCommand.CanExecuteChanged += (s, e) => ybuttonCreate.Sensitive = ViewModel.CreateVersionCommand.CanExecute();
			ViewModel.CreateVersionCommand.RaiseCanExecuteChanged();

			ybuttonCopy.Clicked += (s, e) => ViewModel.CopyVersionCommand.Execute();
			ViewModel.CopyVersionCommand.CanExecuteChanged += (s, e) => ybuttonCopy.Sensitive = ViewModel.CopyVersionCommand.CanExecute();
			ViewModel.CopyVersionCommand.RaiseCanExecuteChanged();

			ybuttonActivate.Clicked += (s, e) => ViewModel.ActivateVersionCommand.Execute();
			ViewModel.ActivateVersionCommand.CanExecuteChanged += (s, e) => ybuttonActivate.Sensitive = ViewModel.ActivateVersionCommand.CanExecute();
			ViewModel.ActivateVersionCommand.RaiseCanExecuteChanged();

			ybuttonClose.Clicked += (s, e) => ViewModel.CloseVersionCommand.Execute();
			ViewModel.CloseVersionCommand.CanExecuteChanged += (s, e) => ybuttonClose.Sensitive = ViewModel.CloseVersionCommand.CanExecute();
			ViewModel.CloseVersionCommand.RaiseCanExecuteChanged();

			ybuttonRemove.Clicked += (s, e) => ViewModel.RemoveVersionCommand.Execute();
			ViewModel.RemoveVersionCommand.CanExecuteChanged += (s, e) => ybuttonRemove.Sensitive = ViewModel.RemoveVersionCommand.CanExecute();
			ViewModel.RemoveVersionCommand.RaiseCanExecuteChanged();

			entryCashSubdivision.ViewModel = ViewModel.CashSubdivisionViewModel;
			entryCashSubdivision.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeCashSubdivision, w => w.ViewModel.IsEditable)
				.InitializeFromSource();
			entityentryWarehouse.ViewModel = ViewModel.WarehouseViewModel;
			entityentryWarehouse.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeWarehouse, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			ViewModel.PropertyChanged += ViewModelPropertyChanged;

			comboMapProvider.TooltipText = "Если карта отображается некорректно или не отображается вовсе - смените тип карты";
			comboMapProvider.ItemsEnum = typeof(MapProviders);
			comboMapProvider.SelectedItem = MapProviders.GoogleMap;
			comboMapProvider.EnumItemSelected += (sender, args) =>
				gMapWidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders)args.SelectedItem);

			gMapWidget.ButtonPressEvent += GMapWidgetButtonPressEvent;
			gMapWidget.ButtonReleaseEvent += GMapWidgetButtonReleaseEvent;
			gMapWidget.MotionNotifyEvent += GMapWidgetMotionNotifyEvent;

			buttonSave.Clicked += (s, e) => ViewModel.SaveCommand.Execute();
			ViewModel.SaveCommand.CanExecuteChanged += (s, e) => buttonSave.Sensitive = ViewModel.SaveCommand.CanExecute();
			ViewModel.SaveCommand.RaiseCanExecuteChanged();

			buttonCancel.Clicked += (s, e) => ViewModel.CancelCommand.Execute();
			ViewModel.CancelCommand.CanExecuteChanged += (s, e) => buttonCancel.Sensitive = ViewModel.CancelCommand.CanExecute();
			ViewModel.CancelCommand.RaiseCanExecuteChanged();

			RefreshVersion();
		}

		private void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(ViewModel.SelectedVersion):
					RefreshVersion();
					SubscribeOnVersionPropertyChanged();
					break;
				default:
					break;
			}
		}

		private void RefreshVersion()
		{
			RebindEntry();
			RefreshMap();
		}

		private void RebindEntry()
		{
			labelCoordinatesValue.Binding.CleanSources();

			if(ViewModel.SelectedVersion is null)
			{
				labelCoordinatesValue.LabelProp = "";
				return;
			}

			labelCoordinatesValue.Binding
				.AddSource(ViewModel.SelectedVersion)
				.AddFuncBinding(x => x.CoordinatesString, w => w.Text).InitializeFromSource();
		}

		private void RefreshMap()
		{
			PointLatLng point;

			if(ViewModel.SelectedVersion == null || !ViewModel.SelectedVersion.Coordinates.HasValue)
			{
				point = new PointLatLng(59.93900, 30.31646);
			}
			else
			{
				var coordinates = ViewModel.SelectedVersion.Coordinates.Value;
				point = new PointLatLng(coordinates.Latitude, coordinates.Longitude);
			}

			UpdateAddressOnMap();

			gMapWidget.MapProvider = GMapProviders.GoogleMap;
			gMapWidget.Position = point;
			gMapWidget.MinZoom = 0;
			gMapWidget.MaxZoom = 24;
			gMapWidget.Zoom = 9;
			gMapWidget.HasFrame = true;
			gMapWidget.Overlays.Add(_addressOverlay);

		}

		private void SubscribeOnVersionPropertyChanged()
		{
			if(ViewModel.SelectedVersion == null)
			{
				return;
			}
			ViewModel.SelectedVersion.PropertyChanged += SelectedVersion_PropertyChanged;
		}

		private void SelectedVersion_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(ViewModel.SelectedVersion != null && ViewModel.SelectedVersion != sender)
			{
				ViewModel.SelectedVersion.PropertyChanged -= SelectedVersion_PropertyChanged;
			}

			switch(e.PropertyName)
			{
				case nameof(ViewModel.SelectedVersion.Coordinates):
					UpdateMapPosition();
					UpdateAddressOnMap();
					break;
				default:
					break;
			}
		}

		void GMapWidgetMotionNotifyEvent(object o, Gtk.MotionNotifyEventArgs args)
		{
			if(!_addressMoving || ViewModel.SelectedVersion == null)
			{
				return;
			}

			_addressMarker.Position = gMapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
		}

		private void GMapWidgetButtonReleaseEvent(object o, Gtk.ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 1)
			{
				_addressMoving = false;

				if(ViewModel.SelectedVersion == null)
				{
					return;
				}
				var newPoint = gMapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
				ViewModel.SelectedVersion.Coordinates = new PointOnEarth(newPoint.Lat, newPoint.Lng);
			}
		}

		private void GMapWidgetButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
		{
			if(args.Event.Button == 1)
			{
				if(ViewModel.SelectedVersion == null)
				{
					return;
				}

				var newPoint = gMapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
				if(_addressMarker == null)
				{
					var coord = new PointLatLng(newPoint.Lat, newPoint.Lng);
					_addressMarker = new PointMarker(coord, PointMarkerType.vodonos, PointMarkerShape.custom);
					_addressOverlay.Markers.Add(_addressMarker);
				}
				else
				{
					_addressMarker.Position = newPoint;
				}

				_addressMoving = true;
			}
		}

		void UpdateMapPosition()
		{
			if(ViewModel.SelectedVersion == null || ViewModel.SelectedVersion.Coordinates == null)
			{
				gMapWidget.Position = new PointLatLng(59.93900, 30.31646);
				gMapWidget.Zoom = 9;
				return;
			}

			var coordinates = ViewModel.SelectedVersion.Coordinates.Value;
			var position = new PointLatLng(coordinates.Latitude, coordinates.Longitude);
			if(!gMapWidget.ViewArea.Contains(position))
			{
				gMapWidget.Position = position;
				gMapWidget.Zoom = 15;
			}
		}

		void UpdateAddressOnMap()
		{
			if(_addressMarker != null)
			{
				_addressOverlay.Markers.Clear();
				_addressMarker = null;
			}

			if(ViewModel.SelectedVersion == null || ViewModel.SelectedVersion.Coordinates == null)
			{
				return;
			}

			var coordinates = ViewModel.SelectedVersion.Coordinates.Value;
			var point = new PointLatLng(coordinates.Latitude, coordinates.Longitude);
			_addressMarker = new PointMarker(point, PointMarkerType.vodonos, PointMarkerShape.custom);
			_addressMarker.ToolTipText = ViewModel.SelectedVersion.CoordinatesString;
			_addressOverlay.Markers.Add(_addressMarker);
		}
	}
}
