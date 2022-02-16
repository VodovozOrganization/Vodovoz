using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class CarManufacturerJournalViewModel : SingleEntityJournalViewModelBase
		<CarManufacturer, CarManufacturerViewModel, CarManufacturerJournalNode>
	{
		public CarManufacturerJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал производителей автомобилей";
			UpdateOnChanges(typeof(CarManufacturer));
		}

		protected override Func<IUnitOfWork, IQueryOver<CarManufacturer>> ItemsSourceQueryFunction => uow =>
		{
			CarManufacturerJournalNode resultAlias = null;
			CarManufacturer carManufacturerAlias = null;

			var query = uow.Session.QueryOver(() => carManufacturerAlias);

			query.Where(GetSearchCriterion(
				() => carManufacturerAlias.Id,
				() => carManufacturerAlias.Name)
			);

			return query
				.SelectList(list => list
					.Select(x => carManufacturerAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(x => carManufacturerAlias.Name).WithAlias(() => resultAlias.Name))
				.OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<CarManufacturerJournalNode>());
		};

		protected override Func<CarManufacturerViewModel> CreateDialogFunction => () =>
			new CarManufacturerViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices);

		protected override Func<CarManufacturerJournalNode, CarManufacturerViewModel> OpenDialogFunction => node =>
			new CarManufacturerViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
