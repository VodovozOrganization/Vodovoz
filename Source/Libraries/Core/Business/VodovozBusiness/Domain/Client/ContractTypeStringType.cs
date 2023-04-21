namespace Vodovoz.Domain.Client
{
	public class ContractTypeStringType : NHibernate.Type.EnumStringType
	{
		public ContractTypeStringType() : base(typeof(ContractType)) { }
	}
}
