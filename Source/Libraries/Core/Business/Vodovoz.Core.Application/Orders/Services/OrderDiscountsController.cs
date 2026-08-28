using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Controllers;
using Vodovoz.Core.Application.Sale;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class OrderDiscountsController : SaleDiscountController, IOrderDiscountsController
	{
		private readonly IGeneralSettings _generalSettings;
		private readonly IOrderSettings _orderSettings;

		public OrderDiscountsController(
			ILogger<OrderDiscountsController> logger,
			INomenclatureFixedPriceController fixedPriceController,
			IDiscountReasonRepository discountReasonRepository,
			IDiscountReasonSettings discountReasonSettings,
			IGeneralSettings generalSettings,
			IOrderSettings orderSettings
			)
			: base(logger, fixedPriceController, discountReasonRepository, discountReasonSettings)
		{
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
		}
		
		public void SetCustomDiscountForOrder(
			IUnitOfWork uow,
			DiscountReason reason,
			IDiscountValue discountValue,
			IEnumerable<IApplyDiscountReasonItem> saleItems)
		{
			foreach(var saleItem in saleItems)
			{
				SetCustomDiscountForOrderItem(uow, saleItem, reason, discountValue);
			}
		}

		public void SetDiscountFromDiscountReasonForOrder(
			DiscountReason reason,
			IEnumerable<IApplyDiscountReasonItem> saleItems,
			bool canChangeDiscountValue,
			out string messages)
		{
			messages = null;
			var i = 0;

			foreach(var saleItem in saleItems)
			{
				SetDiscountFromDiscountReasonForOrderItem(reason, saleItem, canChangeDiscountValue, out string message);

				if(message != null)
				{
					messages += $"№{i + 1} {message}";
				}

				i++;
			}
		}

		public bool SetDiscountFromDiscountReasonForOrderItem(
			DiscountReason reason, IApplyDiscountReasonItem orderItem, bool canChangeDiscountValue, out string message)
		{
			message = null;
			
			var canApplyResult = IsApplicableDiscount(reason, orderItem);
			
			if(canApplyResult.IsFailure)
			{
				return false;
			}
			
			if(!canChangeDiscountValue
				&& orderItem is OrderItem oi
				&& OrderItemContainsPromoSetOrFixedPrice(oi))
			{
				message = $"{orderItem.Nomenclature.Name}\n";
				return false;
			}

			ClearDiscounts(orderItem);
			var addDiscountResult = AddDiscount(reason, orderItem);

			if(addDiscountResult.IsFailure)
			{
				var error = addDiscountResult.Errors.FirstOrDefault();
				message = $"{orderItem.Nomenclature.Name} - {error?.Message}\n";
				return false;
			}

			return true;
		}
		
		public override void RecalculateDiscount(IDataContext context)
		{
			/*if(!CheckInitializedProperties())
			{
				return;
			}*/
			
			if(context.Data is not OrderRecalculateDiscount data)
			{
				throw new InvalidOperationException(
					$"Передаваемый контекст для пересчета скидки в заказе должен быть {nameof(OrderRecalculateDiscount)}");
			}
			
			var saleItem = data.SaleItem;
			var newDiscountValue = data.DiscountValue;

			if(saleItem.CurrentCount == 0)
			{
				if(data.OrderInUndeliveredStatus)
				{
					RemoveAndPreserveDiscount(saleItem);
				}
				else
				{
					ClearDiscounts(saleItem);
				}
			}
			else
			{
				CalculateAndSetDiscount(saleItem, newDiscountValue);
			}
		}
		
		/// <summary>
		/// Удаляет текущие скидки и сохраняет их в <see cref="OriginalDiscountReasons"/>.
		/// Восстановить скидку можно методом <see cref="RestoreOriginalDiscount"/>.
		/// </summary>
		public void RemoveAndPreserveDiscount(IPreserveDiscount saleItem)
		{
			if(saleItem.DiscountData.DiscountMoney > 0)
			{
				saleItem.OriginalDiscountMoney = saleItem.DiscountData.DiscountMoney;
				saleItem.OriginalDiscount = saleItem.DiscountData.Discount;

				saleItem.OriginalDiscountReasons.Clear();
				foreach(var reason in saleItem.DiscountReasons)
				{
					saleItem.OriginalDiscountReasons.Add(reason);
				}
			}
			
			ClearDiscounts(saleItem);
		}
		
		public void RecalculateDiscountWithPreserveOrRestoreDiscount(IPreserveDiscount saleItem)
		{
			/*if(!CheckInitializedProperties())
			{
				return;
			}*/
			
			if(saleItem.CurrentCount == 0)
			{
				RemoveAndPreserveDiscount(saleItem);
			}
			else
			{
				RestoreOriginalDiscount(saleItem);
			}
		}

		public void TryRestoreOriginalDiscount(IPreserveDiscount saleItem)
		{
			if(!saleItem.OriginalDiscountMoney.HasValue && !saleItem.OriginalDiscount.HasValue)
			{
				return;
			}

			saleItem.SetDiscount(
				DiscountValue.Create(
					saleItem.DiscountData.IsDiscountMoney,
					saleItem.OriginalDiscountMoney ?? 0,
					saleItem.OriginalDiscount ?? 0));

			saleItem.DiscountReasons.Clear();
			foreach(var reason in saleItem.OriginalDiscountReasons)
			{
				saleItem.DiscountReasons.Add(reason);
			}

			saleItem.OriginalDiscountMoney = null;
			saleItem.OriginalDiscount = null;
			saleItem.OriginalDiscountReasons.Clear();
		}
		
		public virtual OkResult UpdateClientSecondOrderDiscount(IUnitOfWork uow, ISecondOrderDiscount source)
		{
			if(!_generalSettings.GetIsClientsSecondOrderDiscountActive)
			{
				return OkResult.Failure();
			}

			var discountReasonId = _orderSettings.GetClientsSecondOrderDiscountReasonId;

			if(source.IsSecondOrder)
			{
				return SetClientSecondOrderDiscount(uow, source, discountReasonId);
			}

			ResetClientSecondOrderDiscount(source, discountReasonId);
			return OkResult.Success();
		}

		public void CopyDiscounts(IUnitOfWork uow, IApplyDiscountReasonItem saleItem, IApplyDiscountReasonItem copyingSaleItem)
		{
			CopyDiscounts(uow, saleItem, copyingSaleItem.DiscountData, copyingSaleItem.DiscountReasons, copyingSaleItem.PersonalDiscount);
		}

		public void CopyOriginalDiscounts(IUnitOfWork uow, IApplyDiscountReasonItem saleItem, IPreserveDiscount copyingSaleItem)
		{
			CopyDiscounts(
				uow,
				saleItem,
				DiscountValue.Create(
					copyingSaleItem.DiscountData.IsDiscountMoney,
					copyingSaleItem.OriginalDiscount ?? 0m,
					copyingSaleItem.OriginalDiscountMoney ?? 0m),
				copyingSaleItem.OriginalDiscountReasons,
				copyingSaleItem.PersonalDiscount);
		}

		private OkResult SetClientSecondOrderDiscount(
			IUnitOfWork uow,
			ISecondOrderDiscount source,
			int discountReasonId)
		{
			if(!source.IsSecondOrder)
			{
				return OkResult.Failure();
			}

			var sb = new StringBuilder();
			var count = 0;
			
			var itemsWithoutSecondOrderDiscount = source.SaleItems
				.Where(x => x.DiscountReasons.All(r => r.Id != discountReasonId))
				.ToList();

			foreach(var item in itemsWithoutSecondOrderDiscount)
			{
				var result = SetClientSecondOrderDiscount(uow, source, item, discountReasonId);

				if(result.IsFailureWithDescription)
				{
					sb.AppendLine(result.Description);
					count++;
				}
			}

			if(count == itemsWithoutSecondOrderDiscount.Count && count != 0)
			{
				return OkResult.Failure("Не удалось применить скидку для второго заказа клиента");
			}
			
			return sb.Length > 0
				? OkResult.Failure(sb.ToString())
				: OkResult.Success();
		}

		private void ResetClientSecondOrderDiscount(ISecondOrderDiscount source, int discountReasonId)
		{
			if(source.IsSecondOrder)
			{
				return;
			}

			var orderItemsHavingClientsSecondOrderDiscount = new List<IApplyDiscountReasonItem>();

			foreach(var item in source.SaleItems)
			{
				if(item.DiscountReasons.Any(r => r.Id == discountReasonId))
				{
					orderItemsHavingClientsSecondOrderDiscount.Add(item);
				}
			}
				
			ClearDiscounts(orderItemsHavingClientsSecondOrderDiscount);
		}

		private OkResult SetClientSecondOrderDiscount(
			IUnitOfWork uow,
			ISecondOrderDiscount source,
			IApplyDiscountReasonItem saleItem,
			int discountReasonId
			)
		{
			if(!source.IsSecondOrder)
			{
				return OkResult.Failure();
			}

			if(saleItem.DiscountReasons.Any()
				|| saleItem.PromoSet != null)
			{
				return OkResult.Failure();
			}

			var discountReason = DiscountReasonRepository.GetDiscountReason(uow, discountReasonId);

			if(discountReason != null)
			{
				var result = SetDiscountFromDiscountReasonForOrderItem(discountReason, saleItem, true, out var message);

				if(!result || message != null)
				{
					OkResult.Failure(
						"Не удалось применить скидку для второго заказа клиента к позиции:" +
						$" {saleItem.Nomenclature.Name} - {saleItem.CurrentCount}{saleItem.Nomenclature.Unit?.Name}");
				}
			}

			return OkResult.Success();
		}

		private void RestoreOriginalDiscount(IPreserveDiscount saleItem)
		{
			TryRestoreOriginalDiscount(saleItem);
			CalculateAndSetDiscount(saleItem, saleItem.DiscountData);
		}

		/// <summary>
		/// Установка определенной скидки на строку заказа с прикреплением указанного основания скидки,
		/// после проверки возможности этого действия
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="reason">Основание скидки</param>
		/// <param name="discountValue">Значение скидки</param>
		/// <param name="saleItem">Строка заказа</param>
		private void SetCustomDiscountForOrderItem(
			IUnitOfWork uow,
			IApplyDiscountReasonItem saleItem,
			DiscountReason reason,
			IDiscountValue discountValue)
		{
			var canApplyResult = IsApplicableDiscount(reason, saleItem);
			
			if(canApplyResult.IsFailure)
			{
				return;
			}

			ClearDiscounts(saleItem);
			var addingResult = AddDiscount(reason, saleItem, true);

			if(addingResult.IsFailure)
			{
				return;
			}
			
			SetCustomDiscount(uow, saleItem, discountValue);
		}

		private void CopyDiscounts(
			IUnitOfWork uow,
			IApplyDiscountReasonItem saleItem,
			IDiscountValue discountValue,
			IEnumerable<DiscountReason> copyingDiscountReasons,
			PersonalDiscount copyingPersonalDiscount
			)
		{
			saleItem.SetDiscount(discountValue);
			
			saleItem.DiscountReasons.Clear();
			foreach(var reason in copyingDiscountReasons)
			{
				if(reason != null && !saleItem.DiscountReasons.Contains(reason))
				{
					saleItem.DiscountReasons.Add(reason);
				}
			}

			if(copyingPersonalDiscount != null)
			{
				saleItem.PersonalDiscount = PersonalDiscount.Copy(copyingPersonalDiscount);
				uow.Save(saleItem.PersonalDiscount);
			}
		}
	}
}
