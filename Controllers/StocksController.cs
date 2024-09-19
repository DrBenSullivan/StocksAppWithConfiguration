﻿using Microsoft.AspNetCore.Mvc;
using StocksApp.Application.Interfaces;
using StocksApp.Domain.Models;
using StocksApp.Presentation.Models.ViewModels;

namespace StocksApp.Controllers
{
	public class StocksController : Controller
	{
		#region private readonly fields
		private readonly IFinnhubService _finnhubService;
		private readonly IConfiguration _configuration;
		#endregion

		#region constructor
		public StocksController(IFinnhubService finnhubService, IConfiguration configuration)
		{
			_finnhubService = finnhubService;
			_configuration = configuration;	
		}
		#endregion

		[HttpGet]
		[Route("/Stocks/Explore")]
		public async Task<IActionResult> Explore(string? stock, bool displayAll = false)
		{
			try
			{
				List<FinnhubStock> stocksResponse = await _finnhubService.GetStocks()
					?? throw new Exception("Failed to retrieve stocks data from FinnhubAPI.");

				string[] topStockSymbols = _configuration["Top25PopularStocks"].Split(',')
					?? throw new Exception("Top 25 Popular Stocks not available in the current configuration.");

				var stocks = new List<Stock>();

				foreach (var stockSymbol in topStockSymbols)
				{
					FinnhubStock includedStock = stocksResponse.FirstOrDefault(r => r.symbol == stockSymbol)
						?? throw new Exception($"Stock with symbol {stockSymbol} could not be found in the FinnhubAPI Response.");

					stocks.Add(new Stock
					{
						StockName = includedStock.description,
						StockSymbol = includedStock.symbol
					});
				}
				return View(new StocksExploreViewModel{Stocks = stocks });
			}

			catch (Exception ex) 
			{
				Console.WriteLine(ex.Message);
				return View(null);
			}
		}
	}
}
