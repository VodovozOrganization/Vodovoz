using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Domain.Complaints;
using Vodovoz.ViewModels;

namespace Vodovoz.Views
{
	[System.ComponentModel.ToolboxItem(true)] 
	public partial class FilesView : Gtk.Bin
	{
		private readonly Menu _menu = new Menu();

		private FilesViewModel _viewModel;
		public FilesViewModel ViewModel {
			get => _viewModel;
			set
			{
				_viewModel = value;
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
			{
				return;
			}

			ybuttonAttachFile.Clicked += (sender, e) => _viewModel.AddItemCommand.Execute();
			ytreeviewFiles.ButtonReleaseEvent += KeystrokeHandler;

			ytreeviewFiles.ColumnsConfig = FluentColumnsConfig<ComplaintFile>.Create()
				.AddColumn("Файлы").AddTextRenderer(x => x.FileStorageId)
				.Finish();

			ytreeviewFiles.Binding.AddFuncBinding(_viewModel, e => !e.ReadOnly, w => w.Sensitive).InitializeFromSource();
			ybuttonAttachFile.Binding.AddFuncBinding(_viewModel, e => !e.ReadOnly, w => w.Sensitive).InitializeFromSource();
			ytreeviewFiles.ItemsDataSource = ViewModel.FilesList;

			ytreeviewFiles.RowActivated += (o, args) => _viewModel.OpenItemCommand.Execute(ytreeviewFiles.GetSelectedObject<ComplaintFile>());

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
