using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class UndeliveryObjectViewModel : EntityTabViewModelBase<UndeliveryObject>
	{
		public UndeliveryObjectViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory uowFactory, ICommonServices commonServices)
			: base(uowBuilder, uowFactory, commonServices)
		{
		}

		protected override bool BeforeSave()
		{
			if(Entity.IsArchive && UoW.HasChanges)
			{
				if(!AskQuestion("Будут архивированы все виды и детализации недовозов, привязанные к этому объекту недовоза, вы уверены?", "Внимание!!"))
				{
					return false;
				}

				foreach(var kind in UoW.Query<UndeliveryKind>()
					.Where(x => x.UndeliveryObject.Id == Entity.Id).List())
				{
					kind.IsArchive = true;

					foreach(var detalization in UoW.Query<UndeliveryDetalization>()
								.Where(x => x.UndeliveryKind.Id == kind.Id).List())
					{
						detalization.IsArchive = true;
					}
				}
			}

			return base.BeforeSave();
		}
	}
}
