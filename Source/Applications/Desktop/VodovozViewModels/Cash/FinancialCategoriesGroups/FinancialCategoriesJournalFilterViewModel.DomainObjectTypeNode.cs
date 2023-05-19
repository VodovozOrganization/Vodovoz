using QS.DomainModel.Entity;
using System;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.Cash.FinancialCategoriesGroups
{
	public partial class FinancialCategoriesJournalFilterViewModel
	{
		public class DomainObjectTypeNode : PropertyChangedBase
		{
			private bool _selected;
			public virtual bool Selected
			{
				get => _selected;
				set => SetField(ref _selected, value);
			}

			public Type Type { get; }

			public string Title => Type.GetClassUserFriendlyName().Nominative.CapitalizeSentence();

			public DomainObjectTypeNode(Type type, bool selected = false)
			{
				Type = type;
				Selected = selected;
			}
		}
	}
}
