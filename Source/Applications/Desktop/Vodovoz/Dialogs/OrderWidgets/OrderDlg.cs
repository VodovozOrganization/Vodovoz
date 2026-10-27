OrderDlg.cs 6617L cognitive
// C:\Users\Kostya\source\repos\Vodovoz\Source\Applications\Desktop\Vodovoz\Dialogs\OrderWidgets\OrderDlg.cs
§ function function (L762-L1311)
		private void ConfigureDlg()
		{
			SetPermissions();
			
			_justCreated = UoWGeneric.IsNew;

			if(_currentEmployee == null)
			{
				_currentEmployee = _employeeService.GetEmployeeForUser(UoW, _userRepository.GetCurrentUser(UoW).Id);
			}

			_previousDeliveryDate = DeliveryDate;

			_routeListAddressKeepingDocumentController = new RouteListAddressKeepingDocumentController(_employeeRepository, _nomenclatureRepository);

			enumDiscountUnit.SetEnumItems((DiscountUnits[])Enum.GetValues(typeof(DiscountUnits)));

			_orderSettings = ScopeProvider.Scope.Resolve<IOrderSettings>();
			_dailyNumberController = new OrderDailyNumberController(_orderRepository, ServicesConfig.UnitOfWorkFactory);

			NotifyConfiguration.Instance.BatchSubscribeOnEntity<NomenclatureFixedPrice>(OnNomenclatureFixedPriceChanged);
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<DeliveryPoint, Phone>(OnDeliveryPointChanged);
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<Counterparty, Phone>(OnCounterpartyChanged);
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<LogisticsRequirements>(OnLogisticsRequirementsChanged);

			ConfigureTrees();
			ConfigureAcceptButtons();
			ConfigureButtonActions();
			ConfigureSendDocumentByEmailWidget();

			btnCancel.Clicked += (sender, args) => OnCloseTab(CanEditByPermission, CloseSource.Cancel);

			spinDiscount.Adjustment.Upper = 100;
			_commentManager = Entity.CommentManager ?? string.Empty;

			if(Entity.PreviousOrder != null)
			{
				labelPreviousOrder.Text = "Посмотреть предыдущий заказ";
			}
			else
			{
				labelPreviousOrder.Visible = false;
			}

			hboxStatusButtons.Visible = _orderRepository
				.GetStatusesForOrderCancelation()
				.Contains(Entity.OrderStatus)
				|| Entity.OrderStatus == OrderStatus.Canceled
				|| Entity.OrderStatus == OrderStatus.Closed
				|| Entity.SelfDelivery && Entity.OrderStatus == OrderStatus.OnLoading;

			orderEquipmentItemsView.Configure(UoWGeneric, Entity, _flyerRepository, _saleHandler);
			orderEquipmentItemsView.OnDeleteEquipment += OrderEquipmentItemsView_OnDeleteEquipment;

			//Подписывемся на изменения листов для засеривания клиента
			Entity.ObservableOrderDocuments.ListChanged += ObservableOrderDocuments_ListChanged;
			Entity.ObservableOrderDocuments.ElementRemoved += ObservableOrderDocuments_ElementRemoved;
			Entity.ObservableOrderDocuments.ElementAdded += ObservableOrderDocuments_ElementAdded;
			Entity.ObservableOrderDocuments.ElementAdded += Entity_UpdateClientCanChange;
			Entity.ObservableFinalOrderService.ElementAdded += Entity_UpdateClientCanChange;
			Entity.ObservableInitialOrderService.ElementAdded += Entity_UpdateClientCanChange;

			Entity.ObservableOrderItems.ElementAdded += Entity_ObservableOrderItems_ElementAdded;
			Entity.ObservableOrderItems.ElementRemoved += ObservableOrderItems_ElementRemoved;

			//Подписывемся на изменения листа для сокрытия колонки промонаборов
			Entity.ObservablePromotionalSets.ListChanged += ObservablePromotionalSets_ListChanged;
			Entity.ObservablePromotionalSets.ElementAdded += ObservablePromotionalSets_ElementAdded;
			Entity.ObservablePromotionalSets.ElementRemoved += ObservablePromotionalSets_ElementRemoved;

			//Подписываемся на изменение товара, для обновления количества оборудования в доп. соглашении
			Entity.ObservableOrderItems.ElementChanged += ObservableOrderItems_ElementChanged_ChangeCount;
			Entity.ObservableOrderEquipments.ElementChanged += ObservableOrderEquipments_ElementChanged_ChangeCount;

			Entity.ObservableOrderDepositItems.ElementAdded += ObservableOrderDepositItemsOnElementAdded;
			Entity.ObservableOrderDepositItems.ElementRemoved += ObservableOrderDepositItemsOnElementRemoved;

			Entity.ObservableOrderEquipments.ElementAdded += ObservableOrderEquipmentsOnElementAdded;
			Entity.ObservableOrderEquipments.ElementRemoved += ObservableOrderEquipmentsOnElementRemoved;

			enumSignatureType.ItemsEnum = typeof(OrderSignatureType);

			enumSignatureType.Binding.AddBinding(Entity, s => s.SignatureType, w => w.SelectedItemOrNull).InitializeFromSource();

			labelCreationDateValue.Binding
				.AddFuncBinding(Entity, s => s.CreateDate.HasValue ? s.CreateDate.Value.ToString("dd.MM.yyyy HH:mm") : "", w => w.LabelProp)
				.InitializeFromSource();

			ylabelOrderStatus.Binding
				.AddFuncBinding(Entity, e => e.OrderStatus.GetEnumTitle(), w => w.LabelProp)
				.InitializeFromSource();
			ylabelNumber.Binding
				.AddFuncBinding(Entity, e => e.Code1c + (e.DailyNumber.HasValue ? $" ({e.DailyNumber})" : ""), w => w.LabelProp)
				.InitializeFromSource();

			enumDocumentType.ItemsEnum = typeof(DefaultDocumentType);
			enumDocumentType.Binding.AddBinding(Entity, s => s.DocumentType, w => w.SelectedItem).InitializeFromSource();

			chkContractCloser.Binding.AddBinding(Entity, c => c.IsContractCloser, w => w.Active).InitializeFromSource();

			chkCommentForDriver.Binding.AddBinding(Entity, c => c.HasCommentForDriver, w => w.Active).InitializeFromSource();

			speciallistcomboboxCallBeforeArrivalMinutes.ShowSpecialStateNot = true;
			speciallistcomboboxCallBeforeArrivalMinutes.NameForSpecialStateNot = "Не нужен";

			speciallistcomboboxCallBeforeArrivalMinutes.ItemsList = new int?[] { null, 15, 30, 60 };

			speciallistcomboboxCallBeforeArrivalMinutes.Binding
				.AddBinding(Entity, e => e.CallBeforeArrivalMinutes, w => w.SelectedItem)
				.InitializeFromSource();

			if(UoWGeneric.IsNew
				|| (Entity.CallBeforeArrivalMinutes is null && Entity.IsDoNotMakeCallBeforeArrival != true))
			{
				//Для новых и кривых заказов добавляем и выставляем пустое значение, чтобы пользователь вручную выбрал нужный вариант
				speciallistcomboboxCallBeforeArrivalMinutes.SelectedItem = _defaultCallBeforeArrival;
			}

			speciallistcomboboxCallBeforeArrivalMinutes.ItemSelected += (s, e) =>
			{
				Entity.IsDoNotMakeCallBeforeArrival = !Entity.CallBeforeArrivalMinutes.HasValue;
			};

			specialListCmbOurOrganization.ItemsList = UoW.GetAll<Organization>();
			specialListCmbOurOrganization.Binding.AddBinding(Entity, o => o.OurOrganization, w => w.SelectedItem).InitializeFromSource();
			specialListCmbOurOrganization.Sensitive = _canSetOurOrganization;
			specialListCmbOurOrganization.ItemSelected += OnOurOrganisationsItemSelected;

			pickerDeliveryDate.Binding
				.AddBinding(this, dlg => dlg.DeliveryDate, w => w.DateOrNull)
				.InitializeFromSource();

			pickerDeliveryDate.DateChanged += PickerDeliveryDate_DateChanged;
			pickerDeliveryDate.DateChangedByUser += OnPickerDeliveryDateDateChangedByUser;

			pickerBillDate.Visible = labelBillDate.Visible = PaymentType == PaymentType.Cashless;
			pickerBillDate.Binding.AddBinding(Entity, s => s.BillDate, w => w.DateOrNull).InitializeFromSource();

			textComments.Binding.AddBinding(Entity, s => s.Comment, w => w.Buffer.Text).InitializeFromSource();

			checkSelfDelivery.Binding.AddBinding(Entity, s => s.SelfDelivery, w => w.Active).InitializeFromSource();
			checkPayAfterLoad.Binding.AddBinding(Entity, s => s.PayAfterShipment, w => w.Active).InitializeFromSource();
			checkDelivered.Binding.AddBinding(Entity, s => s.Shipped, w => w.Active).InitializeFromSource();
			ylabelloadAllowed.Binding
				.AddFuncBinding(Entity, s => s.LoadAllowedBy != null ? s.LoadAllowedBy.ShortName : string.Empty, w => w.Text)
				.InitializeFromSource();
			entryBottlesToReturn.ValidationMode = ValidationType.numeric;
			entryBottlesToReturn.Binding.AddBinding(Entity, e => e.BottlesReturn, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			entryBottlesToReturn.Changed += OnEntryBottlesToReturnChanged;

			ylabelGeoGroup.Binding
				.AddBinding(Entity, e => e.SelfDelivery, w => w.Visible)
				.InitializeFromSource();

			specialListCmbSelfDeliveryGeoGroup.ItemsList = GetSelfDeliveryGeoGroups();
			specialListCmbSelfDeliveryGeoGroup.Binding
				.AddSource(Entity)
				.AddBinding(e => e.SelfDeliveryGeoGroup, w => w.SelectedItem)
				.AddBinding(e => e.SelfDelivery, w => w.Visible)
				.InitializeFromSource();

			yChkActionBottle.Binding.AddBinding(Entity, e => e.IsBottleStock, w => w.Active).InitializeFromSource();
			yEntTareActBtlFromClient.ValidationMode = ValidationType.numeric;
			yEntTareActBtlFromClient.Binding.AddBinding(Entity, e => e.BottlesByStockCount, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();
			yEntTareActBtlFromClient.Changed += OnYEntTareActBtlFromClientChanged;

			if(Entity.OrderStatus == OrderStatus.Closed)
			{
				entryTareReturned.Text = ScopeProvider.Scope.Resolve<IBottlesRepository>().GetEmptyBottlesFromClientByOrder(UoW, _nomenclatureRepository, Entity)
					.ToString();
				entryTareReturned.Visible = lblTareReturned.Visible = true;
			}

			entryTrifle.ValidationMode = ValidationType.numeric;
			entryTrifle.Binding.AddBinding(Entity, e => e.Trifle, w => w.Text, new NullableIntToStringConverter()).InitializeFromSource();

			ylabelContract.Binding.AddFuncBinding(Entity,
				e => e.Contract != null && e.Contract.Organization != null
					? e.Contract.Title + " (" + e.Contract.Organization.FullName + ")"
					: string.Empty, w => w.Text).InitializeFromSource();

			OldFieldsConfigure();

			entOnlineOrder.ValidationMode = ValidationType.numeric;
			entOnlineOrder.Binding.AddBinding(Entity, e => e.OnlinePaymentNumber, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			var excludedPaymentFromIds = new[]
			{
				_orderSettings.PaymentByCardFromSmsId,
				_orderSettings.GetPaymentByCardFromAvangardId,
				_orderSettings.GetPaymentByCardFromFastPaymentServiceId,
				_orderSettings.PaymentByCardFromOnlineStoreId
			};

			var paymentFromItemsQuery = UoW.Session.QueryOver<PaymentFrom>();
			if(PaymentByCardFrom == null || !excludedPaymentFromIds.Contains(PaymentByCardFrom.Id))
			{
				paymentFromItemsQuery.WhereRestrictionOn(x => x.Id).Not.IsIn(excludedPaymentFromIds);
			}

			if(PaymentByCardFrom == null || !PaymentByCardFrom.IsArchive)
			{
				paymentFromItemsQuery.Where(p => !p.IsArchive);
			}

			ySpecPaymentFrom.ItemsList = paymentFromItemsQuery.List();
			ySpecPaymentFrom.Binding
				.AddBinding(this, e => e.PaymentByCardFrom, w => w.SelectedItem)
				.InitializeFromSource();

			yenumcomboboxTerminalSubtype.ItemsEnum = typeof(PaymentByTerminalSource);
			yenumcomboboxTerminalSubtype.Sensitive = false;
			yenumcomboboxTerminalSubtype.Binding
				.AddSource(Entity)
				.AddBinding(s => s.PaymentByTerminalSource, w => w.SelectedItemOrNull)
				.AddFuncBinding(s => s.PaymentType == PaymentType.Terminal, w => w.Visible)
				.InitializeFromSource();

			enumTax.ItemsEnum = typeof(TaxType);
			Enum[] hideTaxTypeEnums = { TaxType.None };
			enumTax.AddEnumToHideList(hideTaxTypeEnums);
			enumTax.ChangedByUser += (sender, args) => { Counterparty.TaxType = (TaxType)enumTax.SelectedItem; };

			entityselectionDeliverySchedule.ViewModel = CreateEntityselectionDeliveryScheduleViewModel();
			entityselectionDeliverySchedule.ViewModel.Changed += (s, e) => UpdateClientSecondOrderDiscount();

			var counterpartyFilter = _lifetimeScope.Resolve<CounterpartyJournalFilterViewModel>();

			entityVMEntryClient.SetEntityAutocompleteSelectorFactory(
				new EntityAutocompleteSelectorFactory<CounterpartyJournalViewModel>(typeof(Counterparty),
					() => new CounterpartyJournalViewModel(counterpartyFilter, ServicesConfig.UnitOfWorkFactory,
						ServicesConfig.CommonServices, Startup.MainWin.NavigationManager, filter =>
						{
							filter.IsForRetail = IsForRetail;
							filter.IsForSalesDepartment = IsForSalesDepartment;
							filter.RestrictIncludeArchive = false;
						}))
			);
			entityVMEntryClient.Binding.AddBinding(this, dlg => dlg.Counterparty, w => w.Subject).InitializeFromSource();
			entityVMEntryClient.CanEditReference = true;
			entityVMEntryClient.BeforeChangeByUser += OnClientBeforeChangeByUser;

			evmeContactPhone.SetObjectDisplayFunc<Phone>((phone) => phone.ToString());
			evmeContactPhone.Binding.AddSource(Entity)
				.AddBinding(e => e.ContactPhone, w => w.Subject)
				.AddFuncBinding(e => e.Client != null, w => w.Sensitive)
				.InitializeFromSource();

			var roboatsSettings = _lifetimeScope.Resolve<IRoboatsSettings>();
			var roboatsFileStorageFactory = new RoboatsFileStorageFactory(roboatsSettings, ServicesConfig.CommonServices.InteractiveService, ErrorReporter.Instance);
			var deliveryScheduleRepository = ScopeProvider.Scope.Resolve<IDeliveryScheduleRepository>();
			var fileDialogService = new FileDialogService();
			var _roboatsViewModelFactory = new RoboatsViewModelFactory(roboatsFileStorageFactory, fileDialogService,
				ServicesConfig.CommonServices.CurrentPermissionService);

			ybuttonFastDeliveryCheck.Clicked += OnButtonFastDeliveryCheckClicked;

			ycheckFastDelivery.Binding.AddBinding(Entity, e => e.IsFastDelivery, w => w.Active).InitializeFromSource();
			ycheckFastDelivery.Toggled += OnCheckFastDeliveryToggled;

			chkDontArriveBeforeInterval.Binding
				.AddBinding(Entity, e => e.DontArriveBeforeInterval, w => w.Active)
				.InitializeFromSource();

			evmeAuthor.Binding.AddBinding(Entity, s => s.Author, w => w.Subject).InitializeFromSource();
			evmeAuthor.Sensitive = false;

			entryDeliveryPoint.ViewModel = new LegacyEEVMBuilderFactory<OrderDlg>(this, this, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(dlg => dlg.DeliveryPoint)
				.UseViewModelJournalAndAutocompleter<DeliveryPointByClientJournalViewModel, DeliveryPointJournalFilterViewModel>(filter =>
				{
					filter.Counterparty = Counterparty;
				})
				.UseViewModelDialog<DeliveryPointViewModel>()
				.Finish();

			entryDeliveryPoint.ViewModel.Changed += OnReferenceDeliveryPointChanged;
			entryDeliveryPoint.ViewModel.ChangedByUser += OnReferenceDeliveryPointChangedByUser;

			_phonesJournal = ScopeProvider.Scope.Resolve<PhonesJournalViewModel>();

			var phoneSelectoFactory = new EntityAutocompleteSelectorFactory<PhonesJournalViewModel>(typeof(Phone),
				() => _phonesJournal);
			evmeContactPhone.SetEntitySelectorFactory(phoneSelectoFactory);

			_phonesJournal.FilterViewModel.Counterparty = Counterparty;
			_phonesJournal.FilterViewModel.DeliveryPoint = DeliveryPoint;

			entryDeliveryPoint.ViewModel.ChangedByUser += (s, e) =>
			{
				if(Entity?.DeliveryPoint == null)
				{
					return;
				}

				if(!DeliveryPoint.IsActive
					&& !MessageDialogHelper.RunQuestionDialog("Данный адрес деактивирован, вы уверены, что хотите выбрать его?")
				)
				{
					Entity.UpdateDeliveryPoint(null, _orderContractUpdater);
				}
				UpdateOrderItemsPrices();
			};

			chkContractCloser.Sensitive = _canSetContractCloser;

			buttonViewDocument.Sensitive = false;
			btnDeleteOrderItem.Sensitive = false;
			ntbOrderEdit.ShowTabs = false;
			ntbOrderEdit.Page = 0;
			ntbOrder.ShowTabs = false;
			ntbOrder.Page = 0;

			buttonSelectPaymentType.Clicked += OnSelectPaymentTypeClicked;
			buttonSelectPaymentType.Sensitive = true;

			yentryPaymentType.Binding
				.AddFuncBinding(Entity, e => e.PaymentType.GetEnumTitle(), w => w.Text)
				.InitializeFromSource();

			SetSensitivityOfPaymentType();

			enumAddRentButton.ItemsEnum = typeof(RentType);
			enumAddRentButton.EnumItemClicked += (sender, e) => AddRent((RentType)e.ItemEnum);

			checkSelfDelivery.Toggled += (sender, e) =>
			{
				if(checkSelfDelivery.Active)
				{
					if(!_selectPaymentTypeViewModel.ExcludedPaymentTypes.Contains(PaymentType.DriverApplicationQR))
					{
						_selectPaymentTypeViewModel.AddExcludedPaymentTypes(PaymentType.DriverApplicationQR);
					}

					if(PaymentType == PaymentType.DriverApplicationQR)
					{
						if(Counterparty?.PaymentMethod != PaymentType.DriverApplicationQR)
						{
							Entity.UpdatePaymentType(Counterparty.PaymentMethod, _orderContractUpdater);
							ApplyWorkingWithoutSealSignatureType();
						}
						else
						{
							MessageDialogHelper.RunWarningDialog("Не возможно определить тип оплаты автоматически", "Тип оплаты был сброшен!");
							Entity.UpdatePaymentType(PaymentType.Cash, _orderContractUpdater);
						}
					}
				}
				else
				{
					_selectPaymentTypeViewModel.RemoveExcludedPaymentTypes(PaymentType.DriverApplicationQR);

					Entity.SelfDeliveryGeoGroup = null;
					specialListCmbSelfDeliveryGeoGroup.ShowSpecialStateNot = true;
				}

				SetDeliveryScheduleSelectionEditable();

				ybuttonFastDeliveryCheck.Sensitive =
					ycheckFastDelivery.Sensitive = !checkSelfDelivery.Active && Entity.CanChangeFastDelivery;
				lblDeliveryPoint.Sensitive = entryDeliveryPoint.Sensitive = !checkSelfDelivery.Active;
				buttonAddMaster.Sensitive = !checkSelfDelivery.Active;

				UpdateClientDefaultParam();

				if((DeliveryPoint != null || Entity.SelfDelivery) && Entity.OrderStatus == OrderStatus.NewOrder)
				{
					var updateDeliveryResult = UpdateDelivery(false);

					if(updateDeliveryResult.IsFailure)
					{
						throw new AbortCreatingPageException(updateDeliveryResult.Errors.First().Message, "Ошибка");
					}
				}

				UpdateOrderAddressTypeUI();
				_orderContractUpdater.UpdateOrCreateContract(UoW, Entity);
				UpdateOrderItemsPrices();
				SetLogisticsRequirementsCheckboxes();

				speciallistcomboboxCallBeforeArrivalMinutes.SelectedItem = null;
			};

			dataSumDifferenceReason.Binding.AddBinding(Entity, s => s.SumDifferenceReason, w => w.Text).InitializeFromSource();

			spinSumDifference.Binding.AddBinding(Entity, e => e.ExtraMoney, w => w.ValueAsDecimal).InitializeFromSource();

			labelSum.Binding.AddFuncBinding(Entity, e => CurrencyWorks.GetShortCurrencyString(e.OrderSum), w => w.LabelProp)
				.InitializeFromSource();
			labelCashToReceive.Binding.AddFuncBinding(Entity, e => CurrencyWorks.GetShortCurrencyString(e.OrderCashSum), w => w.LabelProp)
				.InitializeFromSource();

			buttonCopyManagerComment.Clicked += OnButtonCopyManagerCommentClicked;
			textManagerComments.Binding.AddBinding(Entity, e => e.CommentManager, w => w.Buffer.Text).InitializeFromSource();
			lastComment.Binding
				.AddBinding(Entity, e => e.LastOPCommentUpdate, w => w.Text)
				.InitializeFromSource();

			textDriverCommentFromMobile.Binding.AddBinding(Entity, e => e.DriverMobileAppComment, w => w.Buffer.Text)
				.InitializeFromSource();

			enumDiverCallType.ItemsEnum = typeof(DriverCallType);
			enumDiverCallType.Binding.AddBinding(Entity, e => e.DriverCallType, w => w.SelectedItem).InitializeFromSource();
			driverCallId.Binding.AddFuncBinding(Entity, e => e.DriverCallId == null ? "" : e.DriverCallId.ToString(), w => w.LabelProp)
				.InitializeFromSource();

			ySpecCmbNonReturnReason.ItemsList = UoW.Session.QueryOver<NonReturnReason>().List();
			ySpecCmbNonReturnReason.Binding.AddBinding(Entity, e => e.TareNonReturnReason, w => w.SelectedItem).InitializeFromSource();
			ySpecCmbNonReturnReason.ItemSelected += (sender, e) => Entity.IsTareNonReturnReasonChangedByUser = true;
			ySpecCmbNonReturnReason.Sensitive = CanEditByPermission;

			if(DeliveryPoint == null && !string.IsNullOrWhiteSpace(Entity.Address1c))
			{
				var deliveryPoint = Counterparty.DeliveryPoints.FirstOrDefault(d => d.Address1c == Entity.Address1c);
				Entity.UpdateDeliveryPoint(deliveryPoint, _orderContractUpdater);
			}

			_orderItemEquipmentCountHasChanges = false;
			ShowOrderColumnInDocumentsList();

			SetSensitivityOfPaymentType();
			depositrefunditemsview.Configure(UoWGeneric, Entity);
			ycomboboxReason.SetRenderTextFunc<DiscountReasonBase>(x => x.Name);
			ycomboboxReason.ItemsList = _discountReasons;
			ycomboboxReason.ItemSelected += OnYComboBoxReasonItemSelected;

			yCmbReturnTareReasonCategories.SetRenderTextFunc<ReturnTareReasonCategory>(x => x.Name);
			yCmbReturnTareReasonCategories.ItemsList = UoW.Session.QueryOver<ReturnTareReasonCategory>().List();
			yCmbReturnTareReasonCategories.Binding.AddBinding(Entity, e => e.ReturnTareReasonCategory, w => w.SelectedItem)
				.InitializeFromSource();
			yCmbReturnTareReasonCategories.Changed += YCmbReturnTareReasonCategoriesOnChanged;
			HboxReturnTareReasonCategoriesShow();

			yCmbReturnTareReasons.SetRenderTextFunc<ReturnTareReason>(x => x.Name);

			if(Entity.ReturnTareReasonCategory != null)
			{
				ChangeHboxReasonsVisibility();
			}

			yCmbReturnTareReasons.Binding.AddBinding(Entity, e => e.ReturnTareReason, w => w.SelectedItem).InitializeFromSource();

			yCmbPromoSets.SetRenderTextFunc<PromotionalSet>(x => x.ShortTitle);
			yCmbPromoSets.ItemSelected += YCmbPromoSets_ItemSelected;

			bool showEshop = Entity.EShopOrder == null;
			labelEShop.Visible = !showEshop;
			yvalidatedentryEShopOrder.ValidationMode = ValidationType.numeric;
			yvalidatedentryEShopOrder.Binding.AddBinding(Entity, c => c.EShopOrder, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			yvalidatedentryEShopOrder.Visible = !showEshop;

			yhboxCounterpartyExternalOrderId.Binding
				.AddFuncBinding(Entity, c => c.Client != null && c.Client.UseSpecialDocFields, w => w.Visible)
				.InitializeFromSource();

			yvalidatedentryCounterpartyExternalOrderId.Binding
				.AddBinding(Entity, o => o.CounterpartyExternalOrderId, w => w.Text)
				.InitializeFromSource();

			chkAddCertificates.Binding.AddBinding(Entity, c => c.AddCertificates, w => w.Active).InitializeFromSource();

			ToggleVisibilityOfDeposits(Entity.ObservableOrderDepositItems.Any());
			SetDiscountEditable();
			SetDiscountUnitEditable();

			spinSumDifference.Hide();
			labelSumDifference.Hide();
			dataSumDifferenceReason.Hide();
			labelSumDifferenceReason.Hide();

			SetOrderItemDiscountReasonsViewModel();

			SetDriverExtensionCallViewModel();

			SetDriverPhoneForCounterpartyCalls();

			UpdateUIState();

			yChkActionBottle.Toggled += (sender, e) =>
			{
				Entity.RecalculateStockBottles(_orderSettings);
				ControlsActionBottleAccessibility();
				ycomboboxReason.Sensitive = !yChkActionBottle.Active;
				SetDiscountUnitEditable();
				SetDiscountEditable();
				UpdateDiscountReasonSensitive();
			};
			ycheckContactlessDelivery.Binding.AddBinding(Entity, e => e.ContactlessDelivery, w => w.Active).InitializeFromSource();

			UpdateOrderAddressTypeUI();

			Entity.InteractiveService = new CastomInteractiveService();

			Entity.PropertyChanged += OnEntityPropertyChanged;

			if(Entity != null && Entity.Id != 0)
			{
				Entity.CheckDocumentExportPermissions();
			}

			ylabelOrderAddressType.Binding
				.AddFuncBinding(Entity, e => "Тип адреса: " + e.OrderAddressType.GetEnumTitle(), w => w.LabelProp)
				.InitializeFromSource();
			var canChangeOrderAddressType =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_change_order_address_type");
			ybuttonToStorageLogicAddressType.Sensitive = canChangeOrderAddressType;

			UpdateAvailableEnumSignatureTypes();

			btnUpdateEdoDocFlowStatus.Clicked += (sender, args) =>
			{
				UpdateEdoDocumentDataNodes();
				CustomizeSendDocumentAgainButton();
			};

			ybuttonSendDocumentAgain.Visible = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_resend_edo_documents");
			ybuttonSendDocumentAgain.Clicked += OnButtonSendDocumentAgainClicked;
			CustomizeSendDocumentAgainButton();

			btnCopyEntityId.Sensitive = Entity.Id > 0;
			btnCopySummaryInfo.Clicked += OnBtnCopySummaryInfoClicked;

			logisticsRequirementsView.ViewModel = new LogisticsRequirementsViewModel(Entity.LogisticsRequirements ?? _orderService.GetLogisticsRequirements(Entity), ServicesConfig.CommonServices);
			UpdateEntityLogisticsRequirements();
			logisticsRequirementsView.ViewModel.Entity.PropertyChanged += OnLogisticsRequirementsSelectionChanged;

			timepickerWaitUntil.Binding.AddBinding(Entity, e => e.WaitUntilTime, w => w.TimeOrNull).InitializeFromSource();
			timepickerWaitUntil.GetDefaultTime = () => DateTime.Now.AddHours(1).TimeOfDay;

			_lastWaitUntilTime = Entity.WaitUntilTime;

			ybuttonSaveWaitUntil.Visible = Entity.Id != 0;

			OnEnumPaymentTypeChanged(null, EventArgs.Empty);
			UpdateCallBeforeArrivalVisibility();
			SetNearestDeliveryDateLoaderFunc();


			UpdateOrderItemsOriginalValues();

			RefreshBottlesDebtNotifier();

			RefreshDebtorDebtNotifier();

			UpdateDocumentsDescription();
		}
// ... 774 lines omitted
§ function function (L2086-L2142)
		private void TryAddFlyers()
		{
			if(Entity.SelfDelivery
				|| (Entity.OrderStatus != OrderStatus.NewOrder && Entity.OrderStatus != OrderStatus.WaitForPayment)
				|| DeliveryPoint?.District == null
				|| !DeliveryDate.HasValue)
			{
				return;
			}

			var geographicGroupId = DeliveryPoint.District.GeographicGroup.Id;
			var activeFlyers = _flyerRepository.GetAllActiveFlyersByDate(UoW, DeliveryDate.Value);

			if(!activeFlyers.Any())
			{
				if(Entity.ObservableOrderEquipments.Any())
				{
					RemoveFlyers();
				}
				return;
			}

			RemoveFlyers();

			var isOrderContainsKulerServiceGoods =
				_orderOrganizationManager.IsOrderContainsKulerServiceGoods(UoW, Entity.OrderItems);

			// в сервисный заказ листовки не добавляем, при этом организация заказа может быть и не КС,
			// например, если заказ уже оплачен онлайн
			if(Entity.Contract?.Organization?.Id == _organizationSettings.KulerServiceOrganizationId
				|| isOrderContainsKulerServiceGoods)
			{
				return;
			}

			foreach(var flyer in activeFlyers)
			{
				if(!_orderRepository.HasFlyersOnStock(UoW, _routeListSettings, flyer.FlyerNomenclature.Id, geographicGroupId))
				{
					continue;
				}

				if(!flyer.IsForFirstOrder)
				{
					Entity.AddFlyerNomenclature(flyer.FlyerNomenclature);
				}
				else
				{
					if((Entity.Id == 0 && Counterparty != null && Counterparty.FirstOrder == null) || Entity.IsFirstOrder)
					{
						Entity.AddFlyerNomenclature(flyer.FlyerNomenclature);
					}
				}
			}
			orderEquipmentItemsView.UpdateActiveFlyersNomenclaturesIds();
			_previousDeliveryDate = DeliveryDate;
		}
// ... 127 lines omitted
§ function function (L2270-L2540)
		private void ConfigureTrees()
		{
			var colorPrimaryText = GdkColors.PrimaryText;
			var colorBlue = GdkColors.InfoText;
			var colorGreen = GdkColors.SuccessText;
			var colorPrimaryBase = GdkColors.PrimaryBase;
			var colorLightYellow = GdkColors.WarningBase;
			var colorLightRed = GdkColors.DangerBase;

			_discountReasons =
				_discountReasonRepository.GetActiveDiscountReasonsFetchReferences(UoW, _canChoosePremiumDiscount);

			_additionalLoadingNomenclatureIds =
				_deliveryRepository.GetAdditionalLoadingNomenclatureIds(UoW);

			_activeFlyersNomenclatureIds =
				_flyerRepository.GetAllActiveFlyersNomenclaturesIdsByDate(UoW, DateTime.Today);

			var isFastDeliveryEnableColumnName = "IsFastDeliveryEnableColumn";

			treeItems.CreateFluentColumnsConfig<OrderItem>()
				.AddColumn("№")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => Entity.OrderItems.IndexOf(node) + 1)
				.AddColumn("Номенклатура")
					.SetTag(nameof(Nomenclature))
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.NomenclatureString)
				.AddColumn(!_orderRepository.GetStatusesForActualCount(Entity).Contains(Entity.OrderStatus) ? "Кол-во" : "Кол-во [Факт]")
					.SetTag("Count")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Count, OnCountEdited)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
					.AddSetter((c, node) =>
					{
						bool result = true;

						if(node.RentType != OrderRentType.None)
						{
							result = false;
						}

						if(node.Nomenclature.Id == _paidDeliveryNomenclatureId)
						{
							result = false;
						}

						if(node.PromoSet != null && !node.PromoSet.CanEditNomenclatureCount)
						{
							result = false;
						}

						if(node.Nomenclature.Id == _fastDeliveryNomenclatureId)
						{
							result = false;
						}

						c.Editable = result;
					}).WidthChars(10)
					.AddTextRenderer(node => node.ActualCount.HasValue
						? string.Format("[{0:" + $"F{(node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)}" + "}]",
							node.ActualCount)
						: string.Empty)
					.AddTextRenderer(node => node.CanShowReturnedCount
						? string.Format("({0:" + $"F{(node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)}" + "})",
							node.ReturnedCount)
						: string.Empty)
					.AddTextRenderer(node => node.Nomenclature.Unit == null ? string.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn("Аренда")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.RentCount).Editing().Digits(0)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.AddSetter((c, node) => c.Visible = node.RentVisible)
					.EditedEvent(OnRentEdited)
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Price).Digits(2).WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0)).Editing(true)
					.AddSetter((c, node) => c.Editable = node.CanEditPrice)
					.EditedEvent(OnSpinPriceEdited)
					.AddSetter((NodeCellRendererSpin<OrderItem> cell, OrderItem node) =>
					{
						if(Entity.OrderStatus == OrderStatus.NewOrder || (Entity.OrderStatus == OrderStatus.WaitForPayment && !Entity.SelfDelivery))//костыль. на Win10 не видна цветная цена, если виджет засерен
						{
							cell.ForegroundGdk = colorPrimaryText;
							if(node.IsFixedPrice && node.PromoSet == null && node.CopiedFromUndelivery == null)
							{
								cell.ForegroundGdk = colorGreen;
							}
							else if(node.IsUserPrice && Nomenclature.GetCategoriesWithEditablePrice().Contains(node.Nomenclature.Category))
							{
								cell.ForegroundGdk = colorBlue;
							}
						}
					})
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("Альтерн.\nцена")
					.AddToggleRenderer(x => x.IsAlternativePrice).Editing(false)
				.AddColumn("В т.ч. НДС")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.IncludeNDS ?? 0))
					.AddSetter((c, n) => c.Visible = PaymentType == PaymentType.Cashless)
				.AddColumn("Сумма")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.ActualSum))
					.AddSetter((c, n) =>
					{
						if(Entity.OrderStatus == OrderStatus.DeliveryCanceled || Entity.OrderStatus == OrderStatus.NotDelivered)
							c.Text = CurrencyWorks.GetShortCurrencyString(n.OriginalSum);
					}
					)
				.AddColumn("Скидка")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.GetDiscount, OnDiscountEdited)
					.AddSetter((c, n) => c.Editable = _canChangeDiscountValue)
					.AddSetter(
						(c, n) => c.Adjustment = n.IsDiscountInMoney
									? new Adjustment(0, 0, (double)(n.Price * n.CurrentCount), 1, 100, 1)
									: new Adjustment(0, 0, 100, 1, 100, 1)
					)
					.AddSetter((c, n) =>
					{
						if(Entity.OrderStatus == OrderStatus.DeliveryCanceled || Entity.OrderStatus == OrderStatus.NotDelivered)
						{
							c.Text = n.ManualChangingOriginalDiscount.ToString();
						}
					})
					.Digits(2)
					.WidthChars(10)
					.AddTextRenderer(n => n.IsDiscountInMoney ? CurrencyWorks.CurrencyShortName : "%", false)
				.AddColumn("Скидка \nв рублях?")
					.AddToggleRenderer(x => x.IsDiscountInMoney)
					.AddSetter((c, n) => c.Activatable = _canChangeDiscountValue)
				.AddColumn("Основание скидки")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DiscountReasonsNames)
					.WrapMode(Pango.WrapMode.Word)
					.WrapWidth(350)
					.AddSetter(
						(c, n) =>
							c.BackgroundGdk = n.Discount > 0 && !n.DiscountReasons.Any() && n.PromoSet == null ? colorLightRed : colorPrimaryBase
					)
					.AddSetter((c, n) =>
						{
							if(n.PromoSet != null && !n.DiscountReasons.Any() && n.Discount > 0)
							{
								c.Text = n.PromoSet.DiscountReasonInfo;
							}
							else if(Entity.OrderStatus == OrderStatus.DeliveryCanceled || Entity.OrderStatus == OrderStatus.NotDelivered)
							{
								c.Text =
									string.IsNullOrWhiteSpace(n.OriginalDiscountReasonsNames)
									? n.DiscountReasonsNames
									: n.OriginalDiscountReasonsNames;
							}
						})
				.AddColumn("Промонаборы").SetTag(nameof(Entity.PromotionalSets))
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.PromoSet == null ? "" : node.PromoSet.Name)
					.WrapMode(Pango.WrapMode.Word)
					.WrapWidth(300)
				.AddColumn("Доступна для ДЗЧ")
					.SetTag(isFastDeliveryEnableColumnName)
					.HeaderAlignment(0.5f)
					.AddToggleRenderer(x => IsOrderItemAvailableToFastDelivery(x))
					.Editing(false)
				.AddColumn("")
				.RowCells()
					.XAlign(0.5f)
				.Finish();
			treeItems.ItemsDataSource = Entity.ObservableOrderItems;
			treeItems.Selection.Mode = SelectionMode.Multiple;
			treeItems.Selection.Changed += TreeItems_Selection_Changed;
			treeItems.ColumnsConfig.GetColumnsByTag(nameof(Entity.PromotionalSets)).FirstOrDefault().Visible = Entity.PromotionalSets.Count > 0;
			
			var isFastDeliveryEnableColumn =
				treeItems.ColumnsConfig.GetColumnsByTag(isFastDeliveryEnableColumnName).FirstOrDefault();

			if(isFastDeliveryEnableColumn != null)
			{
				isFastDeliveryEnableColumn.Sizing = TreeViewColumnSizing.Fixed;
				isFastDeliveryEnableColumn.FixedWidth = 120;
			}

			treeDocuments.ColumnsConfig = ColumnsConfigFactory.Create<OrderDocument>()
				.AddColumn("Документ").AddTextRenderer(node => node.Name)
				.AddColumn("Дата документа").AddTextRenderer(node => node.DocumentDateText)
				.AddColumn("Заказ №").SetTag("OrderNumberColumn").AddTextRenderer(node => node.Order.Id != node.AttachedToOrder.Id ? node.Order.Id.ToString() : "")
				.AddColumn("Без рекламы").AddToggleRenderer(x => x is IAdvertisable && (x as IAdvertisable).WithoutAdvertising)
					.ChangeSetProperty(PropertyUtil.GetPropertyInfo<IAdvertisable>(x => x.WithoutAdvertising))
					.AddSetter((c, n) => c.Visible = n.Type == OrderDocumentType.Invoice || n.Type == OrderDocumentType.InvoiceContractDoc)
					.AddSetter((c, n) => c.Activatable = CanEditByPermission)
				.AddColumn("Без подписей и печати").AddToggleRenderer(x => x is ISignableDocument && (x as ISignableDocument).HideSignature)
				.Editing().ChangeSetProperty(PropertyUtil.GetPropertyInfo<ISignableDocument>(x => x.HideSignature))
				.AddSetter((c, n) => c.Visible = n is ISignableDocument)
				.AddSetter((toggle, document) =>
				{
					if(document.Type == OrderDocumentType.UPD
						|| document.Type == OrderDocumentType.SpecialUPD)
					{
						toggle.Activatable = CanEditByPermission && _canEditSealAndSignatureUpd;
					}
                    if(document.Type == OrderDocumentType.LetterOfDebt)
                    {
                        toggle.Activatable = false;
                    }
					else
					{
						toggle.Activatable = CanEditByPermission;
					}
				}) // Сделать только для  ISignableDocument, UDP и SpecialUPD
				.AddColumn("")
				.RowCells().AddSetter<CellRenderer>((c, n) =>
				{
					c.CellBackgroundGdk = colorPrimaryBase;
					if(n.Order.Id != n.AttachedToOrder.Id && !(c is CellRendererToggle))
					{
						c.CellBackgroundGdk = colorLightYellow;
					}
				})
				.Finish();
			treeDocuments.Selection.Mode = SelectionMode.Multiple;
			treeDocuments.ItemsDataSource = Entity.ObservableOrderDocuments;
			treeDocuments.Selection.Changed += Selection_Changed;

			treeDocuments.RowActivated += OnButtonViewDocumentClicked;

			treeViewEdoContainers.ColumnsConfig = FluentColumnsConfig<EdoDockflowData>.Create()
				.AddColumn("Новый\nдокументооборот")
					.AddToggleRenderer(x => x.IsNewDockflow)
					.Editing(false)
				.AddColumn("Код\nдокументооборота")
					.AddTextRenderer(x => x.DocFlowId.HasValue ? x.DocFlowId.ToString() : string.Empty)
				.AddColumn("Отправленные\nдокументы")
					.AddTextRenderer(x => x.DocumentType)
				.AddColumn("Статус\nдокументооборота")
					.AddTextRenderer(x => x.EdoDocFlowStatusString)
				.AddColumn("Доставлено\nклиенту?")
					.AddToggleRenderer(x => x.IsReceived)
					.Editing(false)
				.AddColumn("Описание ошибки")
					.AddTextRenderer(x => x.ErrorDescription)
					.WrapWidth(500)
				.AddColumn("Статус задачи\nнового документооборота")
					.AddTextRenderer(x => x.EdoTaskStatus.HasValue ? x.EdoTaskStatus.Value.GetEnumTitle() : string.Empty)
				.AddColumn("Статус документа\nнового документооборота")
					.AddTextRenderer(x => x.EdoDocumentStatus.HasValue ? x.EdoDocumentStatus.Value.GetEnumTitle() : string.Empty)
				.AddColumn("")
				.Finish();

			treeViewEdoContainers.Binding
				.AddBinding(this, vm => vm.SelectedEdoDocumentDataNode, w => w.SelectedRow)
				.InitializeFromSource();

			treeViewEdoContainers.ItemsDataSource = _edoEdoDocumentDataNodes;

			treeServiceClaim.ColumnsConfig = ColumnsConfigFactory.Create<ServiceClaim>()
				.AddColumn("Статус заявки").AddTextRenderer(node => node.Status.GetEnumTitle())
				.AddColumn("Номенклатура оборудования").AddTextRenderer(node => node.Nomenclature != null ? node.Nomenclature.Name : "-")
				.AddColumn("Серийный номер").AddTextRenderer(node => node.Equipment != null && node.Equipment.Nomenclature.IsSerial ? node.Equipment.Serial : "-")
				.AddColumn("Причина").AddTextRenderer(node => node.Reason)
				.RowCells().AddSetter<CellRendererText>((c, n) =>
				{
					c.ForegroundGdk = n.RepeatedService ? GdkColors.DangerText : GdkColors.PrimaryText;
				})
				.Finish();

			treeServiceClaim.ItemsDataSource = Entity.ObservableInitialOrderService;
			treeServiceClaim.Selection.Changed += TreeServiceClaim_Selection_Changed;
		}
// ... 492 lines omitted
§ function function (L3033-L3166)
		private Result CheckPossibilityConfirmation()
		{
			if(!Entity.CanSetOrderAsAccepted)
			{
				return Result.Failure(OrderErrors.CantEdit);
			}

			var canContinue = Entity.DefaultWaterCheck(ServicesConfig.InteractiveService);

			if(canContinue.HasValue && !canContinue.Value)
			{
				toggleGoods.Activate();
				return Result.Failure(OrderErrors.Accept.HasNoDefaultWater);
			}

			var validationResult = ValidateAndFormOrder();

			if(validationResult.IsFailure)
			{
				return Result.Failure(validationResult.Errors);
			}

			if(!CheckCertificates(canSaveFromHere: true))
			{
				return Result.Failure(OrderErrors.HasNoValidCertificates);
			}

			var promosetDuplicateFinder = new PromosetDuplicateFinder(_freeLoaderChecker, new CastomInteractiveService());

			var phones = new List<string>();

			phones.AddRange(Entity.Client.Phones.Select(x => x.DigitsNumber));

			if(DeliveryPoint != null)
			{
				phones.AddRange(Entity.DeliveryPoint.Phones.Select(x => x.DigitsNumber));
			}

			var hasPromoSetForNewClients = Entity.PromotionalSets.Any(x => x.PromotionalSetForNewClients);
			var hasOtherFirstRealOrder = _orderRepository.HasCounterpartyOtherFirstRealOrder(UoW, Counterparty, Entity.Id);

			if(hasPromoSetForNewClients && hasOtherFirstRealOrder)
			{
				if(!MessageDialogHelper.RunQuestionDialog(
					"В заказ добавлен промонабор для новых клиентов, но это не первый заказ клиента\n" +
					"Хотите продолжить сохранение?"))
				{
					return Result.Failure(OrderErrors.AcceptAbortedByUser);
				}
			}

			if(hasPromoSetForNewClients && Entity.OrderItems.Any(x => x.PromoSet != null))
			{
				if(!promosetDuplicateFinder.RequestDuplicatePromosets(UoW, Entity.Id, DeliveryPoint, phones))
				{
					return Result.Failure(OrderErrors.AcceptAbortedByUser);
				}
			}
			if(hasPromoSetForNewClients && _freeLoaderChecker.CheckFreeLoaderOrderByNaturalClientToOfficeOrStore(
				UoW, Entity.SelfDelivery, Counterparty, DeliveryPoint))
			{
				return Result.Failure(OrderErrors.UnableToShipPromoSet);
			}

			PrepareSendBillInformation();

			if(_emailAddressForBill == null
			   && _emailService.NeedSendBillToEmail(UoW, Entity)
			   && (!Counterparty.NeedSendBillByEdo || CurrentCounterpartyEdoAccount().ConsentForEdoStatus != ConsentForEdoStatus.Agree)
			   && !MessageDialogHelper.RunQuestionDialog("Не найден адрес электронной почты для отправки счетов, продолжить сохранение заказа без отправки почты?"))
			{
				return Result.Failure(OrderErrors.AcceptAbortedByUser);
			}

			var fastDeliveryResult = _fastDeliveryHandler.CheckFastDelivery(UoW, Entity);

			if(fastDeliveryResult.IsFailure)
			{
				if(fastDeliveryResult.Errors.Any(x => x.Code == nameof(FastDeliveryErrors.RouteListForFastDeliveryIsMissing)))
				{
					var fastDeliveryVerificationViewModel =
						new FastDeliveryVerificationViewModel(_fastDeliveryHandler.FastDeliveryAvailabilityHistory);
					NavigationManager.OpenViewModel<FastDeliveryVerificationDetailsViewModel, IUnitOfWork, FastDeliveryVerificationViewModel>(
						null, UoW, fastDeliveryVerificationViewModel);
				}

				return fastDeliveryResult;
			}

			var edoLightsMatrixViewModel = !(Startup.MainWin.InfoPanel.GetWidget(typeof(EdoLightsMatrixPanelView)) is EdoLightsMatrixPanelView edoLightsMatrixPanelView)
				? new EdoLightsMatrixViewModel()
				: edoLightsMatrixPanelView.ViewModel.EdoLightsMatrixViewModel;

			edoLightsMatrixViewModel.RefreshLightsMatrix(CurrentCounterpartyEdoAccount());

			var edoLightsMatrixPaymentType = PaymentType == PaymentType.Cashless
				? EdoLightsMatrixPaymentType.Cashless
				: EdoLightsMatrixPaymentType.Receipt;

			var isAccountableInChestniyZnak = Entity.OrderItems.Any(x => x.Nomenclature.IsAccountableInTrueMark);

			if(isAccountableInChestniyZnak
			   && Entity.DeliveryDate >= new DateTime(2022, 11, 01)
			   && !edoLightsMatrixViewModel.IsPaymentAllowed(Entity.Client, edoLightsMatrixPaymentType)
			   && Counterparty.ReasonForLeaving != ReasonForLeaving.Tender)
			{
				if(ServicesConfig.InteractiveService.Question($"Данному контрагенту запрещено отгружать товары по выбранному типу оплаты\n" +
															  $"Оставить черновик заказа в статусе \"Новый\"?"))
				{
					if(Save())
					{
						return Result.Success();
					}

					return Result.Failure(OrderErrors.Save);
				}

				return Result.Failure(OrderErrors.AcceptAbortedByUser);
			}

			if(PaymentType == PaymentType.Cashless)
			{
				var hasUnknownEdoLightsType = edoLightsMatrixViewModel.HasUnknown();

				if(hasUnknownEdoLightsType
				   && !ServicesConfig.InteractiveService.Question(
					   $"Вы уверены, что клиент не работает с ЭДО и хотите отправить заказ без формирования электронной УПД?\nПродолжить?"))
				{
					return Result.Failure(OrderErrors.AcceptAbortedByUser);
				}
			}

			return Result.Success();
		}
// ... 1655 lines omitted
§ function function (L4822-L5247)
		private void OnUndeliveryViewModelSaved(object sender, UndeliveryOnOrderCloseEventArgs e)
		{
			_orderCancellationService.PrepareReceiptEdoTaskCancellation(
				UoW,
				e.CancellationPermit,
				$"Отмена заказа №{Entity.Id}");

			Entity.SetUndeliveredStatus(
				UoW,
				_routeListService,
				_saleHandler,
				_nomenclatureSettings,
				CallTaskWorker,
				needCreateDeliveryFreeBalanceOperation: true);

			var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(UoW, Entity.Id);
			if(routeListItem != null)
			{
				routeListItem.StatusLastUpdate = DateTime.Now;
				routeListItem.SetOrderActualCountsToZeroOnCanceled(_saleHandler);
				UoW.Save(routeListItem);

				var notificationRequest = new NotificationRouteListChangesRequest
				{
					OrderId = e.UndeliveredOrder.OldOrder.Id,
					PushNotificationDataEventType = PushNotificationDataEventType.RouteListContentChanged
				};

				var result = _routeListChangesNotificationSender.NotifyOfRouteListChanged(notificationRequest).GetAwaiter().GetResult();

				if(!result.IsSuccess)
				{
					var transferTypeErrors = result.Errors
						.Where(x => x.Code == RouteListErrors.RouteListItem.TransferTypeNotSet)
						.Select(x => x.Message)
						.Where(x => !string.IsNullOrWhiteSpace(x))
						.ToList();

					var message = transferTypeErrors.Any()
						? string.Join(", ", transferTypeErrors)
						: string.Join(", ", result.Errors.Select(x => x.Message).Where(x => !string.IsNullOrWhiteSpace(x)));

					if(string.IsNullOrWhiteSpace(message))
					{
						message = "Не удалось отправить уведомление об изменении маршрутного листа.";
					}

					ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning, message);
				}
			}
			else
			{
				Entity.SetActualCountsToZeroOnCanceled(_saleHandler);
			}

			var saved = Save();

			UpdateUIState();

			if(!saved)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					"Не удалось сохранить отмену заказа. Исправьте ошибки и нажмите «Сохранить».");
			}

			var allowCancellation = e.CancellationPermit.Type == OrderCancellationPermitType.AllowCancelOrder;
			var hasEdoTaskToCancellationId = e.CancellationPermit.EdoTaskToCancellationId != null;
			if(saved && allowCancellation && hasEdoTaskToCancellationId)
			{
				_orderCancellationService.AutomaticCancelDocflow(
					UoW, 
					$"Отмена заказа №{Entity.Id}", 
					e.CancellationPermit.EdoTaskToCancellationId.Value
				);
			}

			if(saved && e.NeedClose)
			{
				if(e.UndeliveredOrder.NewOrder != null)
				{
					var customerOrderRescheduledEvent = new CustomerNotificationDomainEvent(
						CustomerNotificationEventType.OrderRescheduled,
						e.UndeliveredOrder.OldOrder.OnlineOrder?.Source,
						e.UndeliveredOrder.OldOrder.OnlineOrder?.Id,
						e.UndeliveredOrder.OldOrder.Id,
						e.UndeliveredOrder.NewOrder.Id,
						e.UndeliveredOrder.UndeliveryDetalization?.Name // Пока будут заполнять //e.UndeliveredOrder.UndeliveryDetalization?.CustomerNotificationText
						);
					_customerNotificationPublisher.TryPublish(UoW, customerOrderRescheduledEvent);

					UoW.Commit();
				}

				OnCloseTab(false);
			}				
		}

		protected void OnEnumPaymentTypeChangedByUser(object sender, EventArgs e)
		{
			UpdateOnlineOrderText();

			if(ycheckFastDelivery.Active
				&& (Entity.IsNeedIndividualSetOnLoad(_counterpartyEdoAccountController) || Entity.IsNeedIndividualSetOnLoadForTender))
			{
				ResetFastDeliveryForNetworkClient();
			}
		}

		private void UpdateOnlineOrderText()
		{
			if(PaymentType != PaymentType.PaidOnline
				&& PaymentType != PaymentType.DriverApplicationQR
				&& PaymentType != PaymentType.SmsQR)
			{
				entOnlineOrder.Text = string.Empty;
			}
		}

		protected void OnButtonWaitForPaymentClicked(object sender, EventArgs e)
		{
			ValidationContext validationContext = new ValidationContext(Entity, null, new Dictionary<object, object> {
				{ "NewStatus", OrderStatus.WaitForPayment }
			});

			if(!Validate(validationContext))
			{
				return;
			}

			PrepareSendBillInformation();

			if(_emailAddressForBill == null
			   && _emailService.NeedSendBillToEmail(UoW, Entity)
			   && (!Counterparty.NeedSendBillByEdo || CurrentCounterpartyEdoAccount().ConsentForEdoStatus != ConsentForEdoStatus.Agree)
			   && !MessageDialogHelper.RunQuestionDialog("Не найден адрес электронной почты для отправки счетов, продолжить сохранение заказа без отправки почты?"))
			{
				return;
			}

			Entity.ChangeStatusAndCreateTasks(_saleHandler, OrderStatus.WaitForPayment, CallTaskWorker);
			UpdateUIState();
		}

		protected void OnEnumDiverCallTypeChanged(object sender, EventArgs e)
		{
			var listDriverCallType = UoW.Session.QueryOver<Order>()
										.Where(x => x.Id == Entity.Id)
										.Select(x => x.DriverCallType).List<DriverCallType>().FirstOrDefault();

			if(listDriverCallType != (DriverCallType)enumDiverCallType.SelectedItem)
			{
				var max = UoW.Session.QueryOver<Order>().Select(Projections.Max<Order>(x => x.DriverCallId)).SingleOrDefault<int>();
				Entity.DriverCallId = max != 0 ? max + 1 : 1;
			}
		}

		protected void OnYEntTareActBtlFromClientChanged(object sender, EventArgs e)
		{
			Entity.CalculateBottlesStockDiscounts(_orderSettings);
		}

		protected void OnEntryTrifleChanged(object sender, EventArgs e)
		{
			if(int.TryParse(entryTrifle.Text, out int result))
			{
				Entity.Trifle = result;
			}
		}

		protected void OnShown(object sender, EventArgs e)
		{
			//Скрывает журнал заказов при открытии заказа, чтобы все элементы умещались на экране
			if(TabParent is TdiSliderTab slider)
			{
				slider.IsHideJournal = true;
			}
		}

		protected void OnButtonDepositsClicked(object sender, EventArgs e)
		{
			ToggleVisibilityOfDeposits();
		}

		protected void OnChkContractCloserToggled(object sender, EventArgs e)
		{
			SetSensitivityOfPaymentType();
		}

		protected void OnSpinDiscountValueChanged(object sender, EventArgs e)
		{
			if(spinDiscount.ValueAsDecimal != 0)
			{
				SetDiscount();
			}
		}

		private void OnYComboBoxReasonItemSelected(object sender, ItemSelectedEventArgs e)
		{
			if(ycomboboxReason.SelectedItem != null)
			{
				SetDiscount();
			}
			else
			{
				SetDiscountUnitEditable();
				spinDiscount.ValueAsDecimal = 0;
				SetDiscountEditable();
				_discountsController.ClearDiscounts(Entity.ObservableOrderItems);
			}
		}

		protected void OnEnumDiscountUnitEnumItemSelected(object sender, EnumItemClickedEventArgs e)
		{
			SetDiscountEditable();
			var sum = Entity.ObservableOrderItems.Sum(i => i.CurrentCount * i.Price);
			var unit = (DiscountUnits)e.ItemEnum;
			spinDiscount.Adjustment.Upper = unit == DiscountUnits.money ? (double)sum : 100d;

			if(unit == DiscountUnits.percent && spinDiscount.Value > 100)
			{
				spinDiscount.Value = 100;
			}
			if(spinDiscount.ValueAsDecimal == 0)
			{
				return;
			}

			SetDiscount();
		}

		private void OnEntryBottlesToReturnChanged(object sender, EventArgs e)
		{
			HboxReturnTareReasonCategoriesShow();

			if(DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
			{
				UpdateDelivery();
			}
		}

		private void HboxReturnTareReasonCategoriesShow()
		{
			if(Entity.BottlesReturn.HasValue && Entity.BottlesReturn > 0)
			{
				hboxReturnTareReason.Visible = Entity.GetTotalWater19LCount() == 0;
				if(!hboxReturnTareReason.Visible)
				{
					hboxReasons.Visible = false;
					Entity.RemoveReturnTareReason();
				}
			}
			else
			{
				hboxReturnTareReason.Visible = hboxReasons.Visible = false;
				Entity.RemoveReturnTareReason();
			}
		}

		private void YCmbReturnTareReasonCategoriesOnChanged(object sender, EventArgs e)
		{
			ChangeHboxReasonsVisibility();
		}

		private void ChangeHboxReasonsVisibility()
		{
			if(yCmbReturnTareReasonCategories.SelectedItem is ReturnTareReasonCategory category)
			{
				if(!hboxReasons.Visible)
				{
					hboxReasons.Visible = true;
				}

				yCmbReturnTareReasons.ItemsList = category.ChildReasons;
			}
		}

		private void OnButtonCopyManagerCommentClicked(object sender, EventArgs e)
		{
			var cb = textManagerComments.GetClipboard(Selection.Clipboard);
			cb.Text = textManagerComments.Buffer.Text;
		}

		#endregion

		#region Service functions

		/// <summary>
		/// Is the payment type barter or cashless?
		/// </summary>
		private bool IsPaymentTypeBarterOrCashless() => PaymentType == PaymentType.Barter || PaymentType == PaymentType.Cashless;

		/// <summary>
		/// Is the payment type cashless?
		/// </summary>
		private bool IsPaymentTypeCashless() => PaymentType == PaymentType.Cashless;
		#endregion

		//реализация метода интерфейса ITdiTabAddedNotifier
		public void OnTabAdded()
		{
			//если новый заказ и не создан из недовоза (templateOrder заполняется только из недовоза)
			if(Entity.Id == 0 && _templateOrder == null && Counterparty == null)
			{
				//открыть окно выбора контрагента
				entityVMEntryClient.OpenSelectDialog();
			}
		}

		public virtual bool HideItemFromDirectionReasonComboInEquipment(OrderEquipment node, DirectionReason item)
		{
			switch(item)
			{
				case DirectionReason.None:
					return true;
				case DirectionReason.Rent:
					return node.Direction == Core.Domain.Orders.Direction.Deliver;
				case DirectionReason.Repair:
				case DirectionReason.Cleaning:
				case DirectionReason.RepairAndCleaning:
				default:
					return false;
			}
		}

		private void UpdateClientSecondOrderDiscount()
		{
			var result = _discountsController.UpdateClientSecondOrderDiscount(UoW, Entity);
			
			if(result.IsFailureWithDescription)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, result.Description);
			}
		}

		private void Entity_UpdateClientCanChange(object aList, int[] aIdx)
		{
			entityVMEntryClient.IsEditable = Entity.CanChangeContractor();
		}

		private void Entity_ObservableOrderItems_ElementAdded(object aList, int[] aIdx)
		{
			FixPrice(aIdx[0]);

			if(DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
			{
				Entity.CheckAndSetOrderIsService();
				UpdateDelivery();
			}
			_treeItemsNomenclatureColumnWidth = treeItems.ColumnsConfig.GetColumnsByTag(nameof(Nomenclature)).First().Width;
			treeItems.ExposeEvent += TreeItemsOnExposeEvent;

			UpdateClientSecondOrderDiscount();
			UpdateUIState();
			NotifyDeliveryPricePanel();
		}

		private void TreeItemsOnExposeEvent(object o, ExposeEventArgs args)
		{
			if(_treeItemsNomenclatureColumnWidth != ((yTreeView)o).ColumnsConfig.GetColumnsByTag(nameof(Nomenclature)).First().Width)
			{
				EditGoodsCountCellOnAdd((yTreeView)o);
				treeItems.ExposeEvent -= TreeItemsOnExposeEvent;
			}
		}

		private void ObservableOrderItems_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			var items = aList as GenericObservableList<OrderItem>;
			for(var i = 0; i < items?.Count; i++)
			{
				FixPrice(i);
			}

			HboxReturnTareReasonCategoriesShow();

			if(DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
			{
				Entity.CheckAndSetOrderIsService();
				UpdateDelivery();
			}

			Entity.AddFastDeliveryNomenclatureIfNeeded(UoW, _orderContractUpdater, _saleHandler);
			Entity.UpdateMasterCallNomenclatureIfNeeded(UoW, _orderContractUpdater, _saleHandler);

			UpdateClientSecondOrderDiscount();
			UpdateUIState();
			NotifyDeliveryPricePanel();
		}

		private void NotifyDeliveryPricePanel()
		{
			CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Entity.OrderAddressType));
		}

		private void ObservableOrderDocuments_ListChanged(object aList)
		{
			ShowOrderColumnInDocumentsList();
		}

		private void ObservableOrderDocuments_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			ShowOrderColumnInDocumentsList();
		}

		private void ObservableOrderDocuments_ElementAdded(object aList, int[] aIdx)
		{
			ShowOrderColumnInDocumentsList();
		}

		private void ShowOrderColumnInDocumentsList()
		{
			var column = treeDocuments.ColumnsConfig.GetColumnsByTag("OrderNumberColumn").FirstOrDefault();
			column.Visible = Entity.ObservableOrderDocuments.Any(x => x.Order.Id != x.AttachedToOrder.Id);
		}

		private void UpdateOrderItemsPrices()
		{
			for(int i = 0; i < Entity.ObservableOrderItems.Count; i++)
			{
				FixPrice(i);
			}
// ... 141 lines omitted
§ function function (L5389-L5449)
		private void UpdateUIState()
		{
			bool val = Entity.CanEditByStatus && CanEditByPermission;
			buttonSelectPaymentType.Sensitive = (Counterparty != null) && val && !chkContractCloser.Active && !Entity.IsOrderCashlessAndPaid;
			if(entryDeliveryPoint.ViewModel != null)
			{
				entryDeliveryPoint.ViewModel.IsEditable = val;
			}

			SetDeliveryScheduleSelectionEditable(val);

			ybuttonFastDeliveryCheck.Sensitive = ycheckFastDelivery.Sensitive = !checkSelfDelivery.Active && val && Entity.CanChangeFastDelivery;
			lblDeliveryPoint.Sensitive = entryDeliveryPoint.Sensitive = !checkSelfDelivery.Active && val && Counterparty != null;
			buttonAddMaster.Sensitive = !checkSelfDelivery.Active && val && !Entity.IsLoadedFrom1C;
			enumSignatureType.Sensitive =
				enumDocumentType.Sensitive = val;
			buttonAddDoneService.Sensitive = buttonAddServiceClaim.Sensitive =
				buttonAddForSale.Sensitive = val;
			checkDelivered.Sensitive = checkSelfDelivery.Sensitive = val;
			dataSumDifferenceReason.Sensitive = val;
			ycheckContactlessDelivery.Sensitive = val && _canSetContactlessDelivery;
			enumDiscountUnit.Visible = spinDiscount.Visible = labelDiscont.Visible = vseparatorDiscont.Visible = val;
			ChangeOrderEditable(val);

			ChangeGoodsSensitive(val
				|| (IsStatusForEditGoodsInRouteList && _canEditGoodsInRouteList));

			UpdateDiscountReasonSensitive();

			checkPayAfterLoad.Sensitive = _canSetPaymentAfterLoad && checkSelfDelivery.Active && val;

			UpdateButtonState();
			ControlsActionBottleAccessibility();
			chkContractCloser.Sensitive = _canSetContractCloser && val && !Entity.SelfDelivery;
			lblTax.Visible = enumTax.Visible = val && IsEnumTaxVisible();

			if(Entity != null)
			{
				yCmbPromoSets.Sensitive = val;
			}

			var canChangeSelfDeliveryGeoGroup = val
				|| (Entity.SelfDelivery && Entity.OrderStatus == OrderStatus.WaitForPayment && Entity.SelfDeliveryGeoGroup == null);

			ylabelGeoGroup.Sensitive = canChangeSelfDeliveryGeoGroup;
			specialListCmbSelfDeliveryGeoGroup.Sensitive = canChangeSelfDeliveryGeoGroup;
			ybuttonSaveWaitUntil.Visible = Entity.Id != 0;

			if(PaymentType == PaymentType.Cashless 
				&& Order.HasNonPaidDeliveryItems())
			{
				enumAddRentButton.Sensitive = val && !Entity.IsLoadedFrom1C && Order.HasDepositItems();

				buttonAddForSale.Sensitive = !Entity.IsLoadedFrom1C && !Order.HasDepositItems();
			}
			else
			{
				enumAddRentButton.Sensitive = val && !Entity.IsLoadedFrom1C;
				buttonAddForSale.Sensitive = !Entity.IsLoadedFrom1C;
			}
		}
// ... 106 lines omitted
§ function function (L5556-L5605)
		private void UpdateButtonState()
		{
			if(!CanEditByPermission || !_canEditOrder)
			{
				buttonEditOrder.Sensitive = false;
				buttonEditOrder.TooltipText = "Нет права на редактирование";
			}

			buttonSave.Sensitive = CanEditByPermission;
			menubuttonActions.Sensitive = CanEditByPermission;
			yBtnAddCurrentContract.Sensitive = CanEditByPermission;

			if(Entity.CanSetOrderAsAccepted
				&& (CanFormOrderWithLiquidatedCounterparty || (Counterparty?.RevenueStatus == null || Counterparty?.RevenueStatus == RevenueStatus.Active)))
			{
				btnForm.Visible = true;
				buttonEditOrder.Visible = false;
			}
			else if(Entity.CanSetOrderAsEditable && !Entity.IsOldServiceOrder)
			{
				buttonEditOrder.Visible = true;
				btnForm.Visible = false;
			}
			else
			{
				btnForm.Visible = false;
				buttonEditOrder.Visible = false;
			}

			textComments.Editable = CanEditByPermission;
			btnSaveComment.Sensitive = CanEditByPermission && Entity.OrderStatus != OrderStatus.NewOrder;

			//если новый заказ и тип платежа бартер или безнал, то вкл кнопку
			buttonWaitForPayment.Sensitive = CanEditByPermission && Entity.OrderStatus == OrderStatus.NewOrder && IsPaymentTypeBarterOrCashless() && !Entity.SelfDelivery;

			buttonCancelOrder.Sensitive = CanEditByPermission &&
				(_orderRepository.GetStatusesForOrderCancelation().Contains(Entity.OrderStatus)
					|| (Entity.SelfDelivery && Entity.OrderStatus == OrderStatus.OnLoading)) && Entity.OrderStatus != OrderStatus.NewOrder;

			_menuItemSelfDeliveryToLoading.Sensitive = Entity.SelfDelivery
				&& Entity.OrderStatus == OrderStatus.Accepted
				&& _allowLoadSelfDelivery;
			_menuItemSelfDeliveryPaid.Sensitive = Entity.SelfDelivery
				&& (PaymentType == PaymentType.Cashless || PaymentType == PaymentType.PaidOnline)
				&& Entity.OrderStatus == OrderStatus.WaitForPayment
				&& _acceptCashlessPaidSelfDelivery;

			_menuItemCloseOrder.Sensitive = Entity.OrderStatus == OrderStatus.Accepted && _canCloseOrders && !Entity.SelfDelivery;
			_menuItemReturnToAccepted.Sensitive = Entity.OrderStatus == OrderStatus.Closed && Entity.CanBeMovedFromClosedToAcepted;
		}
7/253 chunks shown (14748 tokens)
[lean-ctx] full source: read "C:\Users\Kostya\source\repos\Vodovoz\Source\Applications\Desktop\Vodovoz\Dialogs\OrderWidgets\OrderDlg.cs" directly (no MCP)  ·  or ctx_read("C:\Users\Kostya\source\repos\Vodovoz\Source\Applications\Desktop\Vodovoz\Dialogs\OrderWidgets\OrderDlg.cs", mode="full")
