using System;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class DiscountReasonViewModel : EntityTabViewModelBase<DiscountReason>
	{
		private readonly IOrderRepository _orderRepository;

		public DiscountReasonViewModel(
			IOrderRepository orderRepository,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			TabName = UoWGeneric.IsNew ? "Новое основание для скидки" : $"Основание для скидки \"{Entity.Name}\"";
		}

		public override bool Save(bool close)
		{
			if(Entity.Id != 0 && Entity.IsArchive == false)
			{// вывод из архива существующей сущности
				return base.Save(close);
			}
			var all = _orderRepository.GetDiscountReasons(UoW);
			var activeList = all.Where(dr => dr.IsArchive == false && dr.Name == Entity.Name).ToList();
			if(activeList.Any() && Entity.IsArchive == false)
			{
				var active = activeList.First();
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
					"Уже существует основание для скидки с таким названием.\n" +
					"Создание нового основания невозможно.\n" +
					"Существующее основание:\n" +
					$"Код: {active.Id}\n" +
					$"Название: {active.Name}");
				return false;
			}
			
			var archivedList = all.Where(dr => dr.IsArchive && dr.Name == Entity.Name).ToList();
			if(archivedList.Any() && Entity.IsArchive == false)
			{
				var archived = archivedList.First();
				if(CommonServices.InteractiveService.Question(
					"Уже существует основание для скидки с таким названием.\n" +
					"Создание нового основания невозможно.\n" +
					"Разархивировать существующее основание?"))
				{
					UoWGeneric.Delete(UoWGeneric.Root);
					archived.IsArchive = false;
					UoWGeneric.Save(archived);
					UoWGeneric.Commit();
					CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
						"Разархивировано основание для скидки:\n" +
						$"Код: {archived.Id}\n" +
						$"Название: {archived.Name}\n");
				}
				return false;
			}
			return base.Save(close);
		}
	}
}
