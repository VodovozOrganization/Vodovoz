using System;
using QS.Project.Journal.EntitySelector;
using QS.RepresentationModel.GtkUI;
namespace Vodovoz.Infrastructure.Services
{
	public class EntityRepresentationAdapterFactory : IEntityAutocompleteSelectorFactory
	{
		private readonly Func<IRepresentationModel> modelFunc;
		private readonly string tabName;
		public Type EntityType { get; private set; }

		[Obsolete("Не корректно работает поиск вкладки c этой моделью в QS.Tdi.Gtk.TdiNotebook", true)]
		public EntityRepresentationAdapterFactory(Type entityType, Func<IRepresentationModel> modelFunc, string tabName = null)
		{
			EntityType = entityType;
			this.modelFunc = modelFunc ?? throw new ArgumentNullException(nameof(modelFunc));
			this.tabName = tabName;
		}

		public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
		{
			return new EntityRepresentationSelectorAdapter(EntityType, modelFunc(), tabName, multipleSelect);
		}

		public IEntitySelector CreateSelector(bool multipleSelect = false)
		{
			return CreateAutocompleteSelector();
		}
	}
}
