﻿namespace PerfumeShop.Infrastructure.Services;

public class BasketService : IBasketService
{
    private readonly IUnitOfWork<ShoppingDbContext> _unitOfWork;
    private readonly ILogger<BasketService> _logger;


    public BasketService(
        IUnitOfWork<ShoppingDbContext> unitOfWork,
        ILogger<BasketService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Basket> AddItemToBasketAsync(string userId, int productId, int quantity = 1)
    {
        var basketRepository = _unitOfWork.GetRepository<Basket>();
        var basket = await basketRepository.GetFirstOrDefaultAsync(
            predicate: i => i.BuyerId == userId,
            include: query => query.Include(b => b.Items));

        if (basket is null)
        {
            basket = new Basket(userId);
            basketRepository.Add(basket);

            _logger.LogInformation($"Basket was created for user ID: '{userId}'.");
        }

        basket.AddItem(productId, quantity);

        basketRepository.Update(basket);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation($"Basket was updated with ID: '{basket.Id}'.");

        return basket;
    }

    public async Task DeleteBasketAsync(int basketId)
    {
        var basketRepository = _unitOfWork.GetRepository<Basket>();
        var basket = await basketRepository.GetFirstOrDefaultAsync(predicate: b => b.Id == basketId)
            ?? throw new NullReferenceException($"Basket not found with ID '{basketId}'.");

        basketRepository.Remove(basket);

        _logger.LogInformation($"Basket was deleted with ID: '{basket.Id}'.");

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveItemFromBasketAsync(int basketItemId)
    {
        var basketRepository = _unitOfWork.GetRepository<BasketItem>();
        var basketItem = await basketRepository.GetFirstOrDefaultAsync(predicate: b => b.Id == basketItemId)
            ?? throw new NullReferenceException($"Basket item not found with ID '{basketItemId}'.");

        basketRepository.Remove(basketItem);

        _logger.LogInformation($"Basket item was deleted with ID: '{basketItem.Id}' from Basket with ID: '{basketItem.BasketId}'.");

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task TransferBasketAsync(string anonymousId, string userId)
    {
        Guard.Against.NullOrEmpty(anonymousId, nameof(anonymousId));
        Guard.Against.NullOrEmpty(userId, nameof(userId));

        var basketRepository = _unitOfWork.GetRepository<Basket>();

        var anonymousBasket = await basketRepository.GetFirstOrDefaultAsync(
            predicate: i => i.BuyerId == anonymousId,
            include: query => query.Include(b => b.Items));

        if (anonymousBasket is not null)
        {
            var userBasket = await basketRepository.GetFirstOrDefaultAsync(
                predicate: i => i.BuyerId == userId,
                include: query => query.Include(b => b.Items));

            if (userBasket is null)
            {
                userBasket = new Basket(userId);
                basketRepository.Add(userBasket);
                _logger.LogInformation($"Basket was created for user with ID: {userId}");
            }

            foreach (var item in anonymousBasket.Items)
            {
                userBasket.AddItem(item.Id);
                _logger.LogInformation($"Item was added with ID: {item.Id}");
            }

            basketRepository.Update(userBasket);
            _logger.LogInformation($"Basket was successfully transferred");

            basketRepository.Remove(anonymousBasket);
            _logger.LogInformation($"Anonymous basket with ID: {anonymousBasket.Id} was removed.");

            await _unitOfWork.SaveChangesAsync();           
        };
    }
}