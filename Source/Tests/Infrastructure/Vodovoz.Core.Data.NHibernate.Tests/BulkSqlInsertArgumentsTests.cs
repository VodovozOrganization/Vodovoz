using NSubstitute;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.NHibernate.Extensions;
using Xunit;

namespace Vodovoz.Core.Data.NHibernate.Tests
{
	/// <summary>
	/// Проверяет аргументы массовой вставки. Эти проверки идут первыми, поэтому БД и сессия не нужны:
	/// UoW здесь заглушка, коллекция не связана с NHibernate.
	/// </summary>
	public class BulkSqlInsertArgumentsTests
	{
		public enum InsertMode
		{
			Sync,
			Async
		}

		private class Item
		{
		}

		private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

		private static async Task Insert(
			InsertMode mode,
			ICollection<Item> collection,
			IUnitOfWork uow,
			IEnumerable<Item> newItems,
			int batchSize = 1000,
			CancellationToken cancellationToken = default)
		{
			if(mode == InsertMode.Sync)
			{
				collection.BulkSqlInsert(uow, newItems, batchSize);
			}
			else
			{
				await collection.BulkSqlInsertAsync(uow, newItems, batchSize, cancellationToken);
			}
		}

		private void AssertSessionNotUsed()
		{
			_ = _uow.DidNotReceive().Session;
			_uow.DidNotReceive().OpenTransaction();
		}

		/// <summary>
		/// A-01. Коллекция не передана.
		/// </summary>
		[Theory]
		[InlineData(InsertMode.Sync)]
		[InlineData(InsertMode.Async)]
		public async Task BulkSqlInsert_WhenCollectionIsNull_ThrowsArgumentNullException(InsertMode mode)
		{
			var exception = await Assert.ThrowsAsync<ArgumentNullException>(
				() => Insert(mode, null, _uow, new[] { new Item() }));

			Assert.Equal("collection", exception.ParamName);
			AssertSessionNotUsed();
		}

		/// <summary>
		/// A-02. UoW не передан.
		/// </summary>
		[Theory]
		[InlineData(InsertMode.Sync)]
		[InlineData(InsertMode.Async)]
		public async Task BulkSqlInsert_WhenUowIsNull_ThrowsArgumentNullException(InsertMode mode)
		{
			var exception = await Assert.ThrowsAsync<ArgumentNullException>(
				() => Insert(mode, new ObservableList<Item>(), null, new[] { new Item() }));

			Assert.Equal("uow", exception.ParamName);
		}

		/// <summary>
		/// A-03. Новые элементы не переданы.
		/// </summary>
		[Theory]
		[InlineData(InsertMode.Sync)]
		[InlineData(InsertMode.Async)]
		public async Task BulkSqlInsert_WhenNewItemsIsNull_ThrowsArgumentNullException(InsertMode mode)
		{
			var exception = await Assert.ThrowsAsync<ArgumentNullException>(
				() => Insert(mode, new ObservableList<Item>(), _uow, null));

			Assert.Equal("newItems", exception.ParamName);
			AssertSessionNotUsed();
		}

		/// <summary>
		/// A-04. Размер пачки меньше единицы.
		/// </summary>
		[Theory]
		[InlineData(InsertMode.Sync, 0)]
		[InlineData(InsertMode.Sync, -1)]
		[InlineData(InsertMode.Async, 0)]
		[InlineData(InsertMode.Async, -1)]
		public async Task BulkSqlInsert_WhenBatchSizeLessThanOne_ThrowsArgumentOutOfRangeException(InsertMode mode, int batchSize)
		{
			var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
				() => Insert(mode, new ObservableList<Item>(), _uow, new[] { new Item() }, batchSize));

			Assert.Equal("batchSize", exception.ParamName);
			AssertSessionNotUsed();
		}

		/// <summary>
		/// A-05. Пустой набор: метод ничего не делает, даже если коллекция не связана с NHibernate и транзакции нет.
		/// </summary>
		[Theory]
		[InlineData(InsertMode.Sync)]
		[InlineData(InsertMode.Async)]
		public async Task BulkSqlInsert_WhenNewItemsEmpty_DoesNothing(InsertMode mode)
		{
			var observable = new ObservableList<Item>();
			var plainList = new List<Item>();

			await Insert(mode, observable, _uow, new Item[0]);
			await Insert(mode, observable, _uow, Enumerable.Empty<Item>().Select(x => x));
			await Insert(mode, plainList, _uow, new List<Item>());

			Assert.Empty(observable);
			Assert.Empty(plainList);
			AssertSessionNotUsed();
		}

		/// <summary>
		/// A-05. Пустой набор не отменяет проверку размера пачки.
		/// </summary>
		[Theory]
		[InlineData(InsertMode.Sync)]
		[InlineData(InsertMode.Async)]
		public async Task BulkSqlInsert_WhenNewItemsEmptyAndBatchSizeInvalid_ThrowsArgumentOutOfRangeException(InsertMode mode)
		{
			await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
				() => Insert(mode, new ObservableList<Item>(), _uow, new Item[0], 0));
		}

		/// <summary>
		/// A-06. В наборе есть null: это ошибка аргумента, она идёт раньше обращения к сессии.
		/// </summary>
		[Theory]
		[InlineData(InsertMode.Sync)]
		[InlineData(InsertMode.Async)]
		public async Task BulkSqlInsert_WhenNewItemsContainsNull_ThrowsArgumentException(InsertMode mode)
		{
			var exception = await Assert.ThrowsAsync<ArgumentException>(
				() => Insert(mode, new ObservableList<Item>(), _uow, new[] { new Item(), null }));

			Assert.Equal("newItems", exception.ParamName);
			AssertSessionNotUsed();
		}

		/// <summary>
		/// A-07. Один и тот же объект дважды в наборе: это ошибка аргумента, она идёт раньше обращения к сессии.
		/// </summary>
		[Theory]
		[InlineData(InsertMode.Sync)]
		[InlineData(InsertMode.Async)]
		public async Task BulkSqlInsert_WhenNewItemsContainsSameObjectTwice_ThrowsArgumentException(InsertMode mode)
		{
			var item = new Item();

			var exception = await Assert.ThrowsAsync<ArgumentException>(
				() => Insert(mode, new ObservableList<Item>(), _uow, new[] { item, item }));

			Assert.Equal("newItems", exception.ParamName);
			AssertSessionNotUsed();
		}

		/// <summary>
		/// A-08. Коллекция не связана с NHibernate.
		/// </summary>
		[Theory]
		[InlineData(InsertMode.Sync)]
		[InlineData(InsertMode.Async)]
		public async Task BulkSqlInsert_WhenCollectionNotPersistent_ThrowsInvalidOperationException(InsertMode mode)
		{
			await Assert.ThrowsAsync<InvalidOperationException>(
				() => Insert(mode, new ObservableList<Item>(), _uow, new[] { new Item() }));
			await Assert.ThrowsAsync<InvalidOperationException>(
				() => Insert(mode, new List<Item>(), _uow, new[] { new Item() }));

			_uow.DidNotReceive().OpenTransaction();
		}

		/// <summary>
		/// A-09. Токен отменён до вызова: метод не начинает никаких действий.
		/// </summary>
		[Fact]
		public async Task BulkSqlInsertAsync_WhenTokenAlreadyCancelled_ThrowsOperationCanceledException()
		{
			using(var cancellation = new CancellationTokenSource())
			{
				cancellation.Cancel();

				await Assert.ThrowsAnyAsync<OperationCanceledException>(
					() => Insert(InsertMode.Async, new ObservableList<Item>(), _uow, new[] { new Item() }, cancellationToken: cancellation.Token));
			}

			AssertSessionNotUsed();
		}

		/// <summary>
		/// A-10. При нескольких ошибках сразу первой бросается ошибка аргумента, а не состояния.
		/// </summary>
		[Theory]
		[InlineData(InsertMode.Sync)]
		[InlineData(InsertMode.Async)]
		public async Task BulkSqlInsert_WhenArgumentAndStateInvalid_ThrowsArgumentErrorFirst(InsertMode mode)
		{
			await Assert.ThrowsAsync<ArgumentNullException>(
				() => Insert(mode, new List<Item>(), null, new[] { new Item() }));
			await Assert.ThrowsAsync<ArgumentNullException>(
				() => Insert(mode, new List<Item>(), _uow, null));
			await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
				() => Insert(mode, new List<Item>(), _uow, new[] { new Item() }, 0));
		}

		/// <summary>
		/// D-11. Ленивый набор перечисляется ровно один раз.
		/// </summary>
		[Theory]
		[InlineData(InsertMode.Sync)]
		[InlineData(InsertMode.Async)]
		public async Task BulkSqlInsert_WhenNewItemsLazy_EnumeratesOnce(InsertMode mode)
		{
			var enumerations = 0;

			IEnumerable<Item> Generate()
			{
				enumerations++;
				yield return new Item();
			}

			await Record.ExceptionAsync(() => Insert(mode, new ObservableList<Item>(), _uow, Generate()));

			Assert.Equal(1, enumerations);
		}
	}
}
