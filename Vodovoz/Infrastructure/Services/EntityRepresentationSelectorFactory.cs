using System;
using QS.Project.Journal.EntitySelector;
using QS.RepresentationModel.GtkUI;

namespace Vodovoz.Infrastructure.Services
{
	public class EntityRepresentationSelectorFactory : IEntityRepresentationSelectorFactory
	{
		public IEntitySelector CreateForRepresentation(Type entityType, Func<IRepresentationModel> modelFunc, string tabName = null)
		{
			return new EntityRepresentationSelectorAdapter(entityType, modelFunc(), tabName);
		}
	}
}
