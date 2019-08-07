using System;
using QS.DomainModel.Config;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.JournalNodes;
using Vodovoz.Domain.Complaints;
using NHibernate;
using Vodovoz.ViewModels.Complaints;
using QS.Project.Domain;
using Vodovoz.TempAdapters;
using Vodovoz.Infrastructure.Services;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Client;
using NHibernate.Criterion;
using Order = Vodovoz.Domain.Orders.Order;
using NHibernate.Dialect.Function;
using NHibernate.Transform;

namespace Vodovoz.JournalViewModels
{
	public class ComplaintsJournalViewModel : MultipleEntityJournalViewModelBase<ComplaintJournalNode>
	{
		private readonly IEntityConfigurationProvider entityConfigurationProvider;
		private readonly ICommonServices commonServices;
		private readonly IUndeliveriesViewOpener undeliveriesViewOpener;
		private readonly IEmployeeService employeeService;
		private readonly IEntitySelectorFactory employeeSelectorFactory;
		private readonly IEntityAutocompleteSelectorFactory counterpartySelectorFactory;
		private readonly IEntityAutocompleteSelectorFactory orderSelectorFactory;
		private readonly IEntityAutocompleteSelectorFactory fineSelectorFactory;

		public ComplaintsJournalViewModel(
			IEntityConfigurationProvider entityConfigurationProvider, 
			ICommonServices commonServices,
			IUndeliveriesViewOpener undeliveriesViewOpener,
			IEmployeeService employeeService,
			IEntitySelectorFactory employeeSelectorFactory,
			IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
			IEntityAutocompleteSelectorFactory orderSelectorFactory,
			IEntityAutocompleteSelectorFactory fineSelectorFactory
		) : base(entityConfigurationProvider, commonServices)
		{
			this.entityConfigurationProvider = entityConfigurationProvider ?? throw new ArgumentNullException(nameof(entityConfigurationProvider));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			this.undeliveriesViewOpener = undeliveriesViewOpener ?? throw new ArgumentNullException(nameof(undeliveriesViewOpener));
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.employeeSelectorFactory = employeeSelectorFactory ?? throw new ArgumentNullException(nameof(employeeSelectorFactory));
			this.counterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			this.orderSelectorFactory = orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory));
			this.fineSelectorFactory = fineSelectorFactory ?? throw new ArgumentNullException(nameof(fineSelectorFactory));

			RegisterInnerComplaints();
			RegisterComplaints();
		}

		private IQueryOver<Complaint> GetComplaintQuery()
		{
			ComplaintJournalNode resultAlias = null;

			Complaint complaintAlias = null;
			Employee authorAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			ComplaintGuiltyItem complaintGuiltyItemAlias = null;
			Employee guiltyEmployeeAlias = null;
			Subdivision guiltySubdivisionAlias = null;
			Fine fineAlias = null;
			Order orderAlias = null;
			ComplaintDiscussion dicussionAlias = null;
			Subdivision subdivisionAlias = null;

			var authorProjection = Projections.SqlFunction("GET_PERSON_NAME_WITH_INITIALS", NHibernateUtil.String,
				Projections.Property(() => authorAlias.LastName),
				Projections.Property(() => authorAlias.Name),
				Projections.Property(() => authorAlias.Patronymic)
			);

			var subdivisionsProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(?1 SEPARATOR ?2)"),
				NHibernateUtil.String,
				Projections.Property(() => subdivisionAlias.Name),
				Projections.Constant("\n"));

			var plannedCompletionDateProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DATE_FORMAT(?1, \"%d.%m.%Y\") SEPARATOR ?2)"),
				NHibernateUtil.String,
				Projections.Property(() => dicussionAlias.PlannedCompletionDate),
				Projections.Constant("\n"));

			var counterpartyWithAddressProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(', ', ?1, COMPILE_ADDRESS(?2))"),
				NHibernateUtil.String,
				Projections.Property(() => counterpartyAlias.Name),
				Projections.Property(() => deliveryPointAlias.Id));

			var guiltyEmployeeProjection = Projections.SqlFunction("GET_PERSON_NAME_WITH_INITIALS", NHibernateUtil.String,
				Projections.Property(() => guiltyEmployeeAlias.LastName),
				Projections.Property(() => guiltyEmployeeAlias.Name),
				Projections.Property(() => guiltyEmployeeAlias.Patronymic)
			);

			var guiltiesProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(" +
					"CASE ?1" +
						$"WHEN '{nameof(ComplaintGuiltyTypes.Client)}' THEN 'Клиент'" +
						$"WHEN '{nameof(ComplaintGuiltyTypes.None)}' THEN 'Нет'" +
						$"WHEN '{nameof(ComplaintGuiltyTypes.Employee)}' THEN ?2" +
						$"WHEN '{nameof(ComplaintGuiltyTypes.Subdivision)}' THEN ?3" +
						"ELSE o.payment_type\n\t" +
					"END" +
					" SEPARATOR ?4)"),
				NHibernateUtil.String,
				Projections.Property(() => complaintGuiltyItemAlias.GuiltyType),
				guiltyEmployeeProjection,
				Projections.Property(() => guiltySubdivisionAlias.Name),
				Projections.Constant("\n"));

			var finesProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(CONCAT(ROUND(?1, 2), ' р.')  SEPARATOR ?2)"),
				NHibernateUtil.String,
				Projections.Property(() => fineAlias.TotalMoney),
				Projections.Constant("\n"));

			var query = UoW.Session.QueryOver<Complaint>(() => complaintAlias)
				.Left.JoinAlias(() => complaintAlias.CreatedBy, () => authorAlias)
				.Left.JoinAlias(() => complaintAlias.Counterparty, () => counterpartyAlias)
				.Left.JoinAlias(() => complaintAlias.Order, () => orderAlias)
				.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => complaintAlias.Guilties, () => complaintGuiltyItemAlias)
				.Left.JoinAlias(() => complaintAlias.Fines, () => fineAlias)
				.Left.JoinAlias(() => complaintAlias.ComplaintDiscussions, () => dicussionAlias)
				.Left.JoinAlias(() => dicussionAlias.Subdivision, () => subdivisionAlias)
				.Left.JoinAlias(() => complaintGuiltyItemAlias.Employee, () => guiltyEmployeeAlias)
				.Left.JoinAlias(() => complaintGuiltyItemAlias.Subdivision, () => guiltySubdivisionAlias);

			query.Where(GetSearchCriterion(
				() => complaintAlias.Id,
				() => complaintAlias.ComplaintText,
				() => complaintAlias.ResultText,
				() => counterpartyAlias.Name,
				() => deliveryPointAlias.CompiledAddress
			));

			query.SelectList(list => list
				.SelectGroup(() => complaintAlias.Id).WithAlias(() => resultAlias.Id)
				.Select(() => complaintAlias.CreationDate).WithAlias(() => resultAlias.Date)
				.Select(() => complaintAlias.ComplaintType).WithAlias(() => resultAlias.Type)
				.Select(() => complaintAlias.Status).WithAlias(() => resultAlias.Status)
				.Select(subdivisionsProjection).WithAlias(() => resultAlias.WorkInSubdivision)
				.Select(plannedCompletionDateProjection).WithAlias(() => resultAlias.PlannedCompletionDate)
				.Select(counterpartyWithAddressProjection).WithAlias(() => resultAlias.ClientNameWithAddress)
				.Select(guiltiesProjection).WithAlias(() => resultAlias.Guilties)
				.Select(authorProjection).WithAlias(() => resultAlias.Author)
				.Select(finesProjection).WithAlias(() => resultAlias.Fines)
				.Select(() => complaintAlias.ComplaintText).WithAlias(() => resultAlias.ComplaintText)
				.Select(() => complaintAlias.ResultText).WithAlias(() => resultAlias.ResultText)
				.Select(() => complaintAlias.ActualCompletionDate).WithAlias(() => resultAlias.ActualCompletionDate)
			);

			query.TransformUsing(Transformers.AliasToBean<ComplaintJournalNode>());

			return query;
		}

		private void RegisterInnerComplaints()
		{
			var innerComplaintConfig = RegisterEntity<Complaint>(GetComplaintQuery);

			innerComplaintConfig.AddDocumentConfiguration(
				//функция диалога создания документа
				() => new CreateInnerComplaintViewModel(EntityConstructorParam.ForCreate(), commonServices),
				//функция диалога открытия документа
				(ComplaintJournalNode node) => new ComplaintViewModel(
					EntityConstructorParam.ForOpen(node.Id),
					commonServices,
					undeliveriesViewOpener,
					employeeService,
					employeeSelectorFactory,
					counterpartySelectorFactory,
					orderSelectorFactory,
					fineSelectorFactory,
					entityConfigurationProvider
				),
				//функция идентификации документа 
				(ComplaintJournalNode node) => {
					return node.EntityType == typeof(Complaint);
				}
			);

			//завершение конфигурации
			innerComplaintConfig.FinishConfiguration();
		}

		private void RegisterComplaints()
		{
			var complaintConfig = RegisterEntity<Complaint>(GetComplaintQuery);

			complaintConfig.AddDocumentConfiguration(
				//функция диалога создания документа
				() => new CreateComplaintViewModel(
					EntityConstructorParam.ForCreate(), 
					counterpartySelectorFactory,
					orderSelectorFactory,
					commonServices
				),
				//функция диалога открытия документа
				(ComplaintJournalNode node) => new ComplaintViewModel(
					EntityConstructorParam.ForOpen(node.Id),
					commonServices,
					undeliveriesViewOpener,
					employeeService,
					employeeSelectorFactory,
					counterpartySelectorFactory,
					orderSelectorFactory,
					fineSelectorFactory,
					entityConfigurationProvider
				),
				//функция идентификации документа 
				(ComplaintJournalNode node) => {
					return node.EntityType == typeof(Complaint);
				}
			);

			//завершение конфигурации
			complaintConfig.FinishConfiguration();
		}

	}
}
