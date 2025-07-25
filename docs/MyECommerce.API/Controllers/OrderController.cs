using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyECommerce.API.Data;
using MyECommerce.Domain.Entities;

namespace MyECommerce.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContext;

    public OrderController(ApplicationDbContext context, IHttpContextAccessor httpContext)
    {
        _context = context;
        _httpContext = httpContext;
    }

    private Guid GetUserId() =>
        Guid.Parse(_httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout()
    {
        var userId = GetUserId();
        var cartItems = await _context.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
            return BadRequest("Cart is empty");

        var order = new Order
        {
            UserId = userId,
            Items = cartItems.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity
            }).ToList()
        };

        _context.Orders.Add(order);
        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        return Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var userId = GetUserId();
        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .ToListAsync();

        return Ok(orders);
    }
    }
}