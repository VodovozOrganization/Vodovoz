using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.ViewModels.Counterparty;

namespace Vodovoz.Views.Client
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class CounterpartyFilesView : WidgetViewBase<CounterpartyFilesViewModel>
    {
        public CounterpartyFilesView()
        {
            this.Build();
        }

        protected override void ConfigureWidget()
        {
            if (ViewModel == null)
                return;

            ybuttonAttachFile.Clicked += (sender, e) => ViewModel.AddItemCommand.Execute();
            ybuttonAttachFile.Binding.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.Sensitive).InitializeFromSource();

            ytreeviewFiles.ColumnsConfig = FluentColumnsConfig<CounterpartyFile>.Create()
                .AddColumn("Файлы").AddTextRenderer(x => x.FileStorageId)
                .Finish();
            ytreeviewFiles.ItemsDataSource = ViewModel.Entity.ObservableFiles;
            ytreeviewFiles.Binding.AddFuncBinding(ViewModel, e => !e.ReadOnly, w => w.Sensitive).InitializeFromSource();
            ytreeviewFiles.ButtonReleaseEvent += KeystrokeHandler;
            ytreeviewFiles.RowActivated += (o, args) => ViewModel.OpenItemCommand.Execute(ytreeviewFiles.GetSelectedObject<CounterpartyFile>());
        }

        protected void ConfigureMenu()
        {
            if (ytreeviewFiles.GetSelectedObject() == null)
                return;

            var menu = new Menu();

            var deleteFile = new MenuItem("Удалить файл");
            deleteFile.Activated += (s, args) => ViewModel.DeleteItemCommand.Execute(ytreeviewFiles.GetSelectedObject() as CounterpartyFile);
            deleteFile.Visible = true;
            menu.Add(deleteFile);

            var saveFile = new MenuItem("Загрузить файл");
            saveFile.Activated += (s, args) => ViewModel.LoadItemCommand.Execute(ytreeviewFiles.GetSelectedObject() as CounterpartyFile);
            saveFile.Visible = true;
            menu.Add(saveFile);

            menu.ShowAll();
            menu.Popup();
        }

        protected void KeystrokeHandler(object o, ButtonReleaseEventArgs args)
        {
            if (args.Event.Button == 3)
                ConfigureMenu();
        }
    }
}
