﻿namespace PerfumeShop.Infrastructure.Services;

public class BasketService : IBasketService
{
    private readonly IUnitOfWork<ShoppingDbContext> _shopping;
    private readonly IUnitOfWork<CatalogDbContext> _catalog;
    private readonly ILogger<BasketService> _logger;

    public BasketService(
        IUnitOfWork<ShoppingDbContext> shopping,
        IUnitOfWork<CatalogDbContext> catalog,
        ILogger<BasketService> logger)
    {
        _catalog = catalog;
        _shopping = shopping;
        _logger = logger;
    }

    public async Task<BasketItem> AddItemToBasketAsync(string userName, int productId, int quantity = 1)
    {
        var basketRepository = _shopping.GetRepository<Basket>();
        var basket = await basketRepository.GetFirstOrDefaultAsync(
            predicate: b => b.BuyerId == userName,
            include: query => query.Include(b => b.Items));

        if (basket is null)
        {
            basket = new Basket(userName);
            basketRepository.Add(basket);
            await _shopping.SaveChangesAsync();
            _logger.LogInformation($"Create basket with ID '{basket.Id}'.");
        }

        BasketItem basketItem = new(productId, quantity);
        basket.AddItem(basketItem);
        basketRepository.Update(basket);
        await _shopping.SaveChangesAsync();
        _logger.LogInformation($"Basket was updated with ID: '{basket.Id}'.");

        return basketItem;
    }

	public async Task<int> GetBasketId(string userName)
	{
		return await _shopping.GetRepository<Basket>()
            .GetFirstOrDefaultAsync(
            predicate: i => i.BuyerId == userName,
	        selector: b => b.Id);
	}

	public async Task<int> GetProductId(int basketItemId)
	{
		return await _shopping.GetRepository<BasketItem>()
	        .GetFirstOrDefaultAsync(
	        predicate: i => i.Id == basketItemId,
	        selector: b => b.ProductId);
	}

	public async Task<BasketItem> UpdateItemBasketAsync(int basketItemId, int productQuantity = 1)
	{
        var basketItemRepository = _shopping.GetRepository<BasketItem>();
        var basketItem = await basketItemRepository.GetFirstOrDefaultAsync(predicate: i => i.Id == basketItemId)
			?? throw new NullReferenceException($"Basket item not found with ID '{basketItemId}'.");

		basketItem.SetQuantity(productQuantity);
		basketItemRepository.Update(basketItem);
        await _shopping.SaveChangesAsync();

		_logger.LogInformation($"Update qty: '{productQuantity}' for basket item with ID: '{basketItemId}'.");

		return basketItem;
	}

	public async Task<Basket> DeleteBasketAsync(int basketId)
    {
        var basketRepository = _shopping.GetRepository<Basket>();
        var basket = await basketRepository.GetFirstOrDefaultAsync(predicate: b => b.Id == basketId)
            ?? throw new NullReferenceException($"Basket not found with ID '{basketId}'.");

        basketRepository.Remove(basket);
        await _shopping.SaveChangesAsync();
        _logger.LogInformation($"Basket was deleted with ID: '{basket.Id}'.");

        return basket;
    }

    public async Task<BasketItem> DeleteItemFromBasketAsync(int basketItemId)
    {
        var basketRepository = _shopping.GetRepository<BasketItem>();
        var basketItem = await basketRepository.GetFirstOrDefaultAsync(predicate: b => b.Id == basketItemId)
            ?? throw new NullReferenceException($"Basket item not found with ID '{basketItemId}'.");

        basketRepository.Remove(basketItem);
        await _shopping.SaveChangesAsync();
        _logger.LogInformation($"Basket item was deleted with ID: '{basketItem.Id}' from Basket with ID: '{basketItem.BasketId}'.");

        return basketItem;
    }

    public async Task TransferBasketAsync(string anonymousId, string userName)
    {
        Guard.Against.NullOrEmpty(anonymousId, nameof(anonymousId));
        Guard.Against.NullOrEmpty(userName, nameof(userName));

        var basketRepository = _shopping.GetRepository<Basket>();

        var anonymousBasket = await basketRepository.GetFirstOrDefaultAsync(
            predicate: i => i.BuyerId == anonymousId,
            include: query => query.Include(b => b.Items));

        if (anonymousBasket is not null)
        {
            var userBasket = await basketRepository.GetFirstOrDefaultAsync(
                predicate: i => i.BuyerId == userName,
                include: query => query.Include(b => b.Items));

            if (userBasket is null)
            {
                userBasket = new Basket(userName);
                basketRepository.Add(userBasket);
                await _shopping.SaveChangesAsync();
                _logger.LogInformation($"Create basket with ID '{userBasket.Id}'.");
            }

            userBasket.AddItems(anonymousBasket.Items);
            basketRepository.Update(userBasket);
            basketRepository.Remove(anonymousBasket);
            await _shopping.SaveChangesAsync();
            _logger.LogInformation($"Basket with ID '{anonymousBasket.Id}' " +
                $"was transferred to Basket with ID '{userBasket.Id}' successful.");
        };
    }
}