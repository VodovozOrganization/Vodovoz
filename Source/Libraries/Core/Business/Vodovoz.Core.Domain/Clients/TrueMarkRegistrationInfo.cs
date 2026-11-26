using TrueMark.Contracts;
using Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Информация о регистрации контрагента в Честном Знаке
	/// </summary>
	public class TrueMarkRegistrationInfo
	{
		/// <summary>
		/// Общий статус регистрации в ЧЗ
		/// </summary>
		public RegistrationInChestnyZnakStatus Status { get; set; }

		/// <summary>
		/// Товарные группы, к которым подключен участник
		/// </summary>
		public IReadOnlyCollection<TrueMarkProductGroup> ProductGroups { get; set; } = Array.Empty<TrueMarkProductGroup>();

		/// <summary>
		///	Название товарных групп, к которым подключен участник
		/// </summary>
		public string ProductGroupsDisplay => string.Join(", ", ProductGroups
			.Select(x => x.GetEnumDisplayName())
			.DefaultIfEmpty("Нет"));
	}
}
