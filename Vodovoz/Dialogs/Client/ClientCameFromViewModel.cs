using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Client;

namespace Vodovoz.Dialogs.Client
{
	public class ClientCameFromViewModel : EntityTabViewModelBase<ClientCameFrom>
	{
		public ClientCameFromViewModel(IEntityUoWBuilder ctorParam, ICommonServices commonServices) : base(ctorParam, commonServices)
		{
			TabName = "Откуда клиент знает о нас";
		}
	}
}