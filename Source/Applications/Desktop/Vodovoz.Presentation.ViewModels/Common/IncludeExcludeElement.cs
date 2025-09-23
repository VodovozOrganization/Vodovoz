using QS.DomainModel.Entity;
using System;
using System.Data.Bindings.Collections.Generic;

namespace Vodovoz.Presentation.ViewModels.Common
{
	public partial class IncludeExcludeElement : PropertyChangedBase
	{
		private bool _include = false;

		private bool _exclude = false;
		private bool _isEditable = true;

		public delegate void OnElementIncluded(IncludeExcludeElement sender, EventArgs eventArgs);
		public delegate void OnElementExcluded(IncludeExcludeElement sender, EventArgs eventArgs);

		public event OnElementIncluded ElementIncluded;
		public event OnElementIncluded ElementUnIncluded;

		public event OnElementExcluded ElementExcluded;
		public event OnElementExcluded ElementUnExcluded;

		public virtual string Number { get; set; }

		public string Title { get; set; }

		private bool CanChangeInclude(bool isEnabling) => isEnabling || !IsRadio;

		public bool Include
		{
			get => _include;
			set
			{
				if(CanChangeInclude(value) && SetField(ref _include, value))
				{
					if(value)
					{
						Exclude = false;

						foreach(var child in Children)
						{
							child.Include = true;
						}

						ElementIncluded?.Invoke(this, EventArgs.Empty);
					}
					else
					{
						ElementUnIncluded?.Invoke(this, EventArgs.Empty);
					}
				}
			}
		}

		public bool IsRadio { get; internal set; }

		public void UnIncludeForRadio() => SetField(ref _include, false);

		public bool Exclude
		{
			get => _exclude;
			set
			{
				if(SetField(ref _exclude, value))
				{
					if(value)
					{
						Include = false;

						foreach(var child in Children)
						{
							child.Exclude = true;
						}

						ElementExcluded?.Invoke(this, EventArgs.Empty);
					}
					else
					{
						ElementUnExcluded?.Invoke(this, EventArgs.Empty);
					}
				}
			}
		}

		/// <summary>
		/// Возможность редактирования элемента(установки включен/исключен)
		/// В текущей реализации сделано не полноценно. Только под блокировку выбора автора заказа текущего сотрудника, если нет спец прав
		/// </summary>
		public bool IsEditable
		{
			get => _isEditable;
			set =>  SetField(ref _isEditable, value);
		}

		public IncludeExcludeElement Parent { get; set; } = null;

		public GenericObservableList<IncludeExcludeElement> Children { get; } = new GenericObservableList<IncludeExcludeElement>();
	}
}
