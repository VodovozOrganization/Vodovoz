using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.StoredEmails
{
	public class BulkEmailOrder : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private BulkEmail _bulkEmail;
		private Order _order;

        /// <summary>
        /// Идентификатор
        /// </summary>
        [Display(Name = "Идентификатор")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Письмо массовой рассылки
		/// </summary>
		[Display(Name = "Письмо массовой рассылки")]
		public virtual BulkEmail BulkEmail
		{
			get => _bulkEmail;
			set => SetField(ref _bulkEmail, value);
		}

		/// <summary>
		/// Заказ
		/// </summary>
		[Display(Name = "Заказ")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}
    }
}
