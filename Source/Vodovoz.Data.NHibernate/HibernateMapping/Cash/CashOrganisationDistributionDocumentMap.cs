using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Cash
{
	public class CashOrganisationDistributionDocumentMap : ClassMap<CashOrganisationDistributionDocument>
	{
		public CashOrganisationDistributionDocumentMap()
		{
			Table("cash_organisation_distribution_documents");

			DiscriminateSubClassesOnColumn("type");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CreationDate).Column("creation_date");
			Map(x => x.LastEditedTime).Column("last_edited_time");
			Map(x => x.Amount).Column("amount");

			References(x => x.Author).Column("author_id");
			References(x => x.Employee).Column("employee_id");
			References(x => x.LastEditor).Column("last_editor_id");
			References(x => x.Organisation).Column("organisation_id");
			References(x => x.OrganisationCashMovementOperation).Column("organisation_cash_movement_operation_id");
			References(x => x.Income).Column("cash_income_id");
			References(x => x.Expense).Column("cash_expense_id");
		}
	}

	public class SelfDeliveryCashDistributionDocumentMap : SubclassMap<SelfDeliveryCashDistributionDocument>
	{
		public SelfDeliveryCashDistributionDocumentMap()
		{
			DiscriminatorValue("SelfDeliveryCashDistributionDoc");
			References(x => x.Order).Column("order_id");
		}
	}

	public class IncomeCashDistributionDocumentMap : SubclassMap<IncomeCashDistributionDocument>
	{
		public IncomeCashDistributionDocumentMap()
		{
			DiscriminatorValue("IncomeCashDistributionDoc");
		}
	}

	public class ExpenseCashDistributionDocumentMap : SubclassMap<ExpenseCashDistributionDocument>
	{
		public ExpenseCashDistributionDocumentMap()
		{
			DiscriminatorValue("ExpenseCashDistributionDoc");
		}
	}

	public class RouteListItemCashDistributionDocumentMap : SubclassMap<RouteListItemCashDistributionDocument>
	{
		public RouteListItemCashDistributionDocumentMap()
		{
			DiscriminatorValue("RouteListItemCashDistributionDoc");
			References(x => x.RouteListItem).Column("route_list_address_id");
		}
	}

	public class AdvanceIncomeCashDistributionDocumentMap : SubclassMap<AdvanceIncomeCashDistributionDocument>
	{
		public AdvanceIncomeCashDistributionDocumentMap()
		{
			DiscriminatorValue("AdvanceIncomeCashDistributionDoc");
			References(x => x.AdvanceReport).Column("cash_advance_report_id");
		}
	}

	public class AdvanceExpenseCashDistributionDocumentMap : SubclassMap<AdvanceExpenseCashDistributionDocument>
	{
		public AdvanceExpenseCashDistributionDocumentMap()
		{
			DiscriminatorValue("AdvanceExpenseCashDistributionDoc");
			References(x => x.AdvanceReport).Column("cash_advance_report_id");
		}
	}

	public class FuelExpenseCashDistributionDocumentMap : SubclassMap<FuelExpenseCashDistributionDocument>
	{
		public FuelExpenseCashDistributionDocumentMap()
		{
			DiscriminatorValue("FuelExpenseCashDistributionDoc");
			References(x => x.FuelDocument).Column("fuel_document_id");
		}
	}
}