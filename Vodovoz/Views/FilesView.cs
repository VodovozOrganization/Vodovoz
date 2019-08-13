using System;
using System.Collections.Generic;
using System.IO;
using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Domain.Complaints;
using Vodovoz.ViewModels;

namespace Vodovoz.Views
{
	[System.ComponentModel.ToolboxItem(true)] 
	public partial class FilesView : Gtk.Bin
	{

		private FilesViewModel viewModel;
		public FilesViewModel ViewModel {
			get { return viewModel; }
			set {
				viewModel = value;
				ConfigureDlg();
			}
		}

		public FilesView()
		{
			this.Build();
		}

		public void ConfigureDlg()
		{
			if(ViewModel == null)
				return;

			ybuttonAttachFile.Clicked += (sender, e) => viewModel.AddItemCommand.Execute();
			ytreeviewFiles.ButtonReleaseEvent += KeystrokeHandler;

			ytreeviewFiles.ColumnsConfig = FluentColumnsConfig<ComplaintFile>.Create()
				.AddColumn("Файлы").AddTextRenderer(x => x.FileStorageId)
				.Finish();

			ytreeviewFiles.Binding.AddFuncBinding(viewModel, e => !e.ReadOnly, w => w.Sensitive).InitializeFromSource();
			ybuttonAttachFile.Binding.AddFuncBinding(viewModel, e => !e.ReadOnly, w => w.Sensitive).InitializeFromSource();
			ytreeviewFiles.ItemsDataSource = ViewModel.FilesList;

			ytreeviewFiles.RowActivated += (o, args) => viewModel.OpenItemCommand.Execute(ytreeviewFiles.GetSelectedObject<ComplaintFile>());
		}

		protected void ConfigureMenu()
		{
			if(ytreeviewFiles.GetSelectedObject() == null)
				return;

			var menu = new Menu();

			var deleteFile = new MenuItem("Удалить файл");
			deleteFile.Activated += (s, args) => viewModel.DeleteItemCommand.Execute(ytreeviewFiles.GetSelectedObject() as ComplaintFile);
			deleteFile.Visible = true;
			menu.Add(deleteFile);

			var saveFile = new MenuItem("Загрузить файл");
			saveFile.Activated += (s, args) => viewModel.LoadItemCommand.Execute(ytreeviewFiles.GetSelectedObject() as ComplaintFile);
			saveFile.Visible = true;
			menu.Add(saveFile);

			menu.ShowAll();
			menu.Popup();
		}

		protected void KeystrokeHandler(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3)
				ConfigureMenu();
		}

	}

}
