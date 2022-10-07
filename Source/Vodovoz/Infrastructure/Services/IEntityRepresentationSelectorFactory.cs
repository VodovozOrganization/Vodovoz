using System;
using QS.Project.Journal.EntitySelector;
using QS.RepresentationModel.GtkUI;

namespace Vodovoz.Infrastructure.Services
{
	public interface IEntityRepresentationSelectorFactory
	{
		IEntitySelector CreateForRepresentation(Type entityType, Func<IRepresentationModel> modelFunc, string tabName = null);
	}
}
