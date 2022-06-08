using _26tack_rewritten.handlers;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.json;
using _26tack_rewritten.models;

namespace _26tack_rewritten.commands.warframeset;
internal class Market : IChatCommand
{
    public Command Info()
    {
        string name = "market";
        string description = "Get sell & buy orders for the specified item from warframe.market";
        string[] aliases = { "price" };
        int[] cooldowns = { 15, 5 };

        return new Command(name, description, aliases, cooldowns);
    }

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

        string desiredItem = string.Join('_', args).ToLower();
        MarketItems? listings = await ExternalAPIHandler.GetMarketItemListings(desiredItem);
        if (listings is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, An error occured whilst trying to get data for your item :(");
            return;
        }

        await Task.Run(() =>
        {
            Order[] orders = listings.payload.orders.Where(x => x.user.status != "offline").ToArray();

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

            string listingsString = $"{totalOrders} Active orders " +
                $"-- {sellersCount} Sellers: Avg. {sellerAveragePrice}P (🡳{cheapestSeller}P   🡱{mostExpensiveSeller}P) " +
                $"-- {buyersCount} Buyers: Avg. {buyerAveragePrice}P (🡱{mostPayingBuyer}P   🡳{leastPayingBuyer}P)";

            MessageHandler.SendMessage(channel, $"@{user}, Item: {desiredItem} => {listingsString}");
        });
    }
}
