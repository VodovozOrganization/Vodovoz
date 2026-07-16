using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using IDocument = Vodovoz.Domain.Orders.OrdersWithoutShipment.IDocument;

namespace Vodovoz.Domain.Orders.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы заказа",
		Nominative = "документ заказа")]
	[HistoryTrace]
	public abstract class OrderDocument : PropertyChangedBase, IDocument
	{
		public virtual int Id { get; set; }

		Order order;
		/// <summary>
		/// Заказ для которого создавался документ
		/// </summary>
		/// <value>The order.</value>
		[Display(Name = "Создан для заказа")]
		public virtual Order Order {
			get => order;
			set => SetField(ref order, value, () => Order);
		}

		Order attachedToOrder;
		private DocumentOrganizationCounter _documentOrganizationCounter;

		/// <summary>
		/// Заказ в котором будет отображатся этот документ. 
		/// (в котором везется этот документ клиенту, может не совпадать с заказом
		/// для которого создавался)
		/// </summary>
		[Display(Name = "Привязан к заказу")]
		public virtual Order AttachedToOrder {
			get => attachedToOrder;
			set => SetField(ref attachedToOrder, value, () => AttachedToOrder);
		}

		/// <summary>
		/// Счетчик документов
		/// </summary>
		[Display(Name = "Счетчик документов")]
		public virtual DocumentOrganizationCounter DocumentOrganizationCounter
		{
			get => _documentOrganizationCounter;
			set => SetField(ref _documentOrganizationCounter, value);
		}

		public abstract string Name { get; }

		public abstract OrderDocumentType Type { get; }
		
		public abstract DateTime? DocumentDate { get; }
        
		public virtual string DocumentDateText => DocumentDate?.ToShortDateString() ?? "не указана";
	}


	public interface ITemplateOdtDocument
	{
		void PrepareTemplate(IUnitOfWork uow, IDocTemplateRepository docTemplateRepository);
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
