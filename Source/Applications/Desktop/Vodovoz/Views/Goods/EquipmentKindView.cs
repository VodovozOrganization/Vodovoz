using System;
using Gamma.Utilities;
using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.Views.Goods
{
	public partial class EquipmentKindView : TabViewBase<EquipmentKindViewModel>
	{
		public EquipmentKindView(EquipmentKindViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}
		private void Configure()
		{
			yentryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, widget => widget.Text)
				.InitializeFromSource();

			enumWarrantyType.ItemsEnum = typeof(WarrantyCardType);
			enumWarrantyType.Binding
				.AddBinding(ViewModel.Entity, e => e.WarrantyCardType, widget => widget.SelectedItem)
				.InitializeFromSource();

			enumEquipmentType.ItemsEnum = typeof(EquipmentType);
			enumEquipmentType.Binding
				.AddBinding(ViewModel.Entity, e => e.EquipmentType, w => w.SelectedItem)
				.InitializeFromSource();

			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);
		}
	}
}
