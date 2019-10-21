using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using Vodovoz.Repositories.Permissions;
using Vodovoz.ViewModels;
using System.ComponentModel;
using Vodovoz.ViewWidgets;
using QS.Project.Repositories;

namespace Vodovoz.Core.Permissions
{
	[ToolboxItem(true)]
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

			foreach(var item in ViewModel.PermissionExtensionStore.PermissionExtensions) {
				var extensionLabel = new yLabel();
				extensionLabel.UseMarkup = true;
				extensionLabel.Markup = $"<b>{item.Name}</b>";
				extensionLabel.WidthRequest = 100;
				extensionLabel.Wrap = true;
				extensionLabel.LineWrapMode = Pango.WrapMode.WordChar;
				extensionLabel.Data.Add("permission_id", item.PermissionId);
				extensionLabel.TooltipText = item.Description;
				hboxExtension.Add(extensionLabel);
			}

			if(viewModel.PermissionsList != null)
				ConfidureList();
		}

		private void ConfidureList()
		{
			viewModel.PermissionsList.ElementAdded += (object aList, int[] aIdx) => aIdx.ToList().ForEach(AddRow);
			viewModel.PermissionsList.ElementRemoved += (aList, aIdx, aObject) => DeleteRow(aObject);
			Redraw();
		}


		private void DrawNewRow(PermissionNode node)
		{
			if(hBoxList?.FirstOrDefault() == null)
				hBoxList = new List<HBox>();

			HBox hBox = new HBox();
			hBox.Spacing = hboxHeader.Spacing;

			yButton deleteButton = new yButton();
			Image image = new Image();
			image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-delete", IconSize.Menu);
			deleteButton.Image = image;
			deleteButton.Clicked += (sender, e) => viewModel.DeleteItemCommand.Execute(((yButton)sender).Parent.Data["permission"] as PermissionNode);
			deleteButton.Binding.AddFuncBinding(viewModel, e => !e.ReadOnly, w => w.Sensitive).InitializeFromSource();
			deleteButton.WidthRequest = 40;
			hBox.Add(deleteButton);
			hBox.SetChildPacking(deleteButton, false, false, 0, PackType.Start);

			var documentLabel = new yLabel();
			documentLabel.Wrap = true;
			documentLabel.LineWrap = true;
			documentLabel.LineWrapMode = Pango.WrapMode.WordChar;
			documentLabel.WidthRequest = ylabelDocument.WidthRequest - deleteButton.WidthRequest;
			documentLabel.Binding.AddBinding(node.EntitySubdivisionOnlyPermission.TypeOfEntity, e => e.CustomName, w => w.Text).InitializeFromSource();

			hBox.Add(documentLabel);
			hBox.SetChildPacking(documentLabel, false, false, 0, PackType.Start);

			var readPermCheckButton = new yCheckButton();
			readPermCheckButton.WidthRequest = ylabelPermView.WidthRequest;
			readPermCheckButton.Binding.AddBinding(node.EntitySubdivisionOnlyPermission, e => e.CanRead, w => w.Active).InitializeFromSource();
			hBox.Add(readPermCheckButton);
			hBox.SetChildPacking(readPermCheckButton, false, true, 0, PackType.Start);

			var createPermCheckButton = new yCheckButton();
			createPermCheckButton.WidthRequest = ylabelPermCreate.WidthRequest;
			createPermCheckButton.Binding.AddBinding(node.EntitySubdivisionOnlyPermission, e => e.CanCreate, w => w.Active).InitializeFromSource();
			hBox.Add(createPermCheckButton);
			hBox.SetChildPacking(createPermCheckButton, false, true, 0, PackType.Start);

			var editPermCheckButton = new yCheckButton();
			editPermCheckButton.WidthRequest = ylabelPermEdit.WidthRequest;
			editPermCheckButton.Binding.AddBinding(node.EntitySubdivisionOnlyPermission, e => e.CanUpdate, w => w.Active).InitializeFromSource();
			hBox.Add(editPermCheckButton);
			hBox.SetChildPacking(editPermCheckButton, false, true, 0, PackType.Start);

			var deletePermCheckButton = new yCheckButton();
			deletePermCheckButton.WidthRequest = ylabelPermDelete.WidthRequest;
			deletePermCheckButton.Binding.AddBinding(node.EntitySubdivisionOnlyPermission, e => e.CanDelete, w => w.Active).InitializeFromSource();
			hBox.Add(deletePermCheckButton);
			hBox.SetChildPacking(deletePermCheckButton, false, true, 0, PackType.Start);

			foreach(var header in hboxExtension.Children.OfType<yLabel>()) 
			{
				var item = node.EntityPermissionExtended.FirstOrDefault(x => x.PermissionId == (header.Data["permission_id"] as string));
				var nullCheckButton = new NullableCheckButton();
				nullCheckButton.RenderMode = RenderMode.Icon;
				nullCheckButton.WidthRequest = header.WidthRequest;
				var permission = ViewModel.PermissionExtensionStore.PermissionExtensions.FirstOrDefault(x => x.PermissionId == item.PermissionId); //TODO Заменить на словарь
				if(permission.IsValidType(TypeOfEntityRepository.GetEntityType(node.TypeOfEntity.Type))) {
					nullCheckButton.Binding.AddBinding(item, e => e.IsPermissionAvailable, w => w.Active).InitializeFromSource();
				}
				else {
					nullCheckButton.Sensitive = false;
					nullCheckButton.Visible = false;
				}
				if(item.Id <= 0)
					item.IsPermissionAvailable = null;

				hBox.Add(nullCheckButton);
				hBox.SetChildPacking(nullCheckButton, false, false, 0, PackType.Start);
			}
 
			hBox.Data.Add("permission", node); //Для свзяки виджета и права
			hBox.ShowAll();

			vboxPermissions.Add(hBox);
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
			DrawNewRow(ViewModel.PermissionsList[index]);
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
			hBoxList.Where(x => x.Data["permission"] == deleteObj).ToList().ForEach(x => x.Destroy());
		}
	}
}
