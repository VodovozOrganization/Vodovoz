using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Dialogs.Counterparties;

namespace Vodovoz.Views.Client
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CounterpartyFilesView : WidgetViewBase<CounterpartyFilesViewModel>
	{
		private readonly Menu _menu = new Menu();

		public CounterpartyFilesView()
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

			ytreeviewFiles.ColumnsConfig = FluentColumnsConfig<CounterpartyFile>.Create()
				.AddColumn("Файлы").AddTextRenderer(x => x.FileStorageId)
				.Finish();
			ytreeviewFiles.ItemsDataSource = ViewModel.Entity.ObservableFiles;
			ytreeviewFiles.Binding.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.Sensitive).InitializeFromSource();
			ytreeviewFiles.ButtonReleaseEvent += KeystrokeHandler;
			ytreeviewFiles.RowActivated +=
				(o, args) => ViewModel.OpenItemCommand.Execute(ytreeviewFiles.GetSelectedObject<CounterpartyFile>());

			ConfigureMenu();
		}

		private void ConfigureMenu()
		{
			var deleteFile = new MenuItem("Удалить файл");
			deleteFile.Activated += (s, args) =>
			{
				ViewModel.DeleteItemCommand.Execute(ytreeviewFiles.GetSelectedObject() as CounterpartyFile);
				_menu.Popdown();
			};
			deleteFile.Visible = true;
			_menu.Add(deleteFile);

			var saveFile = new MenuItem("Загрузить файл");
			saveFile.Activated += (s, args) =>
			{
				ViewModel.LoadItemCommand.Execute(ytreeviewFiles.GetSelectedObject() as CounterpartyFile);
				_menu.Popdown();
			};
			saveFile.Visible = true;
			_menu.Add(saveFile);

			_menu.ShowAll();
		}

		private void KeystrokeHandler(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3
			   && !ViewModel.ReadOnly
			   && ytreeviewFiles.GetSelectedObject() != null)
			{
				_menu.Popup();
			}
		}
	}
}
