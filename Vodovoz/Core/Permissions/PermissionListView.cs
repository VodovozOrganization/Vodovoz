using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using Vodovoz.Repositories.Permissions;
using Vodovoz.ViewModels;
using QS.Widgets.GtkUI;
using System.ComponentModel;

namespace Vodovoz.Core.Permissions
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PermissionListView : Gtk.Bin
	{
		public PermissionListView()
		{
			this.Build();
		}

		private IList<HBox> hBoxList;

		private PermissionListViewModel viewModel;

		public PermissionListViewModel ViewModel {
			get { return viewModel; }
			set {
				viewModel = value;
				ConfigureDlg();
			}
		}

		private void ConfigureDlg()
		{
			viewModel.PropertyChanged += (object sender, PropertyChangedEventArgs e) => { 
					if(e.PropertyName == nameof(viewModel.PermissionsList)) 
							ConfidureList();
			};

			if(viewModel.PermissionsList != null)
				ConfidureList();
		}

		private void ConfidureList()
		{
			viewModel.PermissionsList.ElementAdded += (object aList, int[] aIdx) => AddRow(aIdx.First());
			viewModel.PermissionsList.ElementRemoved += (aList, aIdx, aObject) => DeleteRow(aObject);
			Redraw();
		}


		private void DrawNewRow(PermissionNode node)
		{
			if(hBoxList?.FirstOrDefault() == null)
				hBoxList = new List<HBox>();

			HBox hBox = new HBox();
			hBox.Spacing = hboxHeader.Spacing;

			var documentLabel = new yLabel();
			documentLabel.Wrap = true;
			documentLabel.LineWrap = true;
			documentLabel.LineWrapMode = Pango.WrapMode.WordChar;
			documentLabel.WidthRequest = ylabelDocument.Allocation.Width;
			documentLabel.Binding.AddBinding(node.EntitySubdivisionOnlyPermission.TypeOfEntity, e => e.CustomName, w => w.Text).InitializeFromSource();
			hBox.Add(documentLabel);
			hBox.SetChildPacking(documentLabel, false, false, 0, PackType.Start);

			var readPermCheckButton = new yCheckButton();
			readPermCheckButton.WidthRequest = ylabelPermView.Allocation.Width;
			readPermCheckButton.Binding.AddBinding(node.EntitySubdivisionOnlyPermission, e => e.CanRead, w => w.Active).InitializeFromSource();
			hBox.Add(readPermCheckButton);
			hBox.SetChildPacking(readPermCheckButton, false, false, 0, PackType.Start);

			var createPermCheckButton = new yCheckButton();
			createPermCheckButton.WidthRequest = ylabelPermCreate.Allocation.Width;
			createPermCheckButton.Binding.AddBinding(node.EntitySubdivisionOnlyPermission, e => e.CanCreate, w => w.Active).InitializeFromSource();
			hBox.Add(createPermCheckButton);
			hBox.SetChildPacking(createPermCheckButton, false, false, 0, PackType.Start);

			var editPermCheckButton = new yCheckButton();
			editPermCheckButton.WidthRequest = ylabelPermEdit.Allocation.Width;
			editPermCheckButton.Binding.AddBinding(node.EntitySubdivisionOnlyPermission, e => e.CanUpdate, w => w.Active).InitializeFromSource();
			hBox.Add(editPermCheckButton);
			hBox.SetChildPacking(editPermCheckButton, false, false, 0, PackType.Start);

			var deletePermCheckButton = new yCheckButton();
			deletePermCheckButton.WidthRequest = ylabelPermDelete.Allocation.Width;
			deletePermCheckButton.Binding.AddBinding(node.EntitySubdivisionOnlyPermission, e => e.CanDelete, w => w.Active).InitializeFromSource();
			hBox.Add(deletePermCheckButton);
			hBox.SetChildPacking(deletePermCheckButton, false, false, 0, PackType.Start);

			foreach(var item in node.EntityPermissionExtended) 
			{
				var nullCheckButton = new NullableCheckButton();
				var permission = ViewModel.PermissionExtensionStore.PermissionExtensions.FirstOrDefault(x => x.PermissionId == item.PermissionId); //TODO Заменить на словарь

				if(permission.IsValidType(node.TypeOfEntity))
					nullCheckButton.Binding.AddBinding(item, e => e.IsPermissionAvailable, w => w.Active).InitializeFromSource();
				else
					nullCheckButton.Sensitive = false;

				hBox.Add(nullCheckButton);
				hBox.SetChildPacking(nullCheckButton, false, false, 0, PackType.Start);
			}
 
			hBox.Data.Add("permission", node); //Для свзяки виджета и права
			hBox.ShowAll();

			vboxPermissions.Add(hBox);
			vboxPermissions.ShowAll();
			hBoxList.Add(hBox);

		}

		public void Redraw()
		{
			hboxHeader.ShowAll();
			foreach(var child in vboxPermissions.Children) {
				vboxPermissions.Remove(child);
			}

			foreach(PermissionNode node in viewModel.PermissionsList) {
				DrawNewRow(node);
			}
		}

		private void AddRow(int index)
		{
			Redraw();
			//DrawNewRow(ViewModel.PermissionsList[index]);
		}

		private void DeleteRow(int rowNumber)
		{
			hBoxList.RemoveAt(rowNumber);
		}

		private void DeleteRow(HBox widget)
		{
			hBoxList.Remove(widget);
		}

		private void DeleteRow(object deleteObj)
		{
				
		}
	}
}
