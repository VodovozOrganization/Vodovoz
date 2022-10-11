using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.HibernateMapping.Cash
{
    public class OrganizationCashTransferDocumentMap: ClassMap<OrganizationCashTransferDocument>
    {
        public OrganizationCashTransferDocumentMap()
        {
            Table("organization_cash_transfer_documents");

            Id(x => x.Id).Column("id").GeneratedBy.Native();

            Map(x => x.DocumentDate).Column("document_date");
            Map(x => x.TransferedSum).Column("transfered_sum");
            Map(x => x.Comment).Column("comment");

            References(x => x.Author).Column("employee_id");
            References(x => x.OrganizationFrom).Column("organization_from_id");
            References(x => x.OrganizationTo).Column("organization_to_id");
            References(x => x.OrganisationCashMovementOperationFrom).Column("organisation_cash_movement_operation_from_id");
            References(x => x.OrganisationCashMovementOperationTo).Column("organisation_cash_movement_operation_to_id");
        }
    }
}
