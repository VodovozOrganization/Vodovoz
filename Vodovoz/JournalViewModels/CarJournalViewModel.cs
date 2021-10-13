﻿using System;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.CommonEnums;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;

namespace Vodovoz.JournalViewModels
{
	public class CarJournalViewModel : FilterableSingleEntityJournalViewModelBase<Car, CarsDlg, CarJournalNode, CarJournalFilterViewModel>
	{
		public CarJournalViewModel(CarJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(filterViewModel, unitOfWorkFactory, commonServices)
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
			CarVersion carVersionAlias = null;
			CarModel carModelAlias = null;

			var query = uow.Session.QueryOver<Car>(() => carAlias)
				.Left.JoinAlias(c => c.Driver, () => driverAlias)
				.JoinEntityAlias(() => carVersionAlias, () => carAlias.Id == carVersionAlias.Car.Id, JoinType.InnerJoin)
				.Left.JoinAlias(c => c.CarModel, () => carModelAlias);

			if(FilterViewModel != null) {
				if(!FilterViewModel.IncludeArchive) {
					query.Where(() => !carModelAlias.IsArchive);
				}

				switch(FilterViewModel.VisitingMasters) {
					case AllYesNo.All:
						break;
					case AllYesNo.Yes:
						query.Where(() => driverAlias.VisitingMaster);
						break;
					case AllYesNo.No:
						query.Where(Restrictions.Disjunction()
							.Add(Restrictions.IsNull(Projections.Property(() => driverAlias.Id)))
							.Add(() => !driverAlias.VisitingMaster));
						break;
				}
				
				switch(FilterViewModel.Raskat) {
					case AllYesNo.All:
						break;
					case AllYesNo.Yes:
						query.Where(() => carVersionAlias.IsRaskat);
						break;
					case AllYesNo.No:
						query.Where(() => !carVersionAlias.IsRaskat);
						break;
				}

				if(FilterViewModel.RestrictedCarTypesOfUse != null) {
					if(!FilterViewModel.RestrictedCarTypesOfUse.Any()) {
						query.Where(Restrictions.IsNull(Projections.Property(() => carAlias.Id)));
					}
					else {
						query.WhereRestrictionOn(() => carModelAlias.TypeOfUse)
							.IsIn(FilterViewModel.RestrictedCarTypesOfUse.ToArray());
					}
				}
			}

			query.Where(GetSearchCriterion(
				() => carAlias.Id,
				() => carAlias.CarModel,
				() => carAlias.RegistrationNumber,
				() => driverAlias.Name,
				() => driverAlias.LastName,
				() => driverAlias.Patronymic
			));

			var result = query.SelectList(list => list
			.Select(c => c.Id).WithAlias(() => carJournalNodeAlias.Id)
			.Select(c => c.CarModel).WithAlias(() => carJournalNodeAlias.Model)
			.Select(c => c.RegistrationNumber).WithAlias(() => carJournalNodeAlias.RegistrationNumber)
			.Select(() => carModelAlias.IsArchive).WithAlias(() => carJournalNodeAlias.IsArchive)
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
