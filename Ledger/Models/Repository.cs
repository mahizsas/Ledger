﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Dapper;

namespace Ledger.Models
{
    public class Repository
    {
        readonly IDbConnection _connection;

        public Repository()
        {
            var connectionString = @"Data Source=" + ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            _connection = new SQLiteConnection(connectionString);
        }

        public List<Transaction> GetUnreconciled(int ledger)
        {
            var sql = @"SELECT id, desc, amount, datedue, datepayed, datereconciled, account, ledger
                        FROM transactions
                        WHERE datereconciled IS null
                        AND datepayed IS NOT null
                        AND ledger = @ledger";
            return _connection.Query<Transaction>(sql, new {ledger}).ToList();
        }

        public decimal GetCurrentBalance(int ledger)
        {
            var sql = "SELECT SUM(amount) FROM transactions WHERE datereconciled is not null AND ledger = @ledger";
            return (decimal)Math.Round(_connection.Query<double>(sql, new {ledger}).First(), 2);
        }

        public decimal GetActualBalance(int ledger)
        {
            var sql = "SELECT SUM(amount) FROM transactions WHERE datepayed is not null AND ledger = @ledger";
            return (decimal) Math.Round(_connection.Query<double>(sql, new {ledger}).First(), 2);
        }

        public IEnumerable<LedgerEntity> GetAllLedgers()
        {
            var sql = @"SELECT ledger, ledgerdesc
                        FROM ledgers
                        ORDER BY ledgerdesc";
            return _connection.Query<LedgerEntity>(sql);
        }

        public IEnumerable<Account> GetAllAccounts()
        {
            var sql = @"SELECT id, [desc]
                        FROM accounts
                        ORDER BY [desc]";
            return _connection.Query<Account>(sql);
        }

        public void CreateTransaction(Transaction transaction)
        {
            var sql = @"INSERT INTO transactions (desc, amount, datedue, datepayed, datereconciled, account, ledger) VALUES (
                        @Desc, @Amount, @DateDue, @DatePayed, @DateReconciled, @Account, @Ledger)";
            _connection.Execute(sql, new {transaction});
        }

        public void MarkTransactionReconciled(int id, DateTime reconcileDate)
        {
            var sql = @"UPDATE transactions SET
                        DateReconciled = @DateReconciled
                        WHERE id = @id";
            _connection.Execute(sql, new { id, DateReconciled = reconcileDate });
        }

        public void DeleteTransaction(int id)
        {
            var sql = @"DELETE FROM transactions WHERE id = @id";
            _connection.Execute(sql, new {id});
        }

        public Transaction GetTransaction(long id)
        {
            var sql = @"SELECT id, desc, amount, datedue, datepayed, datereconciled, account, ledger
                        FROM transactions
                        WHERE id = @id";
            return _connection.Query<Transaction>(sql, new {id}).FirstOrDefault();
        }

        public void UpdateTransaction(Transaction transaction)
        {
            var sql = @"UPDATE transactions SET
                            [desc] = @Desc,
                            amount = @Amount,
                            datedue = @DateDue,
                            datepayed = @DatePayed,
                            datereconciled = @DateReconciled,
                            account = @Account,
                            ledger = @Ledger
                        WHERE id = @Id";
            _connection.Execute(sql, transaction);
        }

        public List<Transaction> GetBillsDue(int id)
        {
            throw new NotImplementedException();
        }
    }
}