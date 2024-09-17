using Gdk;
using Gtk;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Presentation.ViewModels.AttachedFiles;

namespace Vodovoz.Presentation.Views
{
	[ToolboxItem(true)]
	public partial class SmallFileInformationsView : WidgetViewBase<AttachedFileInformationsViewModel>
	{
		private static readonly Pixbuf _emptyIcon = new Pixbuf(System.Reflection.Assembly.GetEntryAssembly(), "Vodovoz.icons.common.empty16.png");
		private static readonly Pixbuf _fileMissingOnServerIcon = new Pixbuf(System.Reflection.Assembly.GetEntryAssembly(), "Vodovoz.icons.common.fire16.png");

		private readonly Menu _menu = new Menu();
		private MenuItem _deleteFileMenuItem;
		private MenuItem _saveFileMenuItem;

		public SmallFileInformationsView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			if(ViewModel == null)
			{
				return;
			}

			CleanUpBindingsAndSubscribtions();

			ybuttonAddFileInformation.Binding
				.AddSource(ViewModel)
				.AddFuncBinding(_ => ViewModel.AddCommand.CanExecute(), w => w.Sensitive);

			ybuttonAddFileInformation.Clicked += OnAddFileClicked;

			ytreeviewFiles.CreateFluentColumnsConfig<FileInformation>()
				.AddColumn("").AddPixbufRenderer((node) =>
					ViewModel.FilesMissingOnStorage.Contains(node.FileName)
					? _fileMissingOnServerIcon
					: _emptyIcon)
				.AddColumn("Файл").AddTextRenderer(n => n.FileName)
				.AddColumn("")
				.Finish();

			ytreeviewFiles.ItemsDataSource = ViewModel.FileInformations;
			ytreeviewFiles.ButtonReleaseEvent += KeystrokeHandler;
			ytreeviewFiles.RowActivated += OnRowActivated;

			ytreeviewFiles.Binding
				.AddBinding(ViewModel, vm => vm.SelectedFile, w => w.SelectedRow);

			ConfigureMenu();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void OnRowActivated(object o, RowActivatedArgs args)
		{
			ViewModel.OpenCommand.Execute();
		}

		private void OnAddFileClicked(object sender, EventArgs e)
		{
			ViewModel.AddCommand.Execute();
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.CanSave))
			{
				_saveFileMenuItem.Visible = ViewModel.CanSave;
			}

			if(e.PropertyName == nameof(ViewModel.CanDelete))
			{
				_deleteFileMenuItem.Visible = ViewModel.CanDelete;
			}
		}

		private void ConfigureMenu()
		{
			_deleteFileMenuItem = new MenuItem("Удалить файл");
			_deleteFileMenuItem.Activated += (s, args) =>
			{
				ViewModel.DeleteCommand.Execute();
				_menu.Popdown();
			};
			_deleteFileMenuItem.Visible = ViewModel.CanSave;
			_menu.Add(_deleteFileMenuItem);

			_saveFileMenuItem = new MenuItem("Загрузить файл");
			_saveFileMenuItem.Activated += (s, args) =>
			{
				ViewModel.SaveCommand.Execute();
				_menu.Popdown();
			};
			_saveFileMenuItem.Visible = ViewModel.CanDelete;
			_menu.Add(_saveFileMenuItem);

			_menu.ShowAll();
		}

		private void KeystrokeHandler(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3 && !ViewModel.ReadOnly && ViewModel.SelectedFile != null)
			{
				_menu.Popup();
			}
		}

		private void CleanUpBindingsAndSubscribtions()
		{
			ybuttonAddFileInformation.Clicked -= OnAddFileClicked;
			ytreeviewFiles.ButtonReleaseEvent -= KeystrokeHandler;
			ytreeviewFiles.RowActivated -= OnRowActivated;

			ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
			ytreeviewFiles.ButtonReleaseEvent -= KeystrokeHandler;
			
			ybuttonAddFileInformation.Binding.CleanSources();
			ytreeviewFiles.Binding.CleanSources();
		}

		public override void Destroy()
		{
			CleanUpBindingsAndSubscribtions();

			base.Destroy();
		}
	}
}
