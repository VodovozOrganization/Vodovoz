using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class OrderDocumentEntity : PropertyChangedBase
	{
		private int _id;
		OrderEntity _order;
		OrderEntity _attachedToOrder;

		/// <summary>
		/// Идентификатор документа заказа
		/// </summary>
		[Display(Name = "Идентификатор документа заказа")]
		public virtual int Id 
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Тип документа заказа
		/// </summary>
		[Display(Name = "Тип документа заказа")]
		public virtual OrderDocumentType Type { get; }

		/// <summary>
		/// Заказ для которого создавался документ
		/// </summary>
		/// <value>The order.</value>
		[Display(Name = "Создан для заказа")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		/// <summary>
		/// Заказ в котором будет отображатся этот документ. 
		/// (в котором везется этот документ клиенту, может не совпадать с заказом
		/// для которого создавался)
		/// </summary>
		[Display(Name = "Привязан к заказу")]
		public virtual OrderEntity AttachedToOrder
		{
			get => _attachedToOrder;
			set => SetField(ref _attachedToOrder, value);
		}

		/// <summary>
		/// Дата документа
		/// </summary>
		[Display(Name = "Дата документа")]
		public virtual DateTime? DocumentDate { get; }

		/// <summary>
		/// Наименование документа
		/// </summary>
		[Display(Name = "Наименование документа")]
		public virtual string Name { get; }
	}

	/// <summary>
	/// Интерфейс необходим для документов заказа, напротив которых должен быть крыжик
	/// "Без рекламы" в разделе "Документы" в диалоге заказа.
	/// </summary>
	public interface IAdvertisable
	{
		bool WithoutAdvertising { get; set; }
	}

	/// <summary>
	/// Интерфейс необходим для документов заказа, напротив которых должен быть крыжик
	/// "Без подписей и печати" в разделе "Документы" в диалоге заказа.
	/// </summary>
	public interface ISignableDocument
	{
		bool HideSignature { get; set; }
	}
}
