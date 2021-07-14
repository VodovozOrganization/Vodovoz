using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Orders;

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
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var matchedNames = uow.Session.QueryOver<DiscountReason>()
					.Where(dr => dr.Id != Entity.Id)
					.And(dr=> dr.Name == Entity.Name).List();
				var active = matchedNames.FirstOrDefault(dr => !dr.IsArchive);
				if(active != null)
				{
					CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
						"Уже существует основание для скидки с таким названием.\n" +
						"Сохранение текущего основания невозможно.\n" +
						"Существующее основание:\n" +
						$"Код: {active.Id}\n" +
						$"Название: {active.Name}");
					return false;
				}

				var archived = matchedNames.FirstOrDefault(dr => dr.IsArchive);
				if(archived != null)
				{
					if(CommonServices.InteractiveService.Question(
						"Уже существует основание для скидки с таким названием.\n" +
						"Сохранение текущего основания невозможно.\n" +
						"Разархивировать существующее основание?"))
					{
						archived.IsArchive = false;
						uow.Save(archived);
						uow.Commit();
						CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
							"Разархивировано основание для скидки:\n" +
							$"Код: {archived.Id}\n" +
							$"Название: {archived.Name}\n");
					}
					base.Close(false, CloseSource.Cancel);
					return false;
				}
			}
			return base.Save(close);
		}
	}
}
