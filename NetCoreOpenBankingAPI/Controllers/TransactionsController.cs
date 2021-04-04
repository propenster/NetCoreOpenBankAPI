using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NetCoreOpenBankingAPI.Models;
using NetCoreOpenBankingAPI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetCoreOpenBankingAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private IMapper _mapper;

        public TransactionsController(ITransactionService transactionService, IMapper mapper)
        {
            _transactionService = transactionService;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("create_new_transaction")]
        public IActionResult CreateNewTransation([FromBody] TransactionRequestDto transaction)
        {
            if (!ModelState.IsValid) return BadRequest(transaction);

            var transactionRequest = _mapper.Map<Transaction>(transaction);

            return Ok(_transactionService.CreateNewTransaction(transactionRequest));
        }

        [HttpPost]
        [Route("make_deposit")]
        public IActionResult MakeDeposit(string AccountNumber, decimal Amount, string TransactionPin)
        {
            return Ok(_transactionService.MakeDeposit(AccountNumber, Amount, TransactionPin));
        }
        [HttpPost]
        [Route("make_funds_transfer")]
        public IActionResult MakeFundsTransfer(string FromAccount, string ToAccount, decimal Amount, string TransactionPin)
        {
            if (FromAccount.Equals(ToAccount)) return BadRequest("You cannot transfer money to yourself");

            return Ok(_transactionService.MakeFundsTransfer(FromAccount, ToAccount, Amount, TransactionPin));
        }

        [HttpPost]
        [Route("make_withdrawal")]
        public IActionResult MakeWithdrawal(string AccountNumber, decimal Amount, string TransactionPin)
        {
            //try check validity of accountNumber
            if (!Regex.IsMatch(AccountNumber, @"^[0][1-9]\d{9}$|^[1-9]\d{9}$")) return BadRequest("Your Account Number can only be 10 digits");

            return Ok(_transactionService.MakeWithdrawal(AccountNumber, Amount, TransactionPin));

        }


    }
}
