using System;
using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Domain.Complaints;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.Views.Complaints
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ComplaintFilesView : WidgetViewBase<ComplaintFilesViewModel>
	{
		private readonly Menu _menu = new Menu();

		public ComplaintFilesView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			if(ViewModel == null)
				return;

			ybuttonAttachFile.Clicked += (sender, e) => ViewModel.AddItemCommand.Execute();
			ybuttonAttachFile.Binding.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.Sensitive).InitializeFromSource();

			ytreeviewFiles.ColumnsConfig = FluentColumnsConfig<ComplaintFile>.Create()
				.AddColumn("Файлы").AddTextRenderer(x => x.FileStorageId)
				.Finish();
			ytreeviewFiles.ItemsDataSource = ViewModel.Entity.ObservableFiles;
			ytreeviewFiles.ButtonReleaseEvent += KeystrokeHandler;
			ytreeviewFiles.RowActivated += (o, args) => ViewModel.OpenItemCommand.Execute(ytreeviewFiles.GetSelectedObject<ComplaintFile>());

			ConfigureMenu();
		}

		private void ConfigureMenu()
		{
			var deleteFile = new MenuItem("Удалить файл");
			deleteFile.Activated += (s, args) =>
			{
				ViewModel.DeleteItemCommand.Execute(ytreeviewFiles.GetSelectedObject() as ComplaintFile);
				_menu.Popdown();
			};
			deleteFile.Visible = true;
			_menu.Add(deleteFile);

			var saveFile = new MenuItem("Загрузить файл");
			saveFile.Activated += (s, args) =>
			{
				ViewModel.LoadItemCommand.Execute(ytreeviewFiles.GetSelectedObject() as ComplaintFile);
				_menu.Popdown();
			};
			saveFile.Visible = true;
			_menu.Add(saveFile);

			_menu.ShowAll();
		}

		private void KeystrokeHandler(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3 && !ViewModel.ReadOnly && ytreeviewFiles.GetSelectedObject() != null)
			{
				_menu.Popup();
			}
		}
	}
}
