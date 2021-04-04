using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCoreOpenBankingAPI.DAL;
using NetCoreOpenBankingAPI.Models;
using NetCoreOpenBankingAPI.Services.Interfaces;
using NetCoreOpenBankingAPI.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreOpenBankingAPI.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
        private MyDbContext _dbContext;

        private ILogger<TransactionService> _logger;
        private IUserService _userService;
        private AppSettings _settings;
        private static string _bankSettlementAccount;

        public TransactionService(MyDbContext dbContext, ILogger<TransactionService> logger, IUserService userService, IOptions<AppSettings> settings)
        {
            _dbContext = dbContext;
            _logger = logger;
            _userService = userService;
            _settings = settings.Value;

            _bankSettlementAccount = _settings.NetCoreBankSettlementAccount;

        }

        public Response CreateNewTransaction(Transaction transaction)
        {
            Response response = new Response();
            try
            {
                _dbContext.Transactions.Add(transaction);
                _dbContext.SaveChanges();
                response.ResponseCode = "00";

                response.ResponseMessage = "Transaction created successfully!";
                response.Data = null;
            }
            catch (Exception ex)
            {

                _logger.LogError($"AN ERROR OCCURRED => {ex.Message}");
            }
            return response;

        }

        public Response FindTransactionByDate(DateTime date)
        {
            throw new NotImplementedException();
        }

        public Response MakeDeposit(string AccountNumber, decimal Amount, string TransactionPin)
        {
            Response response = new Response();
            Account sourceAccount; //our Bank Settlement Account
            Account destinationAccount; //individual
            Transaction transaction = new Transaction();

            var authenticateUser = _userService.Authenticate(AccountNumber, TransactionPin);
            if(authenticateUser == null)
            {
                throw new ApplicationException("Invalid Auth details");
            }

            try
            {
                sourceAccount = _userService.GetByAccountNumber(_bankSettlementAccount);
                destinationAccount = _userService.GetByAccountNumber(AccountNumber);

                sourceAccount.CurrentAccountBalance -= Amount;
                destinationAccount.CurrentAccountBalance += Amount;

                if( (_dbContext.Entry(sourceAccount).State == Microsoft.EntityFrameworkCore.EntityState.Modified) && (_dbContext.Entry(destinationAccount).State == Microsoft.EntityFrameworkCore.EntityState.Modified))
                {
                    //sso there was an update
                    transaction.TransactionStatus = TranStatus.Success;
                    response.ResponseCode = "00";
                    response.ResponseMessage = "Transaction Successful!";
                    response.Data = null;

                }
                else
                {
                    transaction.TransactionStatus = TranStatus.Failed;
                    response.ResponseCode = "00";
                    response.ResponseMessage = "Transaction Failed!";
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {

                _logger.LogError($"ERROR OCCURRED => MESSAGE: {ex.Message}");
            }

            transaction.TransactionDate = DateTime.Now;
            transaction.TransactionType = TranType.Deposit;
            transaction.TransactionAmount = Amount;
            transaction.TransactionSourceAccount = _bankSettlementAccount;
            transaction.TransactionDestinationAccount = AccountNumber;
            transaction.TransactionParticulars = $"NEW Transaction FROM SOURCE {JsonConvert.SerializeObject(transaction.TransactionSourceAccount)} TO DESTINATION => {JsonConvert.SerializeObject(transaction.TransactionDestinationAccount)} ON {transaction.TransactionDate} TRAN_TYPE =>  {transaction.TransactionType} TRAN_STATUS => {transaction.TransactionStatus}";

            _dbContext.Transactions.Add(transaction);
            _dbContext.SaveChanges();


            return response;

        }

        public Response MakeFundsTransfer(string FromAccount, string ToAccount, decimal Amount, string TransactionPin)
        {
            //3 accounts or 2 are involved

            //FromAccount iss our current user/customer's account and we'll authenticate with it...
            Response response = new Response();
            Account sourceAccount; //our current authenticated customer
            Account destinationAccount; //target account where money is being sent to...
            Transaction transaction = new Transaction();

            //let's authenticate first
            var authenticateUser = _userService.Authenticate(FromAccount, TransactionPin);
            if(authenticateUser == null)
            {

                throw new ApplicationException("Invalid credentials");
            }
            //user authenticated, then llet's process funds transfer;
            try
            {
                sourceAccount = _userService.GetByAccountNumber(FromAccount);
                destinationAccount = _userService.GetByAccountNumber(ToAccount);

                sourceAccount.CurrentAccountBalance -= Amount; //remove the tranamount from the source customer's balance
                destinationAccount.CurrentAccountBalance += Amount; //add tranamount to our target customer's balance...

                if ((_dbContext.Entry(sourceAccount).State == Microsoft.EntityFrameworkCore.EntityState.Modified) && (_dbContext.Entry(destinationAccount).State == Microsoft.EntityFrameworkCore.EntityState.Modified))
                {
                    //so there was an update in the context State
                    transaction.TransactionStatus = TranStatus.Success;
                    response.ResponseCode = "00";
                    response.ResponseMessage = "Transaction Successful!";
                    response.Data = null;

                }
                else
                {
                    transaction.TransactionStatus = TranStatus.Failed;
                    response.ResponseCode = "00";
                    response.ResponseMessage = "Transaction Failed!";
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {

                _logger.LogError($"AN ERROR OCCURRED => MESSAGE: {ex.Message}");
            }

            transaction.TransactionDate = DateTime.Now;
            transaction.TransactionType = TranType.Transfer;
            transaction.TransactionAmount = Amount;
            transaction.TransactionSourceAccount = FromAccount;
            transaction.TransactionDestinationAccount = ToAccount;
            transaction.TransactionParticulars = $"NEW Transaction FROM SOURCE {JsonConvert.SerializeObject(transaction.TransactionSourceAccount)} TO DESTINATION => {JsonConvert.SerializeObject(transaction.TransactionDestinationAccount)} ON {transaction.TransactionDate} TRAN_TYPE =>  {transaction.TransactionType} TRAN_STATUS => {transaction.TransactionStatus}";

            _dbContext.Transactions.Add(transaction);
            _dbContext.SaveChanges();


            return response;

        }

        public Response MakeWithdrawal(string AccountNumber, decimal Amount, string TransactionPin)
        {
            Response response = new Response();
            Account sourceAccount; //individual
            Account destinationAccount; //our Bank Settlement Account
            Transaction transaction = new Transaction();

            var authenticateUser = _userService.Authenticate(AccountNumber, TransactionPin);
            if (authenticateUser == null)
            {
                throw new ApplicationException("Invalid Auth details");
            }

            try
            {
                sourceAccount = _userService.GetByAccountNumber(AccountNumber);
                destinationAccount = _userService.GetByAccountNumber(_bankSettlementAccount);

                sourceAccount.CurrentAccountBalance -= Amount; //remove the tranamount from the customer's balance
                destinationAccount.CurrentAccountBalance += Amount; //add tranamount to our bankSettlement...

                if ((_dbContext.Entry(sourceAccount).State == Microsoft.EntityFrameworkCore.EntityState.Modified) && (_dbContext.Entry(destinationAccount).State == Microsoft.EntityFrameworkCore.EntityState.Modified))
                {
                    //so there was an update in the context State
                    transaction.TransactionStatus = TranStatus.Success;
                    response.ResponseCode = "00";
                    response.ResponseMessage = "Transaction Successful!";
                    response.Data = null;

                }
                else
                {
                    transaction.TransactionStatus = TranStatus.Failed;
                    response.ResponseCode = "00";
                    response.ResponseMessage = "Transaction Failed!";
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {

                _logger.LogError($"AN ERROR OCCURRED => MESSAGE: {ex.Message}");
            }

            transaction.TransactionDate = DateTime.Now;
            transaction.TransactionType = TranType.Withdrawal;
            transaction.TransactionAmount = Amount;
            transaction.TransactionSourceAccount = _bankSettlementAccount;
            transaction.TransactionDestinationAccount = AccountNumber;
            transaction.TransactionParticulars = $"NEW Transaction FROM SOURCE {JsonConvert.SerializeObject(transaction.TransactionSourceAccount)} TO DESTINATION => {JsonConvert.SerializeObject(transaction.TransactionDestinationAccount)} ON {transaction.TransactionDate} TRAN_TYPE =>  {transaction.TransactionType} TRAN_STATUS => {transaction.TransactionStatus}";

            _dbContext.Transactions.Add(transaction);
            _dbContext.SaveChanges();


            return response;
        }
    }
}
