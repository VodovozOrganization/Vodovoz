using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
    /// <summary>
    /// View Model журнала ответственных за точку доставки лиц
    /// </summary>
    public class DeliveryPointResponsiblePersonJournalViewModel : FilterableSingleEntityJournalViewModelBase<DeliveryPointResponsiblePerson, DeliveryPointResponsiblePersonViewModel, DeliveryPointResponsiblePersonJournalNode, DeliveryPointResponsiblePersonJournalFilterViewModel>
    {
        public DeliveryPointResponsiblePersonJournalViewModel(DeliveryPointResponsiblePersonJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, bool hideJournalForOpenDialog = false, bool hideJournalForCreateDialog = false) : base(filterViewModel, unitOfWorkFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
        {
            TabName = "Журнал ответственных за точку доставки лиц";
        }

        protected override Func<IUnitOfWork, IQueryOver<DeliveryPointResponsiblePerson>> ItemsSourceQueryFunction => (uow) =>
        {
            DeliveryPointResponsiblePersonJournalNode deliveryPointResponsiblePersonJournalNodeAlias = null;
            DeliveryPointResponsiblePerson deliveryPointResponsiblePersonAlias = null;
            Employee employeeAlias = null;

            var query = uow.Session.QueryOver<DeliveryPointResponsiblePerson>(() => deliveryPointResponsiblePersonAlias);

            var employeeProjection = Projections.SqlFunction(
                new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(' ', ?1, ?2, ?3)"),
                NHibernateUtil.String,
                Projections.Property(() => employeeAlias.LastName),
                Projections.Property(() => employeeAlias.Name),
                Projections.Property(() => employeeAlias.Patronymic)
            );

            query.Where(GetSearchCriterion(
                () => deliveryPointResponsiblePersonAlias.Id,
                () => deliveryPointResponsiblePersonAlias.Phone,
                () => deliveryPointResponsiblePersonAlias.DeliveryPointResponsiblePersonType.Title,
                () => employeeProjection
            ));

            var result = query.SelectList(list => list
                .Select(u => u.Id).WithAlias(() => deliveryPointResponsiblePersonJournalNodeAlias.Id)
                .Select(u => u.DeliveryPointResponsiblePersonType).WithAlias(() => deliveryPointResponsiblePersonJournalNodeAlias.DeliveryPointResponsiblePersonType)
                .Select(u => u.Employee).WithAlias(() => deliveryPointResponsiblePersonJournalNodeAlias.Employee))
                .TransformUsing(Transformers.AliasToBean<DeliveryPointResponsiblePersonJournalNode>());

            return result;
        };

        protected override Func<DeliveryPointResponsiblePersonViewModel> CreateDialogFunction => () => new DeliveryPointResponsiblePersonViewModel(
               EntityUoWBuilder.ForCreate(),
               UnitOfWorkFactory,
               commonServices);

        protected override Func<DeliveryPointResponsiblePersonJournalNode, DeliveryPointResponsiblePersonViewModel> OpenDialogFunction => (node) => new DeliveryPointResponsiblePersonViewModel(
               EntityUoWBuilder.ForOpen(node.Id),
               UnitOfWorkFactory,
               commonServices);
    }
}
