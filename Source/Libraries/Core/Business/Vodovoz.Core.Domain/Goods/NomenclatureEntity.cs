using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods
{
	public class NomenclatureEntity : PropertyChangedBase, IDomainObject, IBusinessObject
	{
		private int _id;
		private bool _isAccountableInTrueMark;
		private string _gtin;

		public virtual IUnitOfWork UoW { set; get; }

		public virtual int Id
		{
			get => _id;
			set
			{
				if(SetField(ref _id, value))
				{
					UpdateFileInformations();
				}
			}
		}

		[Display(Name = "Подлежит учету в Честном Знаке")]
		public virtual bool IsAccountableInTrueMark
		{
			get => _isAccountableInTrueMark;
			set => SetField(ref _isAccountableInTrueMark, value);
		}

		[Display(Name = "Номер товарной продукции GTIN")]
		public virtual string Gtin
		{
			get => _gtin;
			set => SetField(ref _gtin, value);
		}

		protected virtual void UpdateFileInformations()
		{

		}
	}
}
