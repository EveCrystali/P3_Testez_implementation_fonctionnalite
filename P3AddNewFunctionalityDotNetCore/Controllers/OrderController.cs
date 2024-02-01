using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;

namespace P3AddNewFunctionalityDotNetCore.Controllers
{
    public class OrderController : Controller
    {
        private readonly ICart _cart;
        private readonly IOrderService _orderService;
        private readonly IStringLocalizer<OrderController> _localizer;

        public OrderController(ICart cart, IOrderService service, IStringLocalizer<OrderController> localizer)
        {
            _cart = cart;
            _orderService = service;
            _localizer = localizer;
        }

        public ViewResult Index()
        {
            return View(new OrderViewModel());
        }

        [HttpPost]
        public IActionResult Index(OrderViewModel order)
        {
            if (!((Cart)_cart).Lines.Any())
            {

                ModelState.AddModelError("", _localizer["CartEmpty"]);
            }
            if (ModelState.IsValid && ((Cart)_cart).Lines.Any())
            {
                foreach(CartLine line in ((Cart)_cart).Lines)
                {
                    if (line.Product.Quantity <= _cart.GetProductQuantityInCart(line.Product))
                    {
                        
                        TempData["ErrorMessageProductNotAvailable"] = $"Unfortunately, your product {line.Product.Name} is out now out stock. We must have deleted it from your Cart."; // TODO Translation of the error message
                        return View(order);
                    }
                }
                order.Lines = (_cart as Cart)?.Lines.ToArray();
                _orderService.SaveOrder(order);
                return RedirectToAction(nameof(Completed));
                
            }
            if (!ModelState.IsValid && ((Cart)_cart).Lines.Any())
            {
                return View(order);
            }
            else
            {
                return View(order);
            }

        }
            public ViewResult Completed()
        {
            _cart.Clear();
            return View();
        }
    }
}
