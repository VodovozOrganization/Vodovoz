using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class ProductGroupViewModel : EntityTabViewModelBase<ProductGroup>
	{
		public ProductGroupViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IProductGroupJournalFactory productGroupJournalFactory)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			ProductGroupSelectorFactory = (productGroupJournalFactory ?? throw new ArgumentNullException(nameof(productGroupJournalFactory)))
				.CreateProductGroupAutocompleteSelectorFactory();

			SetArchiveCommand = new DelegateCommand(SetArchive);
			SetIsHighlightInCarLoadDocumentCommand = new DelegateCommand(SetIsHighlightInCarLoadDocument);
			SetIsNeedAdditionalControlCommand = new DelegateCommand(SetIsNeedAdditionalControl, () => CanEditAdditionalControlSettingsInProductGroup);
		}

		public DelegateCommand SetArchiveCommand { get; }
		public DelegateCommand SetIsHighlightInCarLoadDocumentCommand { get; }
		public DelegateCommand SetIsNeedAdditionalControlCommand { get; }

		public bool CanEditOnlineStoreParametersInProductGroup =>
			Entity.Id == 0
			|| CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.ProductGroup.CanEditOnlineStoreParametersInProductGroups);

		public bool CanEditAdditionalControlSettingsInProductGroup =>
			CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.ProductGroup.CanEditAdditionalControlSettingsInProductGroups);

		public IEntityAutocompleteSelectorFactory ProductGroupSelectorFactory { get; }

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
	}
}
