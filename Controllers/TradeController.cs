﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using StocksApp.Application.Interfaces;
using StocksApp.Domain.Models;
using StocksApp.Presentation.Models;
using StocksApp.Presentation.Models.ViewModels;

namespace StocksApp.Controllers
{
    [Route("trade")]
    public class TradeController : Controller
    {
		#region private readonly fields
		private readonly TradingOptions _tradingOptions;
        private readonly IFinnhubService _finnhubService;
        private readonly IStocksService _stocksService;
        private readonly IConfiguration _configuration;
		#endregion

		#region constructors
		public TradeController(IOptions<TradingOptions> tradingOptions, IFinnhubService finnhubService, IStocksService stocksService, IConfiguration configuration)
        {
            _tradingOptions = tradingOptions.Value;
            _finnhubService = finnhubService;
            _stocksService = stocksService;
            _configuration = configuration;
        }
		#endregion

		[HttpGet]
        [Route("/")]
        [Route("index")]
        public async Task<IActionResult> Index()
		{
            try
            {
                string defaultStockSymbol = _tradingOptions.DefaultStockSymbol
                    ?? throw new Exception("Default Stock Symbol not found in configuration.");

                Dictionary<string, object> stockQuote = await _finnhubService.GetStockPriceQuote(defaultStockSymbol)
                    ?? throw new Exception("Failed to retrieve stockQuote from finnhubService.");

                Dictionary<string, object> companyProfile = await _finnhubService.GetCompanyProfile(defaultStockSymbol)
                    ?? throw new Exception("Failed to retrieve companyProfile from finnhubService.");

                string stockSymbol = companyProfile
                    .GetValueOrDefault("ticker")?
                    .ToString()
                    ?? "Unknown";
                string stockName = companyProfile
                    .GetValueOrDefault("name")?
                    .ToString()
                    ?? "Unknown";
                int quantity = companyProfile.TryGetValue("shareOutstanding", out var quantityValue)
                    ? (int)Convert.ToDouble(quantityValue.ToString())
                    : 0;
                double price = stockQuote.TryGetValue("c", out var priceValue)
                    ? Convert.ToDouble(priceValue.ToString())
                    : 0;

                var stockTradeViewModel = new StockTradeViewModel()
                {
                    StockSymbol = stockSymbol,
                    StockName = stockName,
                    Price = price,
                    Quantity = quantity
                };

                ViewBag.FinnhubAPIKey = _configuration["FinnhubAPIKey"];
                return View(stockTradeViewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View(null);
            }
        }

		[HttpPost]
        [ValidateAntiForgeryToken]
		[Route("buy-order")]
        public IActionResult BuyOrder(BuyOrderRequest request)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Orders", "Trade", request);
            }    

            request.DateAndTimeOfOrder = DateTime.Now;
            BuyOrderResponse response = _stocksService.CreateBuyOrder(request);
            return new RedirectToActionResult("Orders", "Trade", new { });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("sell-order")]
        public IActionResult SellOrder(SellOrderRequest request)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Orders", "Trade", request);
            }

            request.DateAndTimeOfOrder = DateTime.Now;
            SellOrderResponse response = _stocksService.CreateSellOrder(request);
            return new RedirectToActionResult("Orders", "Trade", new { });
        }

        [HttpGet]
        [Route("orders")]
        public IActionResult Orders()
        {
            List<BuyOrderResponse> buyOrders = _stocksService.GetBuyOrders();
            List<SellOrderResponse> sellOrders = _stocksService.GetSellOrders();
            OrdersViewModel viewModel = new OrdersViewModel() { BuyOrders = buyOrders,SellOrders = sellOrders };
            return View(viewModel);
        }   
    }
}