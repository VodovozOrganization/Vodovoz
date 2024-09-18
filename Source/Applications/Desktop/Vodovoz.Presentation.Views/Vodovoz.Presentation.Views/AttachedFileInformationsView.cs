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
	public partial class AttachedFileInformationsView : WidgetViewBase<AttachedFileInformationsViewModel>
	{
		private static readonly Pixbuf _emptyImg = new Pixbuf(System.Reflection.Assembly.GetEntryAssembly(), "Vodovoz.icons.common.empty16.png");
		private static readonly Pixbuf _fire = new Pixbuf(System.Reflection.Assembly.GetEntryAssembly(), "Vodovoz.icons.common.fire16.png");

		public AttachedFileInformationsView() : base()
		{
			Build();
		}

		public AttachedFileInformationsView(AttachedFileInformationsViewModel viewModel)
			: base(viewModel)
		{
			Build();
			ConfigureWidget();
		}

		public void InitializeViewModel(AttachedFileInformationsViewModel viewModel)
		{
			if(ViewModel is null)
			{
				ViewModel = viewModel;
			}
			else
			{
				throw new InvalidOperationException("ViewModel уже инициализирована");
			}
		}

		protected override void ConfigureWidget()
		{
			buttonScan.Visible = false;
			buttonAdd.Clicked -= OnAddClicked;
			buttonScan.Clicked -= OnScanClicked;
			btnOpen.Clicked -= OnOpenClicked;
			btnSave.Clicked -= OnSaveClicked;
			btnDelete.Clicked -= OnDeleteClicked;

			base.ConfigureWidget();

			buttonAdd.Clicked += OnAddClicked;
			buttonScan.Clicked += OnScanClicked;
			btnOpen.Clicked += OnOpenClicked;
			btnSave.Clicked += OnSaveClicked;
			btnDelete.Clicked += OnDeleteClicked;

			btnOpen.Binding.CleanSources();
			btnOpen.Binding
				.AddBinding(ViewModel, vm => vm.CanOpen, w => w.Sensitive)
				.InitializeFromSource();

			btnSave.Binding.CleanSources();
			btnSave.Binding
				.AddBinding(ViewModel, vm => vm.CanSave, w => w.Sensitive)
				.InitializeFromSource();

			btnDelete.Binding.CleanSources();
			btnDelete.Binding
				.AddBinding(ViewModel, vm => vm.CanDelete, w => w.Sensitive)
				.InitializeFromSource();

			ConfigureTreeFiles();

			ViewModel.OnFileInformationChanged += OnViewModelFileInformationChanged;
		}

		private void OnDeleteClicked(object sender, EventArgs e)
		{
			ViewModel.DeleteCommand.Execute();
		}

		private void OnSaveClicked(object sender, EventArgs e)
		{
			ViewModel.SaveCommand.Execute();
		}

		private void OnOpenClicked(object sender, EventArgs e)
		{
			ViewModel.OpenCommand.Execute();
		}

		private void OnScanClicked(object sender, EventArgs e)
		{
			ViewModel.ScanCommand.Execute();
		}

		private void OnAddClicked(object sender, EventArgs e)
		{
			ViewModel.AddCommand.Execute();
		}

		private void OnViewModelFileInformationChanged(object sender, EventArgs e)
		{
			treeFiles.QueueDraw();
		}

		private void ConfigureTreeFiles()
		{
			treeFiles.CreateFluentColumnsConfig<FileInformation>()
				.AddColumn("").AddPixbufRenderer((node) =>
					ViewModel.FilesMissingOnStorage.Contains(node.FileName)
					? _fire
					: _emptyImg)
				.AddColumn("Файл").AddTextRenderer(n => n.FileName)
				.AddColumn("")
				.Finish();

			treeFiles.Binding.CleanSources();
			treeFiles.Binding
				.AddBinding(ViewModel, vm => vm.SelectedFile, w => w.SelectedRow)
				.InitializeFromSource();

			treeFiles.ItemsDataSource = ViewModel.FileInformations;
			treeFiles.RowActivated -= OnRowActivated;
			treeFiles.RowActivated += OnRowActivated;
		}

		private void OnRowActivated(object o, RowActivatedArgs args)
		{
			ViewModel.OpenCommand.Execute();
		}

		public override void Destroy()
		{
			treeFiles.RowActivated -= OnRowActivated;

			buttonAdd.Clicked -= OnAddClicked;
			buttonScan.Clicked -= OnScanClicked;
			btnOpen.Clicked -= OnOpenClicked;
			btnSave.Clicked -= OnSaveClicked;
			btnDelete.Clicked -= OnDeleteClicked;

			ViewModel.OnFileInformationChanged -= OnViewModelFileInformationChanged;
			base.Destroy();
		}
	}
}
