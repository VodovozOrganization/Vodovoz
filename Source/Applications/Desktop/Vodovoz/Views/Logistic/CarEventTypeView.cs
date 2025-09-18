﻿using QS.Navigation;
using QS.Views.GtkUI;
using ReactiveUI;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	public partial class CarEventTypeView : TabViewBase<CarEventTypeViewModel>
	{
		public CarEventTypeView(CarEventTypeViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			yentryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();

			yentryShortName.Binding
				.AddBinding(ViewModel.Entity, e => e.ShortName, w => w.Text)
				.InitializeFromSource();

			ycheckbuttonNeedComment.Binding
				.AddBinding(ViewModel.Entity, e => e.NeedComment, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonIsArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonDoNotShowInOperation.Binding
				.AddBinding(ViewModel.Entity, e => e.IsDoNotShowInOperation, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonIsAttachWriteOffDocument.Binding
				.AddBinding(ViewModel.Entity, e => e.IsAttachWriteOffDocument, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonAreaOfResponsibility.Binding
				.AddBinding(ViewModel, vm => vm.IsVisibleAreaOfResponsibility, w => w.Active)
				.InitializeFromSource();

			ylabelDepartment.Binding
				.AddBinding(ViewModel, vm => vm.IsVisibleAreaOfResponsibility, w => w.Sensitive)
				.InitializeFromSource();

			yenumcomboboxAreaOfResponsibility.ItemsEnum = typeof(AreaOfResponsibility);
			yenumcomboboxAreaOfResponsibility.Binding
				.AddBinding(ViewModel, vm => vm.IsVisibleAreaOfResponsibility, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.AreaOfResponsibility, w => w.SelectedItemOrNull)
				.InitializeFromSource();


			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);
		}
	}
}
