export interface Category {
  id: number;
  name: string;
  type: 'Expense' | 'Income' | 'Neutral';
}

export interface Transaction {
  id: number;
  amount: number;
  date: string;
  type: 'Expense' | 'Income';
  categoryId: number;
  categoryName: string;
}
