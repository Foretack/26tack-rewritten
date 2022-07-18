using Tack.Handlers;
using Tack.Nonclass;
using Tack.Json;
using Tack.Models;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal class Market : IChatCommand
{
    public Command Info() => new(
        name: "market",
        description: "Get sell & buy orders for the specified item from warframe.market. Additional options: `activeOnly:true/false` (true default)",
        aliases: new string[] { "price" },
        cooldowns: new int[] {10, 3}
        );

    public async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        string[] args = ctx.Args;

        if (args.Length == 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, you need to specify the item... FeelsDankMan");
            return;
        }

        string option1 = "activeOnly";
        bool activeOnly = Options.ParseBool(option1, ctx.IrcMessage.Message) ?? true;
        string desiredItem = string.Join('_', args.Where(x => !x.StartsWith(option1))).ToLower();
        MarketItems? listings = await ExternalAPIHandler.GetMarketItemListings(desiredItem);
        if (listings is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, An error occured whilst trying to get data for your item :(");
            return;
        }

        await Task.Run(() =>
        {
            Order[] orders = listings.payload.orders
            .Where(x => !activeOnly || x.user.status != "offline")
            .ToArray();

            int totalOrders = orders.Length;
            int cheapestSeller = int.MaxValue;
            int mostExpensiveSeller = 0;
            int sellersTotalPrice = 0;
            int sellersTotalQuantity = 0;
            int sellersCount = 0;
            int mostPayingBuyer = 0;
            int leastPayingBuyer = int.MaxValue;
            int buyersTotalPrice = 0;
            int buyersTotalQuantity = 0;
            int buyersCount = 0;
            float sellerAveragePrice = 0;
            float buyerAveragePrice = 0;

            foreach (Order o in orders)
            {
                if (o.order_type == "sell")
                {
                    sellersCount++;
                    sellersTotalPrice += o.platinum * o.quantity;
                    sellersTotalQuantity += o.quantity;
                    if (o.platinum < cheapestSeller) cheapestSeller = o.platinum;
                    if (o.platinum > mostExpensiveSeller) mostExpensiveSeller = o.platinum;
                    continue;
                }
                buyersCount++;
                buyersTotalPrice += o.platinum * o.quantity;
                buyersTotalQuantity += o.quantity;
                if (o.platinum < leastPayingBuyer) leastPayingBuyer = o.platinum;
                if (o.platinum > mostPayingBuyer) mostPayingBuyer = o.platinum;
            }
            sellerAveragePrice = (float)Math.Round((float)sellersTotalPrice / sellersTotalQuantity, 2);
            buyerAveragePrice = (float)Math.Round((float)buyersTotalPrice / buyersTotalQuantity, 2);

            string listingsString = $"{totalOrders} " + (activeOnly ? "Active" : "Total") + " orders " +
                $"◆ {sellersCount} Sellers: Avg. {sellerAveragePrice}P (🡳{cheapestSeller}P ◉ 🡱{mostExpensiveSeller}P) " +
                $"◆ {buyersCount} Buyers: Avg. {buyerAveragePrice}P (🡱{mostPayingBuyer}P ◉ 🡳{leastPayingBuyer}P)";

            MessageHandler.SendMessage(channel, $"@{user}, Item: {desiredItem} => {listingsString}");
        });
    }
}
