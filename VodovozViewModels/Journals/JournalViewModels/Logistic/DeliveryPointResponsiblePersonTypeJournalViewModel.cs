using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
    /// <summary>
    /// View Model журнала типов ответственного за точку доставки лица
    /// </summary>
    public class DeliveryPointResponsiblePersonTypeJournalViewModel : FilterableSingleEntityJournalViewModelBase<DeliveryPointResponsiblePersonType, DeliveryPointResponsiblePersonTypeViewModel, DeliveryPointResponsiblePersonTypeJournalNode, DeliveryPointResponsiblePersonTypeJournalFilterViewModel>
    {
        public DeliveryPointResponsiblePersonTypeJournalViewModel(DeliveryPointResponsiblePersonTypeJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
            : base(filterViewModel, unitOfWorkFactory, commonServices)
        {
            TabName = "Журнал типов ответственного за точку доставки лица";

            UpdateOnChanges(
                typeof(DeliveryPointResponsiblePersonType)
            );
        }

        protected override Func<IUnitOfWork, IQueryOver<DeliveryPointResponsiblePersonType>> ItemsSourceQueryFunction => (uow) =>
        {
            DeliveryPointResponsiblePersonTypeJournalNode deliveryPointResponsiblePersonTypeJournalNodeAlias = null;
            DeliveryPointResponsiblePersonType deliveryPointResponsiblePersonTypeAlias = null;

            var query = uow.Session.QueryOver<DeliveryPointResponsiblePersonType>(() => deliveryPointResponsiblePersonTypeAlias);

            query.Where(GetSearchCriterion(
                () => deliveryPointResponsiblePersonTypeAlias.Id,
                () => deliveryPointResponsiblePersonTypeAlias.Title
            ));

            var result = query.SelectList(list => list
                .Select(u => u.Id).WithAlias(() => deliveryPointResponsiblePersonTypeJournalNodeAlias.Id)
                .Select(u => u.Title).WithAlias(() => deliveryPointResponsiblePersonTypeJournalNodeAlias.Name))
                .TransformUsing(Transformers.AliasToBean<DeliveryPointResponsiblePersonTypeJournalNode>());

            return result;
        };

        protected override Func<DeliveryPointResponsiblePersonTypeViewModel> CreateDialogFunction => () => new DeliveryPointResponsiblePersonTypeViewModel(
               EntityUoWBuilder.ForCreate(),
               UnitOfWorkFactory,
               commonServices);

        protected override Func<DeliveryPointResponsiblePersonTypeJournalNode, DeliveryPointResponsiblePersonTypeViewModel> OpenDialogFunction => (node) => new DeliveryPointResponsiblePersonTypeViewModel(
               EntityUoWBuilder.ForOpen(node.Id),
               UnitOfWorkFactory,
               commonServices);
    }
}
