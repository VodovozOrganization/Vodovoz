using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.Domain;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class EquipmentKindViewModel : EntityTabViewModelBase<EquipmentKind>
	{
		public EquipmentKindViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Вид оборудования";
		}

		public Array EquipmentTypes => Enum.GetValues(typeof(EquipmentType));
	}
}
