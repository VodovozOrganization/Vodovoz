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
		public DiscountReasonViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = UoWGeneric.IsNew ? "Новое основание для скидки" : $"Основание для скидки \"{Entity.Name}\"";
		}

		public override bool Save(bool close)
		{
			if(Entity.Id != 0 && Entity.IsArchive == false)
			{// вывод из архива существующей сущности
				return base.Save(close);
			}

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var active = uow.Session.QueryOver<DiscountReason>()
					.Where(dr => dr.IsArchive == false && dr.Name == Entity.Name).SingleOrDefault();
				if(active != null && Entity.IsArchive == false)
				{
					CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
						"Уже существует основание для скидки с таким названием.\n" +
						"Создание нового основания невозможно.\n" +
						"Существующее основание:\n" +
						$"Код: {active.Id}\n" +
						$"Название: {active.Name}");
					return false;
				}

				var archived = uow.Session.QueryOver<DiscountReason>()
					.Where(dr => dr.IsArchive && dr.Name == Entity.Name).SingleOrDefault();
				if(archived != null && Entity.IsArchive == false)
				{
					if(CommonServices.InteractiveService.Question(
						"Уже существует основание для скидки с таким названием.\n" +
						"Создание нового основания невозможно.\n" +
						"Разархивировать существующее основание?"))
					{
						uow.Delete(Entity);
						archived.IsArchive = false;
						uow.Save(archived);
						uow.Commit();
						CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
							"Разархивировано основание для скидки:\n" +
							$"Код: {archived.Id}\n" +
							$"Название: {archived.Name}\n");
					}

					return false;
				}
			}
			return base.Save(close);
		}
	}
}
