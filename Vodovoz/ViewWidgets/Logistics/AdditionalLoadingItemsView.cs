using System;
using System.ComponentModel;
using System.Linq;
using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz.ViewWidgets.Logistics
{
	[ToolboxItem(true)]
	public partial class AdditionalLoadingItemsView : WidgetViewBase<AdditionalLoadingItemsViewModel>
	{
		public AdditionalLoadingItemsView()
		{
			Build();
		}

		public override AdditionalLoadingItemsViewModel ViewModel
		{
			get => base.ViewModel;
			set
			{
				if(base.ViewModel == value)
				{
					return;
				}
				base.ViewModel = value;
				OnViewModelChanged();
			}
		}

		private void OnViewModelChanged()
		{
			if(ViewModel == null)
			{
				return;
			}
			ViewModel.PropertyChanged += (sender, args) =>
			{
				if(args.PropertyName == nameof(ViewModel.AdditionalLoadingDocument))
				{
					UpdateTreeView();
				}
			};
			Configure();
		}

		private void Configure()
		{
			ytreeNomenclatures.Selection.Mode = SelectionMode.Multiple;
			ytreeNomenclatures.ColumnsConfig = FluentColumnsConfig<AdditionalLoadingDocumentItem>.Create()
				.AddColumn("Кол-во")
					.MinWidth(75)
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Amount)
					.Adjustment(new Adjustment(1, 1, 1000000, 1, 100, 0))
					.AddSetter((c, node) => c.Editable = ViewModel.CanEdit)
					.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
				.AddColumn("ТМЦ")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Nomenclature.Name)
				.Finish();

			ytreeNomenclatures.Selection.Changed -= OnSelectionChanged;
			ytreeNomenclatures.Selection.Changed += OnSelectionChanged;

			ybuttonRemove.Binding.CleanSources();
			ybuttonRemove.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			ybuttonRemove.Clicked -= OnButtonRemoveClicked;
			ybuttonRemove.Clicked += OnButtonRemoveClicked;

			ybuttonAdd.Binding.CleanSources();
			ybuttonAdd.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			ybuttonAdd.Clicked -= OnButtonAddClicked;
			ybuttonAdd.Clicked += OnButtonAddClicked;

			UpdateTreeView();
		}

		private void OnButtonAddClicked(object sender, EventArgs args)
		{
			ViewModel.AddItem();
		}

		private void OnButtonRemoveClicked(object sender, EventArgs args)
		{
			ViewModel.RemoveItems(ytreeNomenclatures.GetSelectedObjects<AdditionalLoadingDocumentItem>());
		}

		private void OnSelectionChanged(object sender, EventArgs args)
		{
			ybuttonRemove.Sensitive = ytreeNomenclatures.GetSelectedObjects<AdditionalLoadingDocumentItem>().Any() && ViewModel.CanEdit;
		}

		private void UpdateTreeView()
		{
			ytreeNomenclatures.ItemsDataSource = ViewModel?.AdditionalLoadingDocument?.ObservableItems;
		}
	}
}
