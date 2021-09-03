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
		}

		private void ConfigureMenu()
		{
			if(ViewModel.ReadOnly || ytreeviewFiles.GetSelectedObject() == null)
			{
				return;
			}

			var menu = new Menu();

			var deleteFile = new MenuItem("Удалить файл");
			deleteFile.Activated += (s, args) =>
				ViewModel.DeleteItemCommand.Execute(ytreeviewFiles.GetSelectedObject() as CashlessRequestFile);
			deleteFile.Visible = true;
			menu.Add(deleteFile);

			var saveFile = new MenuItem("Загрузить файл");
			saveFile.Activated += (s, args) =>
				ViewModel.LoadItemCommand.Execute(ytreeviewFiles.GetSelectedObject() as CashlessRequestFile);
			saveFile.Visible = true;
			menu.Add(saveFile);

			menu.ShowAll();
			menu.Popup();
		}

		private void KeystrokeHandler(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3)
			{
				ConfigureMenu();
			}
		}
	}
}
