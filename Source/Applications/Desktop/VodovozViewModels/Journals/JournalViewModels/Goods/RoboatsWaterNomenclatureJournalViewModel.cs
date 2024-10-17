using NHibernate;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Goods
{
	public class RoboatsWaterNomenclatureJournalViewModel : SingleEntityJournalViewModelBase<Nomenclature, NomenclatureViewModel, WaterJournalNode>
	{
		public RoboatsWaterNomenclatureJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices
		) : base(unitOfWorkFactory, commonServices)
		{
			TabName = "Выбор номенклатуры воды";

			UpdateOnChanges(
				typeof(Nomenclature),
				typeof(MeasurementUnits)
			);
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
		}

		protected override Func<IUnitOfWork, IQueryOver<Nomenclature>> ItemsSourceQueryFunction => (uow) => {
			Nomenclature nomenclatureAlias = null;
			MeasurementUnits unitAlias = null;
			WaterJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.Unit, () => unitAlias);

			itemsQuery.Where(
				GetSearchCriterion(
					() => nomenclatureAlias.Name,
					() => nomenclatureAlias.Id
				)
			);

			itemsQuery
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water)
				.Where(() => nomenclatureAlias.IsDisposableTare == false)
				.Where(() => nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Where(() => !nomenclatureAlias.IsArchive)
				.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => unitAlias.Name).WithAlias(() => resultAlias.UnitName)
					.Select(() => unitAlias.Digits).WithAlias(() => resultAlias.UnitDigits)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
				)
				.OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<WaterJournalNode>());

			return itemsQuery;
		};

		protected override Func<NomenclatureViewModel> CreateDialogFunction => () =>
			throw new NotSupportedException("Не поддерживается создание номенклатуры воды из текущего журнала");

		protected override Func<WaterJournalNode, NomenclatureViewModel> OpenDialogFunction => (node) =>
			 throw new NotSupportedException("Не поддерживается открытие номенклатуры воды из текущего журнала");
	}
}
