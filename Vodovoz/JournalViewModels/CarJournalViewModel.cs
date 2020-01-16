using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Journal.Search;
using QS.Project.Journal.Search.Criterion;
using QS.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;

namespace Vodovoz.JournalViewModels
{
	public class CarJournalViewModel : FilterableSingleEntityJournalViewModelBase<Car, CarsDlg, CarJournalNode, CarJournalFilterViewModel>
	{
		public CarJournalViewModel(CarJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, ICriterionSearch criterionSearch) : base(filterViewModel, unitOfWorkFactory, commonServices, criterionSearch)
		{
			TabName = "Журнал автомобилей";
			UpdateOnChanges(
				typeof(Car),
				typeof(Employee)
			);
		}

		protected override Func<IUnitOfWork, IQueryOver<Car>> ItemsSourceQueryFunction => (uow) => {
			CarJournalNode carJournalNodeAlias = null;
			Car carAlias = null;
			Employee driverAlias = null;

			var query = uow.Session.QueryOver<Car>(() => carAlias)
			.Left.JoinAlias(c => c.Driver, () => driverAlias);

			if(FilterViewModel != null && !FilterViewModel.IncludeArchive) {
				query.Where(c => !c.IsArchive);
			}

			query.Where(GetSearchCriterion(
				() => carAlias.Id,
				() => carAlias.Model,
				() => carAlias.RegistrationNumber,
				() => driverAlias.Name,
				() => driverAlias.LastName,
				() => driverAlias.Patronymic
			));

			var result = query.SelectList(list => list
			.Select(c => c.Id).WithAlias(() => carJournalNodeAlias.Id)
			.Select(c => c.Model).WithAlias(() => carJournalNodeAlias.Model)
			.Select(c => c.RegistrationNumber).WithAlias(() => carJournalNodeAlias.RegistrationNumber)
			.Select(Projections.SqlFunction(
					   new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(' ', ?2, ?1, ?3)"),
					   NHibernateUtil.String,
						   Projections.Property(() => driverAlias.Name),
					       Projections.Property(() => driverAlias.LastName),
						   Projections.Property(() => driverAlias.Patronymic))
						   )
						   .WithAlias(() => carJournalNodeAlias.DriverName))
				.TransformUsing(Transformers.AliasToBean<CarJournalNode>());

			return result;
		};

		protected override Func<CarsDlg> CreateDialogFunction => () => new CarsDlg();

		protected override Func<CarJournalNode, CarsDlg> OpenDialogFunction => (node) => new CarsDlg(node.Id);
	}
}
