using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Заказ по перемещению подотчетных в ЧЗ товаров между организациями
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "заказы по перемещению подотчетных в ЧЗ товаров между организациями",
		Nominative = "заказ по перемещению подотчетного в ЧЗ товара между организациями")]
	public class TransferOrder : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private OrganizationEntity _seller;
		private OrganizationEntity _customer;
		private IEnumerable<TransferOrderTrueMarkCode> _trueMarkCodes = new List<TransferOrderTrueMarkCode>();

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Продавец
		/// </summary>
		[Display(Name = "Продавец")]
		public virtual OrganizationEntity Seller
		{
			get => _seller;
			set => SetField(ref _seller, value);
		}

		/// <summary>
		/// Покупатель
		/// </summary>
		[Display(Name = "Покупатель")]
		public virtual OrganizationEntity Customer
		{
			get => _customer;
			set => SetField(ref _customer, value);
		}

		/// <summary>
		/// Коды ЧЗ
		/// </summary>
		[Display(Name = "Коды ЧЗ")]
		public virtual IEnumerable<TransferOrderTrueMarkCode> TrueMarkCodes
		{
			get => _trueMarkCodes;
			set => SetField(ref _trueMarkCodes, value);
		}
	}
}
