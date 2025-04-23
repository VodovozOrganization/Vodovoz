using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents
{
	public class TaxcomDocflowActionMap : ClassMap<TaxcomDocflowAction>
	{
		public TaxcomDocflowActionMap()
		{
			Table("taxcom_docflow_actions");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.DocFlowState).Column("state");
			Map(x => x.TrueMarkTraceabilityStatus).Column("true_mark_traceability_status");
			Map(x => x.ErrorMessage).Column("error_message");
			Map(x => x.Time).Column("time");
			Map(x => x.TaxcomDocflowId).Column("taxcom_docflow_id");
		}
	}
}
