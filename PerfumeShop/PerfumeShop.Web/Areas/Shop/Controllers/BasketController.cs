﻿namespace PerfumeShop.Web.Areas.Shop.Controllers;

[Area("Shop")]
public class BasketController : Controller
{
    private readonly IBasketService _basketService;
    private readonly IBasketViewModelService _basketViewModelService;

    public BasketController(
        IBasketService basketService,
        IBasketViewModelService basketViewModelService)
    {
        _basketService = basketService;
        _basketViewModelService = basketViewModelService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddToBasket(int productId, int quantity = 1)
    {
        var userName = GetBuyerId();

        var availabilityVM = await _basketViewModelService.BasketToStockRatio(userName, productId, quantity);
        if (availabilityVM.IsAvailable)
        {
            await _basketService.AddItemToBasketAsync(userName, productId, quantity);

            TempData["success"] = $"Product \"{availabilityVM.ProductName}\"" +
                $" added to cart in quantity {quantity}.";
        }
        else
        {
            TempData["error"] = $"Product \"{availabilityVM.ProductName}\"" +
                $" already added to cart in quantity {availabilityVM.BasketQty - quantity}." +
                $" You can add no more than {availabilityVM.StockQty}.";
        }
        return Redirect(Request.GetTypedHeaders().Referer.ToString());
    }

    private string GetBuyerId()
    {
        string? userName = null;

        if (Request.HttpContext.User.Identity.IsAuthenticated)
        {
            return Request.HttpContext.User.Identity.Name;
        }
        if (Request.Cookies.ContainsKey(Constants.BasketCookie))
        {
            userName = Request.Cookies[Constants.BasketCookie];

            if (!Request.HttpContext.User.Identity.IsAuthenticated 
                && !Guid.TryParse(userName, out var _))
            {
                userName = null;                
            }
        }
        if (userName != null) return userName;

        userName = Guid.NewGuid().ToString();

        Response.Cookies.Append(Constants.BasketCookie, userName,
            new CookieOptions 
            {
                IsEssential = true,
                Expires = DateTime.Today.AddYears(1)
            });

        return userName;
    }
}
