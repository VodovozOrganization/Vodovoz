using System;

namespace Vodovoz.Presentation.ViewModels.Common
{
	/// <summary>
	/// Аргументы изменения выделения ноды <see cref="SelectableNode{T}"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SelectionChanged<T> : EventArgs
	{
		/// <summary>
		/// Приватный конструктор для иммутабельности
		/// </summary>
		/// <param name="selectableNode"></param>
		/// <param name="value"></param>
		private SelectionChanged(SelectableNode<T> selectableNode, bool value)
		{
			SelectableNode = selectableNode;
			Value = value;
		}

		/// <summary>
		/// Нода
		/// </summary>
		public SelectableNode<T> SelectableNode { get; }

		/// <summary>
		/// Значение выделения (выделена/не выделена)
		/// </summary>
		public bool Value { get; }

		/// <summary>
		/// Фабричный метод создания аргументов события
		/// </summary>
		/// <param name="selectableNode">Нода</param>
		/// <param name="value">Новое значение выделения</param>
		/// <returns></returns>
		public static SelectionChanged<T> Create(SelectableNode<T> selectableNode, bool value) =>
			new SelectionChanged<T>(selectableNode, value);
	}
}
