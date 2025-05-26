using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class TaxcomDocflowActionMap : ClassMap<TaxcomDocflowAction>
	{
		public TaxcomDocflowActionMap()
		{
			Table("taxcom_docflow_actions");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.DocFlowState).Column("state");
			Map(x => x.ErrorMessage).Column("error_message");
			Map(x => x.Time).Column("time");
			Map(x => x.TaxcomDocflowId).Column("taxcom_docflow_id");
		}
	}
}
