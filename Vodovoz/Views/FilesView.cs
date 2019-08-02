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

			ybuttonAttachFile.Clicked += (sender, e) => SelectFile();
			ytreeviewFiles.ButtonReleaseEvent += KeystrokeHandler;

			ytreeviewFiles.ColumnsConfig = FluentColumnsConfig<ComplaintFile>.Create()
				.AddColumn("Файлы").AddTextRenderer(x => x.FileStorageId)
				.Finish();

			ytreeviewFiles.Binding.AddFuncBinding(viewModel, e => !e.ReadOnly, w => w.Sensitive).InitializeFromSource();
			ybuttonAttachFile.Binding.AddFuncBinding(viewModel, e => !e.ReadOnly, w => w.Sensitive).InitializeFromSource();

			ytreeviewFiles.Binding.AddBinding(viewModel, e => e.FilesList, w => w.ItemsDataSource).InitializeFromSource();

			ytreeviewFiles.RowActivated += (o, args) => viewModel.OpenItemCommand.Execute(ytreeviewFiles.GetSelectedObject<ComplaintFile>());

			DestroyEvent += (o, args) => ViewModel.DeleteTempFiles();
		}

		protected void ConfigureMenu()
		{
			var menu = new Menu();

			var deleteFile = new MenuItem("Удалить файл");
			deleteFile.Activated += (s, args) => viewModel.DeleteItemCommand.Execute(ytreeviewFiles.GetSelectedObject() as ComplaintFile);
			menu.Append(deleteFile);
			deleteFile.Show();

			deleteFile.Show();
		}

		protected void KeystrokeHandler(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3)
				ConfigureMenu();
		}

		protected void SelectFile() //TODO Убрать логику в ViewModel (создать FileService)
		{
			FileChooserDialog Chooser = new FileChooserDialog(
				"Выберите файл для загрузки...",
				(Window)this.Toplevel,
				FileChooserAction.Open,
				"Отмена", ResponseType.Cancel,
				"Загрузить", ResponseType.Accept
			);

			Chooser.SelectMultiple = true;

			if((ResponseType)Chooser.Run() == ResponseType.Accept) 
			{
				Chooser.Hide();
				foreach(var filePath in Chooser.Filenames) 
				{
					viewModel.AddItemCommand.Execute(filePath);
				}
			}
			Chooser.Destroy();
		}

	}

}
