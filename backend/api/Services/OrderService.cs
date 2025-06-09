using api.DTOs.Order;
using api.Exceptions;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;
using api.Models;

namespace api.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICartRepository _cartRepository;
        private readonly ICacheService _cacheService;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ICartRepository cartRepository,
            ICacheService cacheService)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _cartRepository = cartRepository;
            _cacheService = cacheService;
        }

        public async Task<OrderDTO> CreateOrderAsync(CreateOrderDTO orderDTO, Guid userId)
        {
            if (orderDTO.Items == null || !orderDTO.Items.Any())
            {
                throw new BadRequestException("Order must contain at least one item");
            }

            // Tạo order mới
            var order = new Order
            {
                UserId = userId,
                Status = "Chờ xác nhận",
                PaymentMethod = orderDTO.PaymentMethod,
                OrderDate = DateTime.Now,
                OrderItems = new List<OrderItem>()
            };

            decimal totalAmount = 0;
            int totalItems = 0;

            foreach (var item in orderDTO.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new NotFoundException($"Product with ID {item.ProductId} not found");

                if (!product.IsActive)
                    throw new BadRequestException($"Product '{product.Name}' is no longer available");

                var color = product.Colors.FirstOrDefault(c => c.Id == item.ColorId);
                if (color == null)
                    throw new NotFoundException($"Color with ID {item.ColorId} not found for product '{product.Name}'");

                if (color.Quantity < item.Quantity)
                    throw new BadRequestException($"Not enough quantity available for product '{product.Name}' in the selected color");

                var productDto = product.ToProductDTO();
                // Tạo order item
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    ColorId = item.ColorId,
                    Quantity = item.Quantity,
                    Price = productDto.Price
                };

                // Update total
                totalAmount += orderItem.Price * orderItem.Quantity;
                totalItems += item.Quantity;

                // Add item vào order
                order.OrderItems.Add(orderItem);

                color.Quantity -= item.Quantity;
                await _productRepository.UpdateAsync(product);
                _cacheService.RemoveProductCache(product.Id);
            }

            decimal shippingFee = CalculateShippingFee(totalAmount);
            order.ShippingFee = shippingFee;

            // Set tổng tiền cho order
            order.TotalAmount = totalAmount + shippingFee;

            // Lưu order
            var createdOrder = await _orderRepository.CreateOrderAsync(order);

            // Xóa item trong giỏ hàng của người dùng sau khi đặt hàng thông qua giỏ hàng
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart != null)
            {
                foreach (var item in orderDTO.Items)
                {
                    var cartItem = cart.Items.FirstOrDefault(ci => ci.ProductId == item.ProductId && ci.ColorId == item.ColorId);
                    if (cartItem != null)
                    {
                        await _cartRepository.RemoveCartItemAsync(cartItem);
                    }
                }
            }

            _cacheService.RemoveAllProductsCache();

            return createdOrder.ToOrderDTO();
        }

        private static decimal CalculateShippingFee(decimal orderTotal)
        {
            decimal baseShippingFee = 30_000; // Phí vận chuyển cơ bản
            decimal discountThreshold = 5_000_000; // Giảm 50% phí vận chuyển cho đơn hàng trên 5 triệu
            decimal freeShippingThreshold = 10_000_000; // Miễn phí vận chuyển cho đơn hàng trên 10 triệu

            if (orderTotal >= freeShippingThreshold)
                return 0;

            // Phí vận chuyển cơ bản
            decimal shippingFee = baseShippingFee;

            // Giảm 50% phí vận chuyển nếu đơn hàng trên ngưỡng giảm giá
            if (orderTotal >= discountThreshold && orderTotal < freeShippingThreshold)
            {
                shippingFee *= 0.5m;
            }

            return shippingFee;
        }

        public async Task<OrderDTO> CancelOrderAsync(Guid id, Guid userId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id) ?? throw new NotFoundException("Order not found");

            if (order.UserId != userId)
            {
                throw new ForbiddenException("You don't have permission to cancel this order");
            }

            if (order.Status != "Chờ xác nhận" && order.Status != "Đã xác nhận")
            {
                throw new BadRequestException($"Order cannot be canceled because it is in '{order.Status}' status. Only orders in 'Chờ xác nhận' or 'Đã xác nhận' status can be canceled.");
            }

            order.Status = "Đã huỷ";

            foreach (var item in order.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    var color = product.Colors.FirstOrDefault(c => c.Id == item.ColorId);
                    if (color != null)
                    {
                        color.Quantity += item.Quantity;
                        await _productRepository.UpdateAsync(product);
                        _cacheService.RemoveProductCache(product.Id);
                    }
                }
            }

            var updatedOrder = await _orderRepository.UpdateOrderAsync(order);
            _cacheService.RemoveAllProductsCache();

            return updatedOrder.ToOrderDTO();
        }

        public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            if (orders == null || !orders.Any())
            {
                throw new NotFoundException("Not found any orders");
            }

            return orders.Select(o => o.ToOrderDTO());
        }

        public async Task<OrderDTO> GetOrderByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id) ?? throw new NotFoundException("Order not found");
            return order.ToOrderDTO();
        }

        public async Task<IEnumerable<OrderDTO>> GetOrdersByUserIdAsync(Guid userId)
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            if (orders == null || !orders.Any())
            {
                throw new NotFoundException("Not found any orders for you");
            }

            return orders.Select(o => o.ToOrderDTO());
        }
        public async Task<IEnumerable<OrderDTO>> GetCurrentOrdersByUserIdAsync(Guid userId)
        {
            var orders = await _orderRepository.GetCurrentOrdersByUserIdAsync(userId);
            if (orders == null || !orders.Any())
            {
                throw new NotFoundException("Not found any orders for you");
            }

            return orders.Select(o => o.ToOrderDTO());
        }
        public async Task<OrderDTO> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusDTO updateOrderStatusDTO)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id) ?? throw new NotFoundException("Order not found");

            // Kiểm tra trạng thái hiện tại và trạng thái mới có hợp lệ không
            if (!IsValidStatusTransition(order.Status, updateOrderStatusDTO.Status))
            {
                throw new BadRequestException($"Invalid status transition from '{order.Status}' to '{updateOrderStatusDTO.Status}'");
            }

            // Update order status
            order.Status = updateOrderStatusDTO.Status;

            if (updateOrderStatusDTO.Status == "Đã giao hàng")
            {
                order.DeliveryDate = updateOrderStatusDTO.DeliveryDate ?? DateTime.Now;
            }
            // Nếu đơn hàng chuyển sang trạng thái "Hoàn thành", tăng số lượng đã bán của sản phẩm
            else if (updateOrderStatusDTO.Status == "Hoàn thành")
            {
                foreach (var item in order.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Sold += item.Quantity;
                        await _productRepository.UpdateAsync(product);
                        _cacheService.RemoveProductCache(product.Id);
                    }
                }
                _cacheService.RemoveAllProductsCache();
            }
            else if (updateOrderStatusDTO.Status == "Đã trả hàng" || updateOrderStatusDTO.Status == "Đã huỷ")
            {
                foreach (var item in order.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        var color = product.Colors.FirstOrDefault(c => c.Id == item.ColorId);
                        if (color != null)
                        {
                            color.Quantity += item.Quantity;
                            await _productRepository.UpdateAsync(product);
                            _cacheService.RemoveProductCache(product.Id);
                        }
                    }
                }
                _cacheService.RemoveAllProductsCache();
            }

            var result = await _orderRepository.UpdateOrderAsync(order);

            return result.ToOrderDTO();
        }
        private static bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            Dictionary<string, List<string>> validTransitions = new Dictionary<string, List<string>>
            {
                ["Chờ xác nhận"] = new List<string> { "Đã xác nhận", "Đã huỷ" },
                ["Đã xác nhận"] = new List<string> { "Đang giao hàng", "Đã huỷ" },
                ["Đang giao hàng"] = new List<string> { "Đã giao hàng" },
                ["Đã giao hàng"] = new List<string> { "Hoàn thành", "Đã trả hàng" },
                ["Hoàn thành"] = new List<string>(),
                ["Đã huỷ"] = new List<string>(),
                ["Đã hoàn tiền"] = new List<string>(),
                ["Đã trả hàng"] = new List<string> { "Đã hoàn tiền" }
            };

            return validTransitions.ContainsKey(currentStatus) && validTransitions[currentStatus].Contains(newStatus);
        }

        public async Task<bool> HasUserPurchasedProductAsync(Guid userId, int productId)
        {
            var completedOrders = await _orderRepository.GetOrdersByUserIdAsync(userId);

            if (completedOrders == null || !completedOrders.Any())
                return false;

            var purchasedProduct = completedOrders
                .Where(o => o.Status == "Hoàn thành")
                .SelectMany(o => o.OrderItems)
                .Any(item => item.ProductId == productId);

            return purchasedProduct;
        }
    }
}