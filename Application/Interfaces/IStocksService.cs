﻿using StocksApp.Presentation.Models;

namespace StocksApp.Application.Interfaces
{
    public interface IStocksService
    {
        BuyOrderResponse CreateBuyOrder(BuyOrderRequest? buyOrderRequest);

        SellOrderResponse CreateSellOrder(SellOrderRequest? sellOrderRequest);

        List<BuyOrderResponse> GetBuyOrders();

        List<SellOrderResponse> GetSellOrders();
    }
}
