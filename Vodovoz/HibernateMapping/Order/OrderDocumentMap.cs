using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.HMap
{
	public class OrderDocumentMap : ClassMap<OrderDocument>
	{
		public OrderDocumentMap ()
		{
			Table ("order_documents");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			DiscriminateSubClassesOnColumn ("type");
			References (x => x.Order).Column ("order_id");
		}
	}

	public class OrderAgreementMap : SubclassMap<OrderAgreement>
	{
		public OrderAgreementMap ()
		{
			DiscriminatorValue ("order_agreement");
			References (x => x.AdditionalAgreement).Column ("agreement_id");
		}
	}

	public class OrderContractMap : SubclassMap<OrderContract>
	{
		public OrderContractMap ()
		{
			DiscriminatorValue ("order_contract");
			References (x => x.Contract).Column ("contract_id");
		}
	}

}