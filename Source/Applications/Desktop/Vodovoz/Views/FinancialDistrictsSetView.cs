using System;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gdk;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using Gtk;
using QS.Dialog.GtkUI;
using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.ViewModels;

namespace Vodovoz.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FinancialDistrictsSetView : TabViewBase<FinancialDistrictsSetViewModel>
	{
		private readonly GMapOverlay bordersOverlay = new GMapOverlay("district_borders");
		private readonly GMapOverlay newBordersPreviewOverlay = new GMapOverlay("district_preview_borders");
		private readonly GMapOverlay verticeOverlay = new GMapOverlay("district_vertice");
		
		public FinancialDistrictsSetView(FinancialDistrictsSetViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureView();
		}

		private void ConfigureView()
		{
			#region TreeView

			var colorRed = GdkColors.DangerText;
			var colorPrimaryBase = GdkColors.PrimaryBase;

			ytreeDistricts.ColumnsConfig = FluentColumnsConfig<FinancialDistrict>.Create()
				.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Id.ToString())
				.AddColumn("Название")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Name)
					.AddSetter((c, n) => 
						c.Editable = n.FinancialDistrictsSet.Status != DistrictsSetStatus.Active)
				.AddColumn("Организация")
					.AddComboRenderer(x => x.Organization)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.UoW.GetAll<Domain.Organizations.Organization>().ToList())
					.AddSetter((c, n) => 
						c.Editable = n.FinancialDistrictsSet.Status != DistrictsSetStatus.Active)
				.AddColumn("")
				.Finish();
			
			ytreeDistricts.Binding.AddBinding(ViewModel.Entity, e => e.ObservableFinancialDistricts, w => w.ItemsDataSource).InitializeFromSource();
			ytreeDistricts.Selection.Changed += YTreeDistrictsSelectionOnChanged;
			
			#endregion
			
			btnSave.Clicked += (sender, args) => ViewModel.Save();
			btnSave.Binding.AddFuncBinding(ViewModel, vm => vm.CanEditDistrict || vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			
			btnCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);
			
			ylabelStatusString.Text = ViewModel.Entity.Status.GetEnumTitle();
			
			entryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			entryName.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			
			btnAddDistrict.Binding.AddFuncBinding(ViewModel, vm => vm.CanCreateDistrict, w => w.Sensitive).InitializeFromSource();
			btnAddDistrict.Clicked += BtnAddDistrictOnClicked;

			btnRemoveDistrict.Clicked += BtnRemoveDistrictOnClicked;
			btnRemoveDistrict.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedDistrict != null && vm.CanDeleteDistrict, w => w.Sensitive).InitializeFromSource();
			
			#region GMap

			btnAddBorder.Binding.AddFuncBinding(ViewModel, vm => !vm.IsCreatingNewBorder, w => w.Visible).InitializeFromSource();
			btnAddBorder.Binding.AddFuncBinding(ViewModel, vm => vm.CanEditDistrict && !vm.IsCreatingNewBorder && vm.SelectedDistrict != null && vm.SelectedDistrict.Border == null, w => w.Sensitive).InitializeFromSource();
			btnAddBorder.Clicked += BtnAddBorderOnClicked;

			btnRemoveBorder.Binding.AddFuncBinding(ViewModel, vm => !vm.IsCreatingNewBorder, w => w.Visible).InitializeFromSource();
			btnRemoveBorder.Binding.AddFuncBinding(ViewModel, vm => vm.CanEditDistrict && !vm.IsCreatingNewBorder && vm.SelectedDistrict != null && vm.SelectedDistrict.Border != null, w => w.Sensitive).InitializeFromSource();
			btnRemoveBorder.Clicked += BtnRemoveBorderOnClicked;

			btnConfirmNewBorder.Binding.AddFuncBinding(ViewModel, vm => vm.IsCreatingNewBorder, w => w.Visible).InitializeFromSource();
			btnConfirmNewBorder.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedDistrict != null && vm.IsCreatingNewBorder, w => w.Sensitive).InitializeFromSource();
			btnConfirmNewBorder.Clicked += BtnConfirmNewBorderOnClicked;

			btnCancelNewBorder.Binding.AddFuncBinding(ViewModel, vm => vm.IsCreatingNewBorder, w => w.Visible).InitializeFromSource();
			btnCancelNewBorder.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedDistrict != null && vm.IsCreatingNewBorder, w => w.Sensitive).InitializeFromSource();
			btnCancelNewBorder.Clicked += BtnCancelNewBorderOnClicked;

			toggleNewBorderPreview.Binding.AddFuncBinding(ViewModel, vm => vm.IsCreatingNewBorder, w => w.Visible).InitializeFromSource();
			toggleNewBorderPreview.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedDistrict != null && vm.IsCreatingNewBorder, w => w.Sensitive).InitializeFromSource();
			toggleNewBorderPreview.Toggled += ToggleNewBorderPreviewOnToggled;

			cmbMapType.ItemsEnum = typeof(MapProviders);
			cmbMapType.TooltipText = "Если карта отображается некорректно или не отображается вовсе - смените тип карты";
			cmbMapType.EnumItemSelected += (sender, args) =>
				gmapWidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders)args.SelectedItem);
			cmbMapType.SelectedItem = MapProviders.GoogleMap;

			gmapWidget.Position = new PointLatLng(59.93900, 30.31646);
			gmapWidget.HeightRequest = 150;
			gmapWidget.HasFrame = true;
			gmapWidget.Overlays.Add(bordersOverlay);
			newBordersPreviewOverlay.IsVisibile = false;
			gmapWidget.Overlays.Add(newBordersPreviewOverlay);
			gmapWidget.Overlays.Add(verticeOverlay);
			RefreshBorders();
			gmapWidget.ButtonPressEvent += GmapWidgetOnButtonPressEvent;

			#endregion
		}

		private void BtnAddDistrictOnClicked(object sender, EventArgs e)
		{
			ViewModel.AddDistrictCommand.Execute();
			ScrollToSelectedDistrict();
		}

		private void BtnRemoveDistrictOnClicked(object sender, EventArgs e)
		{
			ViewModel.RemoveDistrictCommand.Execute();
			
			if(ViewModel.SelectedDistrict == null)
				RefreshBorders();
			else 
				ScrollToSelectedDistrict();
		}

		private void BtnAddBorderOnClicked(object sender, EventArgs e)
		{
			ViewModel.CreateBorderCommand.Execute();
			verticeOverlay.Clear();
		}

		private void BtnRemoveBorderOnClicked(object sender, EventArgs e)
		{
			if(MessageDialogHelper.RunQuestionDialog($"Удалить границу района {ViewModel.SelectedDistrict.Name}?")) {
				ViewModel.RemoveBorderCommand.Execute();
				RefreshBorders();
				verticeOverlay.Clear();
			}
		}

		private void BtnConfirmNewBorderOnClicked(object sender, EventArgs e)
		{
			if(MessageDialogHelper.RunQuestionDialog("Завершить создание границы района?")) {
				if(ViewModel.NewBorderVertices.Count < 3) {
					MessageDialogHelper.RunInfoDialog("Нельзя создать границу района меньше чем за 3 точки");
					return;
				}
				
				toggleNewBorderPreview.Active = false;
				newBordersPreviewOverlay.Clear();
				ViewModel.ConfirmNewBorderCommand.Execute();
				RefreshBorders();
			}
		}

		private void BtnCancelNewBorderOnClicked(object sender, EventArgs e)
		{
			if(MessageDialogHelper.RunQuestionDialog("Отменить создание границы района?")) {
				ViewModel.CancelNewBorderCommand.Execute();
				verticeOverlay.Clear();
				newBordersPreviewOverlay.Clear();
				toggleNewBorderPreview.Active = false;
			}
		}

		private void GmapWidgetOnButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			if(args.Event.Button == 1 && ViewModel.IsCreatingNewBorder) {
				var point = gmapWidget.FromLocalToLatLng((int) args.Event.X, (int) args.Event.Y);
				
				ViewModel.AddNewVertexCommand.Execute(point);
				
				RefreshVerticeOverlay();
				RefreshPreviewBorders();
			}

			if (args.Event.Button == 3 && ViewModel.IsCreatingNewBorder)
			{
				var marker = verticeOverlay.Markers.FirstOrDefault(m => m.IsMouseOver);

				if (marker == null)
					return;

				var pointMarker = new PointLatLng(marker.Position.Lat, marker.Position.Lng);

				if (ViewModel.NewBorderVertices.Contains(pointMarker))
				{
					Menu popupMenu = new Menu();
					var item = new MenuItem("Удалить");

					item.Activated += (sender, e) =>
					{
						ViewModel.RemoveNewBorderVertexCommand.Execute(pointMarker);

						RefreshVerticeOverlay();
						RefreshPreviewBorders();
					};

					popupMenu.Add(item);
					popupMenu.ShowAll();
					popupMenu.Popup();
				}
			}
		}

		private void RefreshPreviewBorders()
		{
			newBordersPreviewOverlay.Clear();
			var previewBorder = new GMapPolygon(ViewModel.NewBorderVertices.ToList(), "Предпросмотр новых границ");
			newBordersPreviewOverlay.Polygons.Add(previewBorder);
		}

		private void RefreshVerticeOverlay()
		{
			verticeOverlay.Clear();
			for (int i = 0; i < ViewModel.NewBorderVertices.Count; i++) {
				var color = GMarkerGoogleType.red;

				if (i == 0) {
					color = GMarkerGoogleType.yellow;
				}
				else if (i == ViewModel.NewBorderVertices.Count - 1) {
					color = GMarkerGoogleType.green;
				}

				GMapMarker point = new GMarkerGoogle(ViewModel.NewBorderVertices[i], color);
				verticeOverlay.Markers.Add(point);
			}
		}

		private void RefreshBorders()
		{
			bordersOverlay.Clear();
			foreach (FinancialDistrict district in ViewModel.Entity.ObservableFinancialDistricts) {
				if(district.Border != null) {
					bordersOverlay.Polygons.Add(new GMapPolygon(
						district.Border.Coordinates.Select(p => new PointLatLng(p.X, p.Y)).ToList(), district.Name)
					);
				}
			}
		}

		private void ToggleNewBorderPreviewOnToggled(object sender, EventArgs e)
		{
			newBordersPreviewOverlay.IsVisibile = toggleNewBorderPreview.Active;
		}

		private void YTreeDistrictsSelectionOnChanged(object sender, EventArgs e)
		{
			if(ViewModel.IsCreatingNewBorder) {
				ViewModel.CancelNewBorderCommand.Execute();
				toggleNewBorderPreview.Active = false;
			}
			
			ViewModel.SelectedDistrict = ytreeDistricts.GetSelectedObject() as FinancialDistrict;
			verticeOverlay.Clear();
			ViewModel.FillSelectedDistrictBorderVertices();
			
			if(ViewModel.SelectedDistrictBorderVertices != null) {
				foreach (PointLatLng vertex in ViewModel.SelectedDistrictBorderVertices) {
					GMapMarker point = new GMarkerGoogle(vertex, GMarkerGoogleType.blue);
					verticeOverlay.Markers.Add(point);
				}
			}
		}

		private void ScrollToSelectedDistrict()
		{
			if(ViewModel.SelectedDistrict != null) {
				var iter = ytreeDistricts.YTreeModel.IterFromNode(ViewModel.SelectedDistrict);
				var path = ytreeDistricts.YTreeModel.GetPath(iter);
				ytreeDistricts.ScrollToCell(path, ytreeDistricts.Columns.FirstOrDefault(), false, 0, 0);
				ytreeDistricts.Selection.SelectPath(path);
			}
		}
	}
}
