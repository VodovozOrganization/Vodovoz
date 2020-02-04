using System.Linq;
using QS.Project.Services;
using QS.Services;
using QS.Views.GtkUI;
using Vodovoz.Additions.Store;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Users;

namespace Vodovoz.Views.Users
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UserSettingsView : TabViewBase<UserSettingsViewModel>
	{
		public UserSettingsView(UserSettingsViewModel viewModel) : base(viewModel) {
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			yentryrefWarehouse.ItemsQuery = StoreDocumentHelper.GetWarehouseQuery();
			yentryrefWarehouse.Binding.AddBinding(ViewModel.Entity, e => e.DefaultWarehouse, w => w.Subject).InitializeFromSource();

			yenumcomboDefaultCategory.ItemsEnum = typeof(NomenclatureCategory);
			var itemsToHide = Nomenclature.GetAllCategories().Except(Nomenclature.GetCategoriesForSaleToOrder()).Cast<object>().ToArray();
			yenumcomboDefaultCategory.AddEnumToHideList(itemsToHide);
			yenumcomboDefaultCategory.Binding.AddBinding(ViewModel.Entity, e => e.DefaultSaleCategory, w => w.SelectedItemOrNull).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(false); };
		}
	}
}
