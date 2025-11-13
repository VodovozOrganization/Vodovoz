using QS.Commands;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Linq;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Goods;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class GroupGtinViewModel : DialogViewModelBase
	{
		private readonly Nomenclature _nomenclature;

		public GroupGtinViewModel(INavigationManager navigationManager, GroupGtin groupGtin, Nomenclature nomenclature)
			: this(navigationManager)
		{
			GroupGtin = groupGtin ?? throw new ArgumentNullException(nameof(groupGtin));

			_nomenclature = nomenclature;

			Title = $"Редактирование {groupGtin}";
		}

		public GroupGtinViewModel(INavigationManager navigationManager, Nomenclature nomenclature)
			: this(navigationManager)
		{
			_nomenclature = nomenclature ?? throw new ArgumentNullException(nameof(nomenclature));

			GroupGtin = new GroupGtin
			{
				Nomenclature = _nomenclature
			};

			Title = $"Новый групповой Gtin для номенклатуры {nomenclature.Id} {nomenclature.Name}";
		}

		private GroupGtinViewModel(INavigationManager navigationManager) : base(navigationManager)
		{
			CloseCommand = new DelegateCommand(CloseEditDialog);
		}

		public GroupGtin GroupGtin { get; }

		public DelegateCommand CloseCommand { get; set; }

		private void CloseEditDialog()
		{
			if(GroupGtin.Id == 0 && !_nomenclature.GroupGtins.Any(x => x.GtinNumber == GroupGtin.GtinNumber && x.CodesCount == GroupGtin.CodesCount))
			{
				_nomenclature.GroupGtins.Add(GroupGtin);
			}

			Close(false, CloseSource.Self);
		}
	}
}
