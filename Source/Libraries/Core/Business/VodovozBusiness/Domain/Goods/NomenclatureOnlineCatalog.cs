using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods
{
	public abstract class NomenclatureOnlineCatalog : PropertyChangedBase, IDomainObject
	{
		private string _name;
		
		public virtual int Id { get; set; }

		public string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
		
		public abstract NomenclatureOnlineParameterType Type { get; }
	}
}
