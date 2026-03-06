using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.ViewModels.Goods.ProductGroups;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class ProductGroupViewModel : EntityTabViewModelBase<ProductGroup>
	{
		private readonly ViewModelEEVMBuilder<ProductGroup> _productGroupEEVMBuilder;

		public ProductGroupViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ViewModelEEVMBuilder<ProductGroup> productGroupEEVMBuilder)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_productGroupEEVMBuilder = productGroupEEVMBuilder ?? throw new ArgumentNullException(nameof(productGroupEEVMBuilder));

			ProductGroupEntityEntryViewModel = CreateProductGroupEEVM();

			SetArchiveCommand = new DelegateCommand(SetArchive);
			SetIsHighlightInCarLoadDocumentCommand = new DelegateCommand(SetIsHighlightInCarLoadDocument);
			SetIsNeedAdditionalControlCommand = new DelegateCommand(SetIsNeedAdditionalControl, () => CanEditAdditionalControlSettingsInProductGroup);
		}

		public DelegateCommand SetArchiveCommand { get; }
		public DelegateCommand SetIsHighlightInCarLoadDocumentCommand { get; }
		public DelegateCommand SetIsNeedAdditionalControlCommand { get; }

		public IEntityEntryViewModel ProductGroupEntityEntryViewModel { get; }

		private bool IsNewEntity =>
			Entity.Id == 0;

		public bool CanEdit =>
			(IsNewEntity && CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(ProductGroup)).CanCreate)
			|| (!IsNewEntity && CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(ProductGroup)).CanUpdate);

		public bool CanEditOnlineStoreParametersInProductGroup =>
			CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.ProductGroupPermissions.CanEditOnlineStoreParametersInProductGroups);

		public bool CanEditAdditionalControlSettingsInProductGroup =>
			CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.ProductGroupPermissions.CanEditAdditionalControlSettingsInProductGroups);

		private void SetArchive()
		{
			if(!Entity.IsArchive)
			{
				var parent = Entity.Parent;
				if(parent != null && parent.IsArchive)
				{
					Entity.IsArchive = true;
					ShowWarningMessage(
						$"Родительская группа {parent.Name} архивирована.\n" +
						"Выполните одно из действий:\n" +
						"Либо уберите родительскую группу у данной группы\n" +
						"Либо разархивируйте родителя\n" +
						"Либо перенесите эту группу в действующую неархивную группу товаров");
				}
			}
			else
			{
				Entity.FetchChilds(UoW);
				Entity.SetIsArchiveRecursively(Entity.IsArchive);
			}
		}

		private void SetIsHighlightInCarLoadDocument()
		{
			var infoMessage = $"Атрибут \"Выделять в талонах погрузки\" будет " +
				$"{(Entity.IsHighlightInCarLoadDocument ? "проставлен" : "снят")} " +
				$"также для всех дочерних групп";

			ShowWarningMessage(infoMessage);

			Entity.FetchChilds(UoW);
			Entity.SetIsHighlightInCarLoadDocumenToAllChildGroups(Entity.IsHighlightInCarLoadDocument);
		}

		private void SetIsNeedAdditionalControl()
		{
			var infoMessage = $"Атрибут \"Требует доп. контроля водителя\" будет " +
				$"{(Entity.IsNeedAdditionalControl ? "проставлен" : "снят")} " +
				$"также для всех дочерних групп";

			ShowWarningMessage(infoMessage);

			Entity.FetchChilds(UoW);
			Entity.SetIsNeedAdditionalControlToAllChildGroups(Entity.IsNeedAdditionalControl);
		}

		private IEntityEntryViewModel CreateProductGroupEEVM()
		{
			var viewModel =
				_productGroupEEVMBuilder
				.SetViewModel(this)
				.SetUnitOfWork(UoW)
				.ForProperty(Entity, x => x.Parent)
				.UseViewModelJournalAndAutocompleter<ProductGroupsJournalViewModel, ProductGroupsJournalFilterViewModel>(
					filter =>
					{
						filter.IsGroupSelectionMode = true;
					})
				.UseViewModelDialog<ProductGroupViewModel>()
				.Finish();

			viewModel.CanViewEntity =
				CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(ProductGroup)).CanUpdate;

			return viewModel;
		}
	}
}
