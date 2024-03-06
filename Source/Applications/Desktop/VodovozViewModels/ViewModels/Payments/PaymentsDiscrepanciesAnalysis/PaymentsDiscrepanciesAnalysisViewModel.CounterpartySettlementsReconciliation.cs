using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;
using static Vodovoz.EntityRepositories.Orders.OrderRepository;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		/// <summary>
		/// Сверка взаиморасчетов по контрагенту
		/// </summary>
		public class CounterpartySettlementsReconciliation
		{
			public static DateTime OldOrdersMaxDate = new DateTime(2020, 08, 12);

			private static readonly OrderStatus[] _availableOrderStatuses = new OrderStatus[]
			{
				OrderStatus.Accepted,
				OrderStatus.InTravelList,
				OrderStatus.OnLoading,
				OrderStatus.OnTheWay,
				OrderStatus.Shipped,
				OrderStatus.UnloadingOnStock,
				OrderStatus.Closed
			};

			private readonly IUnitOfWork _unitOfWork;
			private readonly IOrderRepository _orderRepository;
			private readonly IPaymentsRepository _paymentsRepository;
			private readonly ICounterpartyRepository _counterpartyRepository;
			private readonly IList<IList<string>> _rowsFromFile;

			private CounterpartySettlementsReconciliation(
				IUnitOfWork unitOfWork,
				IOrderRepository orderRepository,
				IPaymentsRepository paymentsRepository,
				ICounterpartyRepository counterpartyRepository,
				IList<IList<string>> rowsFromFile
				)
			{
				_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
				_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
				_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
				_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
				_rowsFromFile = rowsFromFile ?? throw new ArgumentNullException(nameof(rowsFromFile));
			}

			private void Initialize()
			{
				CounterpartyInn = GetClientInnFromFile();
				Counterparty = GetCounterpartyByInn(CounterpartyInn);

				OrderNodes = GetOrderNodes(Counterparty.Id);
				PaymentNodes = GetPaymentNodes(CounterpartyInn, Counterparty.Id);

				OrdersTotalSumInFile = GetCounterpartyOrdersTotalSumFromFile();
				PaymentsTotalSumInFile = GetCounterpartyPaymentsTotalSumFromFile();
				TotalDebtInFile = OrdersTotalSumInFile - PaymentsTotalSumInFile;
				OldDebtInFile = GetCounterpartyOldDebtFromFile();

				OrdersTotalSumInDatabase = GetCounterpartyOrdersTotalSum(Counterparty.Id);
				PaymentsTotalSumInDatabase = GetCounterpartyPaymentsTotalSum(Counterparty.Id, CounterpartyInn);
				TotalDebtInDatabase = GetCounterpartyTotalDebt(Counterparty.Id);
				OldDebtInDatabase = GetCounterpartyOldDebt(Counterparty.Id);
			}

			#region Properties

			public string CounterpartyInn { get; private set; }
			public Domain.Client.Counterparty Counterparty { get; private set; }
			public IDictionary<int, OrderDiscrepanciesNode> OrderNodes { get; private set; }
			public IDictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode> PaymentNodes { get; private set; }
			public decimal OrdersTotalSumInFile { get; private set; }
			public decimal PaymentsTotalSumInFile { get; private set; }
			public decimal TotalDebtInFile { get; private set; }
			public decimal OldDebtInFile { get; private set; }
			public decimal OrdersTotalSumInDatabase { get; private set; }
			public decimal PaymentsTotalSumInDatabase { get; private set; }
			public decimal TotalDebtInDatabase { get; private set; }
			public decimal OldDebtInDatabase { get; private set; }

			#endregion Properties

			public static CounterpartySettlementsReconciliation CreateCounterpartySettlementsReconciliationFromXml(
				IUnitOfWork unitOfWork,
				IOrderRepository orderRepository,
				IPaymentsRepository paymentsRepository,
				ICounterpartyRepository counterpartyRepository,
				string fileName)
			{
				var rowsFromXls = XlsParseHelper.GetRowsFromXls(fileName);

				var reconciliationOfMutualSettlements = 
					CreateCounterpartySettlementsReconciliation(unitOfWork, orderRepository, paymentsRepository, counterpartyRepository, rowsFromXls);

				reconciliationOfMutualSettlements.Initialize();

				return reconciliationOfMutualSettlements;
			}

			private static CounterpartySettlementsReconciliation CreateCounterpartySettlementsReconciliation(
				IUnitOfWork unitOfWork,
				IOrderRepository orderRepository,
				IPaymentsRepository paymentsRepository,
				ICounterpartyRepository counterpartyRepository,
				IList<IList<string>> rows)
			{
				return new CounterpartySettlementsReconciliation(
					unitOfWork,
					orderRepository,
					paymentsRepository,
					counterpartyRepository,
					rows);
			}

			private string GetClientInnFromFile()
			{
				var clientInn = string.Empty;

				foreach(var rowData in _rowsFromFile)
				{
					if(rowData.Count < 1)
					{
						continue;
					}

					if(rowData.Count > 1 && rowData[0].StartsWith("(Сосновцев"))
					{
						clientInn = XlsParseHelper.ParseClientInnFromString(rowData[1]);
						continue;
					}
				}

				if(string.IsNullOrEmpty(clientInn))
				{
					throw new Exception($"ИНН не найден в файле");
				}

				return clientInn;
			}

			private Domain.Client.Counterparty GetCounterpartyByInn(string counterpartyInn)
			{
				var counterparties = GetCounterpartiesByInn(counterpartyInn);

				if(counterparties.Count != 1)
				{
					throw new Exception($"Найдено {counterparties.Count} контрагентов с ИНН {CounterpartyInn}. Должно быть 1");
				}

				return counterparties.First();
			}

			private IList<Domain.Client.Counterparty> GetCounterpartiesByInn(string counterpartyInn)
			{
				return _counterpartyRepository.GetCounterpartiesByINN(_unitOfWork, counterpartyInn);
			}

			#region GetOrderNodes
			private IDictionary<int, OrderDiscrepanciesNode> GetOrderNodes(int counterpartyId)
			{
				var orderDiscrepanciesNode = GetOrderNodesFromFile();

				var allocations = GetAllocationsFromDatabase(counterpartyId, orderDiscrepanciesNode.Keys.ToList());

				MatchOrdersNodesFromFileWithDatabase(allocations, ref orderDiscrepanciesNode);

				return orderDiscrepanciesNode;
			}

			private IDictionary<int, OrderDiscrepanciesNode> GetOrderNodesFromFile()
			{
				var orderNodes = new Dictionary<int, OrderDiscrepanciesNode>();

				foreach (var rowData in _rowsFromFile)
				{
					if(!IsOrderOrPaymentDataRow(rowData))
					{
						continue;
					}

					if(rowData[1].StartsWith("Продажа"))
					{
						var orderNode = CreateOrderNode(rowData);

						orderNodes.Add(orderNode.OrderId, orderNode);
					}
				}

				return orderNodes;
			}

			private IList<OrderWithAllocation> GetAllocationsFromDatabase(int clientId, IList<int> orderIds)
			{
				var allocations = _orderRepository.GetOrdersWithAllocationsOnDay(_unitOfWork, orderIds);
				var ordersMissingFromDocument = _orderRepository.GetOrdersWithAllocationsOnDay2(_unitOfWork, clientId, orderIds);

				return allocations.Concat(ordersMissingFromDocument).ToList();
			}

			private void MatchOrdersNodesFromFileWithDatabase(
				IList<OrderWithAllocation> allocations,
				ref IDictionary<int, OrderDiscrepanciesNode> orderDiscrepanciesNode)
			{
				foreach(var allocation in allocations)
				{
					if(orderDiscrepanciesNode.TryGetValue(allocation.OrderId, out var node))
					{
						node.ProgramOrderSum = allocation.OrderSum;
						node.AllocatedSum = allocation.OrderAllocation;
						node.OrderDeliveryDateInDatabase = allocation.OrderDeliveryDate;
						node.OrderStatus = allocation.OrderStatus;
						node.OrderPaymentStatus = allocation.OrderPaymentStatus;
						node.IsMissingFromDocument = allocation.IsMissingFromDocument;
					}
					else
					{
						orderDiscrepanciesNode.Add(
							allocation.OrderId,
							new OrderDiscrepanciesNode
							{
								OrderId = allocation.OrderId,
								AllocatedSum = allocation.OrderAllocation,
								ProgramOrderSum = allocation.OrderSum,
								OrderDeliveryDateInDatabase = allocation.OrderDeliveryDate,
								OrderStatus = allocation.OrderStatus,
								OrderPaymentStatus = allocation.OrderPaymentStatus,
								IsMissingFromDocument = allocation.IsMissingFromDocument
							});
					}
				}
			}
			#endregion GetOrderNodes

			#region GetGetPaymentNodes
			private IDictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode> GetPaymentNodes(string counterpartyInn, int counterpartyId)
			{
				var paymentDiscrepanciesNodes = GetPaymentDiscrepanciesNodesFromFile();
				var paymentNums = paymentDiscrepanciesNodes.Keys.Select(k => k.PaymentNum).ToList();

				var paymentFromDatabase = GetPaymentsByNumbersFromDatabase(paymentNums, counterpartyInn, counterpartyId);

				MatchPaymentsFromFileWithDatabase(paymentFromDatabase, ref paymentDiscrepanciesNodes);

				return paymentDiscrepanciesNodes;
			}

			private IDictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode> GetPaymentDiscrepanciesNodesFromFile()
			{
				var paymentNodes = new Dictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode>();

				foreach(var rowData in _rowsFromFile)
				{
					if(!IsOrderOrPaymentDataRow(rowData))
					{
						continue;
					}

					if(rowData[1].StartsWith("Оплата"))
					{
						var paymentNode = CreatePaymentNode(rowData);

						paymentNodes.Add((paymentNode.PaymentNum, paymentNode.PaymentDate), paymentNode);
					}
				}

				return paymentNodes;
			}

			private IList<PaymentNode> GetPaymentsByNumbersFromDatabase(
				IList<int> paymentNums,
				string counterpartyInn,
				int counterpartyId)
			{
				var payments = _paymentsRepository
					.GetCounterpartyPaymentNodes(_unitOfWork, counterpartyId, counterpartyInn)
					.ToList();

				return payments;
			}

			private void MatchPaymentsFromFileWithDatabase(
				IList<PaymentNode> paymentNodesFromDatabase,
				ref IDictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode> paymentDiscrepanciesNodes)
			{
				foreach(var paymentNode in paymentNodesFromDatabase)
				{
					if(paymentDiscrepanciesNodes.TryGetValue((paymentNode.PaymentNum, paymentNode.PaymentDate), out var node))
					{
						node.ProgramPaymentSum = paymentNode.PaymentSum;
						node.IsManuallyCreated = paymentNode.IsManuallyCreated;
						node.CounterpartyId = paymentNode.CounterpartyId;
						node.CounterpartyName = paymentNode.CounterpartyName;
						node.CounterpartyInn = paymentNode.CounterpartyInn;
						node.PaymentPurpose = paymentNode.PaymentPurpose;
					}
					else
					{
						paymentDiscrepanciesNodes.Add(
							(paymentNode.PaymentNum, paymentNode.PaymentDate),
							new PaymentDiscrepanciesNode
							{
								PaymentNum = paymentNode.PaymentNum,
								PaymentDate = paymentNode.PaymentDate,
								ProgramPaymentSum = paymentNode.PaymentSum,
								IsManuallyCreated = paymentNode.IsManuallyCreated,
								CounterpartyId = paymentNode.CounterpartyId,
								CounterpartyName = paymentNode.CounterpartyName,
								CounterpartyInn = paymentNode.CounterpartyInn,
								PaymentPurpose = paymentNode.PaymentPurpose
							});
					}
				}
			}

			#endregion GetGetPaymentNodes OrdersTotalSumInDatabase

			private decimal GetCounterpartyPaymentsTotalSum(int counterpartyId, string counterpartyInn)
			{
				var sum = _paymentsRepository
					.GetCounterpartyPaymentsSums(_unitOfWork, counterpartyId, counterpartyInn)
					.ToList().Sum();

				return sum;
			}

			private decimal GetCounterpartyOrdersTotalSum(int counterpartyId)
			{
				var sum = _counterpartyRepository
					.GetCounterpartyOrdersActuaSums(_unitOfWork, counterpartyId, _availableOrderStatuses, false)
					.ToList().Sum();

				return sum;

			}

			private decimal GetCounterpartyTotalDebt(int counterpartyId)
			{
				var sum = _counterpartyRepository
					.GetCounterpartyOrdersActuaSums(_unitOfWork, counterpartyId, _availableOrderStatuses, true)
					.ToList().Sum();

				return sum;
			}

			private decimal GetCounterpartyOldDebt(int counterpartyId)
			{
				var sum = _counterpartyRepository
					.GetCounterpartyOrdersActuaSums(_unitOfWork, counterpartyId, _availableOrderStatuses, true, OldOrdersMaxDate)
					.ToList().Sum();

				return sum;
			}

			private decimal GetCounterpartyOrdersTotalSumFromFile()
			{
				var sum = default(decimal);

				foreach(var rowData in _rowsFromFile)
				{
					if(rowData.Count < 3)
					{
						continue;
					}

					if(rowData[0].StartsWith("Обороты за период"))
					{
						sum = decimal.Parse(rowData[1]);
					}
				}

				return sum;
			}

			private decimal GetCounterpartyPaymentsTotalSumFromFile()
			{
				var sum = default(decimal);

				foreach(var rowData in _rowsFromFile)
				{
					if(rowData.Count < 3)
					{
						continue;
					}

					if(rowData[0].StartsWith("Обороты за период"))
					{
						sum = decimal.Parse(rowData[2]);
					}
				}

				return sum;
			}

			private decimal GetCounterpartyOldDebtFromFile()
			{
				var debt = default(decimal);

				foreach(var rowData in _rowsFromFile)
				{
					if(!IsOrderOrPaymentDataRow(rowData))
					{
						continue;
					}

					if(rowData[1].StartsWith("Продажа"))
					{
						debt += decimal.Parse(rowData[2]);
					}

					if(rowData[1].StartsWith("Оплата"))
					{
						debt -= decimal.Parse(rowData[2]);
					}
				}

				return debt;
			}

			private bool IsOrderOrPaymentDataRow(IList<string> rowData)
			{
				if(rowData.Count < 3)
				{
					return false;
				}

				DateTime.TryParse(rowData[0], out var date);

				if(date == default)
				{
					return false;
				}

				return true;
			}

			private OrderDiscrepanciesNode CreateOrderNode(IEnumerable<string> rowData)
			{
				var data = rowData.ToArray();

				var node = new OrderDiscrepanciesNode
				{
					OrderId = XlsParseHelper.ParseNumberFromString(data[1]),
					OrderDeliveryDateInDocument = XlsParseHelper.ParseDateFromString(data[1]),
					DocumentOrderSum = decimal.Parse(data[2])
				};

				return node;
			}

			private PaymentDiscrepanciesNode CreatePaymentNode(IEnumerable<string> rowData)
			{
				var data = rowData.ToArray();

				var node = new PaymentDiscrepanciesNode
				{
					PaymentNum = XlsParseHelper.ParseNumberFromString(data[1]),
					PaymentDate = XlsParseHelper.ParseDateFromString(data[1]),
					DocumentPaymentSum = decimal.Parse(data[2])
				};

				return node;
			}

			public class OrderDiscrepanciesNode
			{
				public int OrderId { get; set; }
				public DateTime? OrderDeliveryDateInDatabase { get; set; }
				public DateTime? OrderDeliveryDateInDocument { get; set; }
				public OrderStatus? OrderStatus { get; set; }
				public OrderPaymentStatus? OrderPaymentStatus { get; set; }
				public decimal DocumentOrderSum { get; set; }
				public decimal ProgramOrderSum { get; set; }
				public decimal AllocatedSum { get; set; }
				public bool IsMissingFromDocument { get; set; }
				public bool OrderSumDiscrepancy => ProgramOrderSum != DocumentOrderSum;
				public DateTime? OrderDeliveryDate => OrderDeliveryDateInDatabase ?? OrderDeliveryDateInDocument;
			}

			public class PaymentDiscrepanciesNode
			{
				public int PaymentNum { get; set; }
				public DateTime PaymentDate { get; set; }
				public decimal DocumentPaymentSum { get; set; }
				public decimal ProgramPaymentSum { get; set; }
				public int CounterpartyId { get; set; }
				public string CounterpartyName { get; set; }
				public string CounterpartyInn { get; set; }
				public bool IsManuallyCreated { get; set; }
				public string PaymentPurpose { get; set; }
			}
		}
	}
}
