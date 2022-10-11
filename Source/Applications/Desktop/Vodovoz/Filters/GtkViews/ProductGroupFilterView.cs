using System;
using QS.DomainModel.UoW;
using QS.Views.GtkUI;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProductGroupFilterView : FilterViewBase<ProductGroupFilterViewModel> 
	{
		public ProductGroupFilterView(ProductGroupFilterViewModel productGroupFilterViewModel) : base(productGroupFilterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ycheckArchive.Binding.AddBinding(ViewModel, x => x.HideArchive, w => w.Active).InitializeFromSource();
		}
	}
}
