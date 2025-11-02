using QS.Navigation;
using System;
using Vodovoz.Core.Application.Entity;
using Vodovoz.Domain.Logistic;
using Vodovoz.Presentation.ViewModels.Common;

namespace Vodovoz.Presentation.ViewModels.Logistic
{
	public class AcceptBeforeViewModel : EntityReactiveDialog<AcceptBefore>
	{
		public AcceptBeforeViewModel(
			IEntityIdentifier entityId,
			EntityModelFactory entityModelFactory,
			INavigationManager navigator
		) : base(entityId, entityModelFactory, navigator)
		{
			Title = "Время приема до";
		}

		private int? hours;
		public int? Hours
		{
			get => hours;
			set
			{
				if(SetField(ref hours, value, () => Hours))
				{
					Entity.Time = new TimeSpan(value ?? 0, Minutes ?? 0, 0);
				}
			}
		}

		private int? minutes;
		public int? Minutes
		{
			get => minutes;
			set
			{
				if(SetField(ref minutes, value, () => Minutes))
					Entity.Time = new TimeSpan(Hours ?? 0, value ?? 0, 0);
			}
		}
	}
}
