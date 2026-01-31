namespace ReposcraperTest.ExtractorTest.Sample
{
    using System;
    using System.Collections.Generic;

    namespace BankingSystem
    {
        /// <summary>
        /// Represents a bank account with basic operations like deposit and withdrawal
        /// </summary>
        public class BankAccount
        {
            // Private fields
            private string _accountNumber;
            private string _accountHolder;
            private decimal _balance;
            private List<string> _transactionHistory;

            /// <summary>
            /// Initializes a new instance of BankAccount class
            /// </summary>
            /// <param name="accountNumber">The unique account number</param>
            /// <param name="accountHolder">The name of the account holder</param>
            /// <param name="initialBalance">The initial balance for the account</param>
            public BankAccount(string accountNumber, string accountHolder, decimal initialBalance = 0)
            {
                _accountNumber = accountNumber;
                _accountHolder = accountHolder;
                _balance = initialBalance;
                _transactionHistory = new List<string>();

                // Record the initial deposit if any
                if (initialBalance > 0)
                {
                    _transactionHistory.Add($"Account opened with initial balance: {initialBalance:C}");
                }
            }

            /// <summary>
            /// Gets the current account balance
            /// </summary>
            /// <returns>The current balance as decimal</returns>
            public decimal GetBalance()
            {
                return _balance;
            }

            /// <summary>
            /// Deposits the specified amount into the account
            /// </summary>
            /// <param name="amount">The amount to deposit</param>
            /// <returns>True if deposit was successful, false otherwise</returns>
            public bool Deposit(decimal amount)
            {
                // Validate the deposit amount
                if (amount <= 0)
                {
                    Console.WriteLine("Deposit amount must be positive.");
                    return false;
                }

                _balance += amount;
                _transactionHistory.Add($"Deposited: {amount:C}");
                Console.WriteLine($"Successfully deposited {amount:C}. New balance: {_balance:C}");
                return true;
            }

            /// <summary>
            /// Withdraws the specified amount from the account
            /// </summary>
            /// <param name="amount">The amount to withdraw</param>
            /// <returns>True if withdrawal was successful, false otherwise</returns>
            public bool Withdraw(decimal amount)
            {
                // Check for sufficient funds
                if (amount <= 0)
                {
                    Console.WriteLine("Withdrawal amount must be positive.");
                    return false;
                }

                if (amount > _balance)
                {
                    Console.WriteLine($"Insufficient funds. Current balance: {_balance:C}");
                    return false;
                }

                _balance -= amount;
                _transactionHistory.Add($"Withdrawn: {amount:C}");
                Console.WriteLine($"Successfully withdrew {amount:C}. New balance: {_balance:C}");
                return true;
            }

            // Helper method to get account summary
            private string GetAccountSummary()
            {
                return $"Account: {_accountNumber}, Holder: {_accountHolder}, Balance: {_balance:C}";
            }

            /// <summary>
            /// Displays the account information and transaction history
            /// </summary>
            public void DisplayAccountInfo()
            {
                Console.WriteLine("=== Account Information ===");
                Console.WriteLine(GetAccountSummary());
                Console.WriteLine("=== Transaction History ===");

                foreach (var transaction in _transactionHistory)
                {
                    Console.WriteLine($"- {transaction}");
                }
            }

            // Static method to validate account number format
            public static bool IsValidAccountNumber(string accountNumber)
            {
                return !string.IsNullOrWhiteSpace(accountNumber) && accountNumber.Length == 10;
            }
        }

        /// <summary>
        /// Manages a collection of bank accounts and provides banking operations
        /// </summary>
        public class Bank
        {
            private Dictionary<string, BankAccount> _accounts;
            private string _bankName;

            // Public property for bank name
            public string BankName
            {
                get { return _bankName; }
            }

            /// <summary>
            /// Initializes a new Bank instance
            /// </summary>
            /// <param name="bankName">The name of the bank</param>
            public Bank(string bankName)
            {
                _bankName = bankName;
                _accounts = new Dictionary<string, BankAccount>();
            }

            /// <summary>
            /// Creates a new bank account and adds it to the bank's collection
            /// </summary>
            /// <param name="accountNumber">The account number</param>
            /// <param name="accountHolder">The account holder name</param>
            /// <param name="initialDeposit">The initial deposit amount</param>
            /// <returns>The created BankAccount object</returns>
            public BankAccount CreateAccount(string accountNumber, string accountHolder, decimal initialDeposit = 0)
            {
                // Validate account number
                if (!BankAccount.IsValidAccountNumber(accountNumber))
                {
                    throw new ArgumentException("Invalid account number format. Must be 10 characters.");
                }

                // Check if account already exists
                if (_accounts.ContainsKey(accountNumber))
                {
                    throw new InvalidOperationException($"Account {accountNumber} already exists.");
                }

                var newAccount = new BankAccount(accountNumber, accountHolder, initialDeposit);
                _accounts[accountNumber] = newAccount;

                Console.WriteLine($"Account created successfully for {accountHolder}");
                return newAccount;
            }

            /// <summary>
            /// Retrieves an account by its account number
            /// </summary>
            /// <param name="accountNumber">The account number to search for</param>
            /// <returns>The BankAccount object if found, null otherwise</returns>
            public BankAccount GetAccount(string accountNumber)
            {
                if (_accounts.TryGetValue(accountNumber, out BankAccount account))
                {
                    return account;
                }

                Console.WriteLine($"Account {accountNumber} not found.");
                return null;
            }

            /// <summary>
            /// Transfers money between two accounts
            /// </summary>
            /// <param name="fromAccountNumber">Source account number</param>
            /// <param name="toAccountNumber">Destination account number</param>
            /// <param name="amount">Amount to transfer</param>
            /// <returns>True if transfer was successful, false otherwise</returns>
            public bool Transfer(string fromAccountNumber, string toAccountNumber, decimal amount)
            {
                var fromAccount = GetAccount(fromAccountNumber);
                var toAccount = GetAccount(toAccountNumber);

                if (fromAccount == null || toAccount == null)
                {
                    Console.WriteLine("One or both accounts not found.");
                    return false;
                }

                // Withdraw from source account
                if (fromAccount.Withdraw(amount))
                {
                    // Deposit to destination account
                    if (toAccount.Deposit(amount))
                    {
                        Console.WriteLine($"Transfer of {amount:C} from {fromAccountNumber} to {toAccountNumber} completed.");
                        return true;
                    }
                    else
                    {
                        // Rollback the withdrawal if deposit fails
                        fromAccount.Deposit(amount);
                        Console.WriteLine("Transfer failed. Rolled back withdrawal.");
                        return false;
                    }
                }

                return false;
            }

            /// <summary>
            /// Displays summary of all accounts in the bank
            /// </summary>
            public void DisplayAllAccounts()
            {
                Console.WriteLine($"=== Accounts in {_bankName} ===");

                if (_accounts.Count == 0)
                {
                    Console.WriteLine("No accounts found.");
                    return;
                }

                foreach (var account in _accounts.Values)
                {
                    account.DisplayAccountInfo();
                    Console.WriteLine("---");
                }
            }

            // Internal method for bank operations
            internal void ProcessEndOfDay()
            {
                Console.WriteLine($"Processing end of day for {_bankName}...");
                Console.WriteLine($"Total accounts: {_accounts.Count}");
                // In a real system, this would do more complex operations
            }
        }
    }
}
