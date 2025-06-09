using api.DTOs.Order;

namespace api.Interfaces.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDTO>> GetAllOrdersAsync();
        Task<IEnumerable<OrderDTO>> GetOrdersByUserIdAsync(Guid userId);
        Task<IEnumerable<OrderDTO>> GetCurrentOrdersByUserIdAsync(Guid userId);
        Task<OrderDTO> GetOrderByIdAsync(Guid id);
        Task<OrderDTO> CreateOrderAsync(CreateOrderDTO orderDTO, Guid userId);
        Task<OrderDTO> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusDTO updateOrderStatusDTO);
        Task<OrderDTO> CancelOrderAsync(Guid id, Guid userId);
        Task<bool> HasUserPurchasedProductAsync(Guid userId, int productId);
    }
}