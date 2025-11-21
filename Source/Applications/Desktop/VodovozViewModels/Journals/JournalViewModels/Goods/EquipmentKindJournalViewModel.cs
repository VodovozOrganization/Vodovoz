using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Goods
{
	public class EquipmentKindJournalViewModel : SingleEntityJournalViewModelBase<EquipmentKind, EquipmentKindViewModel,
		EquipmentKindJournalNode>
	{
		public EquipmentKindJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал видов оборудования";

			UpdateOnChanges(typeof(EquipmentKind));
		}

		protected override Func<IUnitOfWork, IQueryOver<EquipmentKind>> ItemsSourceQueryFunction => (uow) =>
		{
			EquipmentKind equipmentKindAlias = null;
			EquipmentKindJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => equipmentKindAlias);

			itemsQuery.Where(GetSearchCriterion(
				() => equipmentKindAlias.Id,
				() => equipmentKindAlias.Name)
			);

			itemsQuery.SelectList(list => list
					.Select(() => equipmentKindAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => equipmentKindAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => equipmentKindAlias.WarrantyCardType).WithAlias(() => resultAlias.WarrantyCardType)
				)
				.TransformUsing(Transformers.AliasToBean<EquipmentKindJournalNode>());

			return itemsQuery;
		};

		protected override Func<EquipmentKindViewModel> CreateDialogFunction => () =>
			new EquipmentKindViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices);

		protected override Func<EquipmentKindJournalNode, EquipmentKindViewModel> OpenDialogFunction =>
			(node) => new EquipmentKindViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
