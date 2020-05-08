using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum ResidueEquipmentDirection
	{
		[Display(Name = "К клиенту")]
		ToClient,
		[Display(Name = "От клиента")]
		FromClient
	}

	public class ResidueEquipmentDirectionStringType : NHibernate.Type.EnumStringType
	{
		public ResidueEquipmentDirectionStringType() : base(typeof(ResidueEquipmentDirection))
		{
		}
	}
}
