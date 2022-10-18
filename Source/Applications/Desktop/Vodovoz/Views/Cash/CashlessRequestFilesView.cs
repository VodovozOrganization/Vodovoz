using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.Views.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashlessRequestFilesView : WidgetViewBase<CashlessRequestFilesViewModel>
	{
		private readonly Menu _menu = new Menu();

		public CashlessRequestFilesView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			if(ViewModel == null)
			{
				return;
			}

			ybuttonAttachFile.Clicked += (sender, e) => ViewModel.AddItemCommand.Execute();
			ybuttonAttachFile.Binding.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.Sensitive).InitializeFromSource();

			ytreeviewFiles.ColumnsConfig = FluentColumnsConfig<CashlessRequestFile>.Create()
				.AddColumn("Файлы").AddTextRenderer(x => x.FileStorageId)
				.Finish();
			ytreeviewFiles.ItemsDataSource = ViewModel.Entity.ObservableFiles;
			ytreeviewFiles.ButtonReleaseEvent += KeystrokeHandler;
			ytreeviewFiles.RowActivated += (o, args) =>
				ViewModel.OpenItemCommand.Execute(ytreeviewFiles.GetSelectedObject<CashlessRequestFile>());

			ConfigureMenu();
		}

		private void ConfigureMenu()
		{
			var deleteFile = new MenuItem("Удалить файл");
			deleteFile.Activated += (s, args) =>
			{
				ViewModel.DeleteItemCommand.Execute(ytreeviewFiles.GetSelectedObject() as CashlessRequestFile);
				_menu.Popdown();
			};
			deleteFile.Visible = true;
			_menu.Add(deleteFile);

			var saveFile = new MenuItem("Загрузить файл");
			saveFile.Activated += (s, args) =>
			{
				ViewModel.LoadItemCommand.Execute(ytreeviewFiles.GetSelectedObject() as CashlessRequestFile);
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
