using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System.Collections.Generic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class UndeliveryKindViewModel : EntityTabViewModelBase<UndeliveryKind>
	{
		public UndeliveryKindViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			UndeliveryObjects = UoW.Session.QueryOver<UndeliveryObject>().List();

			TabName = "Вид недовоза";
		}

		public IList<UndeliveryObject> UndeliveryObjects { get; }

		protected override bool BeforeSave()
		{
			if(Entity.IsArchive && UoW.HasChanges)
			{
				if(!AskQuestion("Будут архивированы все детализации привязанные к этому виду недовоза, вы уверены?", "Внимание!!"))
				{
					return false;
				}

				foreach(var detalizationst in UoW.Query<UndeliveryDetalization>()
					.Where(x => x.UndeliveryKind.Id == Entity.Id).List())
				{
					detalizationst.IsArchive = true;
				}
			}

			return base.BeforeSave();
		}
	}
}
