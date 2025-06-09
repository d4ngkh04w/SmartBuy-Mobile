using api.DTOs.Order;
using api.Exceptions;
using api.Helpers;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "admin", Roles = "admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return ApiResponseHelper.Success("Orders retrieved successfully", orders);
        }

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = "user", Roles = "user")]
        public async Task<IActionResult> GetUserOrders()
        {
            var userId = HttpContextHelper.CurrentUserId;
            if (userId == Guid.Empty)
                throw new UnauthorizedException();

            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            return ApiResponseHelper.Success("User orders retrieved successfully", orders);
        }

        [HttpGet("me-current")]
        [Authorize(AuthenticationSchemes = "user", Roles = "user")]
        public async Task<IActionResult> GetCurrentUserOrders()
        {
            var userId = HttpContextHelper.CurrentUserId;
            if (userId == Guid.Empty)
                throw new UnauthorizedException();

            var orders = await _orderService.GetCurrentOrdersByUserIdAsync(userId);
            return ApiResponseHelper.Success("User orders retrieved successfully", orders);
        }

        [HttpGet("{id:guid}")]
        [Authorize(AuthenticationSchemes = "smart", Roles = "admin,user")]
        public async Task<IActionResult> GetOrderById([FromRoute] Guid id)
        {
            // Kiểm tra xem người dùng có phải là admin hay không hoặc nếu người dùng đang truy cập đơn hàng của chính họ
            bool isAdmin = HttpContextHelper.CurrentUserRole == "admin";
            Guid userId = HttpContextHelper.CurrentUserId;

            var order = await _orderService.GetOrderByIdAsync(id);

            // Xác minh người dùng có quyền truy cập vào đơn hàng này hay không
            if (!isAdmin && order.UserId != userId)
            {
                throw new ForbiddenException();
            }
            return ApiResponseHelper.Success("Order retrieved successfully", order);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "user", Roles = "user")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO createOrderDTO)
        {
            var userId = HttpContextHelper.CurrentUserId;
            if (userId == Guid.Empty)
                return Unauthorized(new { Message = "User not authenticated" });

            var order = await _orderService.CreateOrderAsync(createOrderDTO, userId);
            return ApiResponseHelper.Created("Order created successfully", order);
        }

        [HttpPut("{id:guid}/status")]
        [Authorize(AuthenticationSchemes = "smart", Roles = "admin,user")]
        public async Task<IActionResult> UpdateOrderStatus([FromRoute] Guid id, [FromBody] UpdateOrderStatusDTO updateOrderStatusDTO)
        {
            bool isAdmin = HttpContextHelper.CurrentUserRole == "admin";
            Guid userId = HttpContextHelper.CurrentUserId;

            var order = await _orderService.GetOrderByIdAsync(id);

            if (!isAdmin && order.UserId != userId && !(updateOrderStatusDTO.Status == "Hoàn thành" || updateOrderStatusDTO.Status == "Đã trả hàng"))
            {
                throw new ForbiddenException();
            }
            order = await _orderService.UpdateOrderStatusAsync(id, updateOrderStatusDTO);
            return ApiResponseHelper.Success("Order status updated successfully", order);
        }

        [HttpPut("{id:guid}/cancel")]
        [Authorize(AuthenticationSchemes = "user", Roles = "user")]
        public async Task<IActionResult> CancelOrder([FromRoute] Guid id)
        {
            var userId = HttpContextHelper.CurrentUserId;
            if (userId == Guid.Empty)
                throw new UnauthorizedException();

            var order = await _orderService.CancelOrderAsync(id, userId);
            return ApiResponseHelper.Success("Order canceled successfully", order);
        }
    }
}