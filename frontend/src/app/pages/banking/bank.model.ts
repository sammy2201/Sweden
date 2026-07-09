export interface Bank {
  name: string;
  website: string;

  providesBankId: boolean;
  providesSwish: boolean;

  studentFriendly: boolean;
  englishSupport: boolean;

  debitCard: boolean;
  creditCard: boolean;

  applePay: boolean;
  googleWallet: boolean;
  samsungWallet: boolean;

  physicalBranches: boolean;
  mobileApp: boolean;

  accountWithoutPersonnummer: boolean;
  businessAccounts: boolean;

  fee: string;

  notes: string;

  recommendedFor?: string[];
}
