using System;
using System.ComponentModel;

namespace Vodovoz
{
	internal class MagicAttribute : Attribute {}

	[Magic]
	public abstract class PropertyChangedBase : INotifyPropertyChanged {
		protected virtual void RaisePropertyChanged(string propName) {
			var e = PropertyChanged;
			if (e != null)
				e(this, new PropertyChangedEventArgs(propName));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}

