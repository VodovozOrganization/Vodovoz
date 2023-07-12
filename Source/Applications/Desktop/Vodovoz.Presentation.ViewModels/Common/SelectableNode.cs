using System;

namespace Vodovoz.Presentation.ViewModels.Common
{
	/// <summary>
	/// Нода с возможностью выбора
	/// </summary>
	/// <typeparam name="T">Тип значения ноды</typeparam>
	public class SelectableNode<T>
	{
		private bool _selected;

		public event EventHandler<SelectionChanged<T>> SelectChanged;

		/// <summary>
		/// Приватный конструктор для иммутабельности ноды
		/// </summary>
		/// <param name="obj"></param>
		private SelectableNode(T obj)
		{
			Value = obj;
			Selected = false;
		}

		/// <summary>
		/// Состояние выбора ноды (выбрана/не выбрана)
		/// </summary>
		public bool Selected
		{
			get => _selected;
			set
			{
				if(_selected != value)
				{
					_selected = value;
					SelectChanged?.Invoke(this, SelectionChanged<T>.Create(this, value));
				}
			}
		}

		/// <summary>
		/// Тихий выбор, без оповещения собитием <see cref="SelectChanged"/> с аргументами <see cref="SelectionChanged{T}"/>
		/// </summary>
		public void SilentUnselect()
		{
			_selected = false;
		}

		/// <summary>
		/// Значение ноды
		/// </summary>
		public T Value { get; }

		/// <summary>
		/// Фабричный метод создания ноды, принимает обьект типа <see cref="T"/>
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static SelectableNode<T> Create(T obj)
			=> new SelectableNode<T>(obj);
	}
}
