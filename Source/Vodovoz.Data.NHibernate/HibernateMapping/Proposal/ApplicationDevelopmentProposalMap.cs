using FluentNHibernate.Mapping;
using Vodovoz.Domain.Proposal;

namespace Vodovoz.HibernateMapping.Proposal
{
    public class ApplicationDevelopmentProposalMap : ClassMap<ApplicationDevelopmentProposal>
    {
        public ApplicationDevelopmentProposalMap()
        {
            Table("application_development_proposals");

            Id(x => x.Id).GeneratedBy.Native();

            Map(x => x.Title).Column("title");
            Map(x => x.CreationDate).Column("creation_date").ReadOnly();
            Map(x => x.Location).Column("location");
            Map(x => x.Description).Column("description");
            Map(x => x.ProposalResponse).Column("proposal_response");
            Map(x => x.Status).CustomType<ApplicationDevelopmentProposalStatusStringType>();

            References(x => x.Author).Column("author_id");
        }
    }
}