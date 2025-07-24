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
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
         private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContext;

    public CartController(ApplicationDbContext context, IHttpContextAccessor httpContext)
    {
        _context = context;
        _httpContext = httpContext;
    }

    private Guid GetUserId() =>
        Guid.Parse(_httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = GetUserId();
        var items = await _context.CartItems
            .Where(c => c.UserId == userId)
            .Include(c => c.Product)
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(Guid productId, int quantity = 1)
    {
        var userId = GetUserId();
        var existing = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

        if (existing != null)
            existing.Quantity += quantity;
        else
            _context.CartItems.Add(new CartItem { UserId = userId, ProductId = productId, Quantity = quantity });

        await _context.SaveChangesAsync();
        return Ok("Cart updated");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveItem(Guid id)
    {
        var item = await _context.CartItems.FindAsync(id);
        if (item is null) return NotFound();

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetUserId();
        var items = _context.CartItems.Where(c => c.UserId == userId);
        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
        return NoContent();
    }
    }
}