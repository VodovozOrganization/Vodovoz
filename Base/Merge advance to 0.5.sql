UPDATE cash_advance_closing SET money = (SELECT money FROM cash_income WHERE id = cash_advance_closing.income_id) WHERE income_id IS NOT NULL;

UPDATE cash_advance_closing SET money = (SELECT money FROM cash_expense WHERE id = cash_advance_closing.expense_id) WHERE advance_report_id IS NOT NULL;
