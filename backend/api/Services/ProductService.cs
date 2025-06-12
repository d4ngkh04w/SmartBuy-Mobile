using api.DTOs.Product;
using api.Exceptions;
using api.Helpers;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;
using api.Models;
using api.Queries;
using api.Utils;

namespace api.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductLineRepository _productLineRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IWebHostEnvironment _env;
        private readonly ICacheService _cacheService;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);
        public ProductService(IProductRepository productRepository,
                              IProductLineRepository productLineRepository,
                              ICommentRepository commentRepository,
                              IWebHostEnvironment webHostEnvironment,
                              ICacheService cacheService)
        {
            _productRepository = productRepository;
            _productLineRepository = productLineRepository;
            _commentRepository = commentRepository;
            _env = webHostEnvironment;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsAsync(bool? isActive = null)
        {
            string cacheKey = CacheKeyManager.GetAllProductsKey();
            bool hasFilters = isActive.HasValue;
            if (!hasFilters && _cacheService.TryGetValue(cacheKey, out IEnumerable<ProductDTO>? cachedProducts) && cachedProducts != null)
            {
                return cachedProducts;
            }
            var products = await _productRepository.GetAllAsync();
            if (!products.Any())
                return new List<ProductDTO>();
            if (isActive.HasValue)
            {
                products = products.Where(p => p.IsActive == isActive.Value);
            }
            var productDtos = products.Select(p => p.ToProductDTO()).ToList();
            foreach (var productDto in productDtos)
            {
                productDto.Rating = (decimal)await _commentRepository.GetProductAverageRatingAsync(productDto.Id);
                productDto.RatingCount = await _commentRepository.GetProductRatingCountAsync(productDto.Id);
            }
            if (!hasFilters)
            {
                _cacheService.Set(cacheKey, productDtos, _cacheDuration);
            }
            return productDtos;
        }

        public async Task<ProductDTO> GetProductByIdAsync(int id)
        {
            string cacheKey = CacheKeyManager.GetProductKey(id);

            if (_cacheService.TryGetValue(cacheKey, out ProductDTO? cachedProduct) && cachedProduct != null)
            {
                return cachedProduct;
            }

            var product = await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product not found");
            var productDto = await product.ToProductDTO(_commentRepository);

            _cacheService.Set(cacheKey, productDto, _cacheDuration);

            return productDto;
        }

        public async Task<ProductDTO> CreateProductAsync(CreateProductDTO productDTO)
        {
            if (await _productRepository.ExistsByNameAsync(productDTO.Name.Trim()))
                throw new AlreadyExistsException("Product with this name already exists");
            else
                productDTO.Name = productDTO.Name.Trim();

            var productLine = await _productLineRepository.GetProductLineByIdAsync(productDTO.ProductLineId) ?? throw new NotFoundException($"Product line with ID {productDTO.ProductLineId} not found");
            if (!productLine.IsActive)
                throw new BadRequestException($"Product line with ID {productDTO.ProductLineId} is inactive");

            var product = productDTO.ToProductModel();
            product.Detail = productDTO.ToProductDetailModel();

            var createdProduct = await _productRepository.CreateAsync(product);
            _cacheService.RemoveAllProductsCache();

            return await createdProduct.ToProductDTO(_commentRepository);
        }
        public async Task<ProductDTO> UpdateProductAsync(int id, UpdateProductDTO dto)
        {
            var product = await _productRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Product not found");

            // Cập nhật thông tin cơ bản
            if (!string.IsNullOrWhiteSpace(dto.Name)) product.Name = dto.Name.Trim();
            if (dto.ImportPrice.HasValue) product.ImportPrice = dto.ImportPrice.Value;
            if (dto.SalePrice.HasValue) product.SalePrice = dto.SalePrice.Value;
            if (!string.IsNullOrWhiteSpace(dto.Description)) product.Description = dto.Description.Trim();
            if (dto.ProductLineId.HasValue) product.ProductLineId = dto.ProductLineId.Value;

            // Cập nhật chi tiết sản phẩm
            if (product.Detail != null)
            {
                if (int.TryParse(dto.Warranty, out var warranty)) product.Detail.WarrantyMonths = warranty;
                if (int.TryParse(dto.RAM, out var ram)) product.Detail.RAMInGB = ram;
                if (int.TryParse(dto.Storage, out var storage)) product.Detail.StorageInGB = storage;
                if (decimal.TryParse(dto.ScreenSize, out var screenSize)) product.Detail.ScreenSizeInch = screenSize;

                if (!string.IsNullOrWhiteSpace(dto.ScreenResolution)) product.Detail.ScreenResolution = dto.ScreenResolution.Trim();
                if (!string.IsNullOrWhiteSpace(dto.Battery) && int.TryParse(dto.Battery, out var battery)) product.Detail.BatteryCapacityMAh = battery;
                if (!string.IsNullOrWhiteSpace(dto.OS)) product.Detail.OperatingSystem = dto.OS.Trim();
                if (!string.IsNullOrWhiteSpace(dto.Processor)) product.Detail.Processor = dto.Processor.Trim();
                if (dto.SimSlots.HasValue) product.Detail.SimSlots = dto.SimSlots.Value;
            }

            var updated = await _productRepository.UpdateAsync(product);
            _cacheService.RemoveProductCache(id);
            _cacheService.RemoveAllProductsCache();

            return await updated.ToProductDTO(_commentRepository);
        }

        public async Task<ProductColorDTO> UpdateProductColorAsync(int productId, int colorId, UpdateColorDTO dto)
        {
            // Kiểm tra định dạng ảnh hợp lệ
            if (dto.AddImages?.Any(i => !i.IsImage()) == true)
                throw new BadRequestException("Invalid image format");

            // Lấy dữ liệu liên quan
            _ = await _productRepository.GetByIdAsync(productId) ?? throw new NotFoundException("Product not found");
            var color = await _productRepository.GetProductColorAsync(productId, colorId) ?? throw new NotFoundException("Product color not found");

            // Cập nhật tên và số lượng
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                var trimmedName = dto.Name.Trim();
                if (color.Name == trimmedName)
                    throw new AlreadyExistsException($"Color with name {trimmedName} already exists for this product");
                color.Name = trimmedName;
            }
            if (dto.Quantity.HasValue) color.Quantity = dto.Quantity.Value;

            // Xử lý xóa ảnh
            bool wasMainDeleted = false;
            if (dto.RemoveImageIds?.Any() == true)
            {
                var imagesToDelete = color.Images.Where(i => dto.RemoveImageIds.Contains(i.Id)).ToList();

                wasMainDeleted = imagesToDelete.Any(i => i.IsMain);

                await Task.WhenAll(imagesToDelete.Select(img =>
                    ImageUtils.DeleteImageAsync(_env.WebRootPath + img.ImagePath)
                ));

                foreach (var img in imagesToDelete)
                {
                    color.Images.Remove(img);
                }
            }

            // Cập nhật ảnh chính
            if (dto.MainImageId.HasValue)
            {
                var newMain = color.Images.FirstOrDefault(i => i.Id == dto.MainImageId.Value)
                    ?? throw new NotFoundException($"Image with ID {dto.MainImageId.Value} not found");

                foreach (var img in color.Images) img.IsMain = false;
                newMain.IsMain = true;

            }
            else if (wasMainDeleted && color.Images.Any())
            {
                color.Images.First().IsMain = true;
            }

            // Thêm ảnh mới
            if (dto.AddImages?.Any() == true)
            {
                if (dto.MainImageIndex.HasValue)
                {
                    foreach (var img in color.Images) img.IsMain = false;
                }
                var tasks = dto.AddImages.Select(async (file, i) =>
                {
                    var path = await ImageUtils.SaveImageAsync(file, _env.WebRootPath, "products", 5 * 1024 * 1024);
                    color.Images.Add(new ProductImage
                    {
                        ImagePath = path,
                        IsMain = dto.MainImageIndex == i,
                        ColorId = color.Id
                    });
                });
                await Task.WhenAll(tasks);
            }

            // Đảm bảo có ảnh chính
            if (!color.Images.Any(i => i.IsMain) && color.Images.Any())
                color.Images.First().IsMain = true;

            var updated = await _productRepository.UpdateColorAsync(color);
            _cacheService.RemoveProductCache(productId);
            _cacheService.RemoveAllProductsCache();

            return updated.ToProductColorDTO();
        }


        public async Task DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product not found");
            await _productRepository.DeleteAsync(product);
            _cacheService.RemoveProductCache(id);
            _cacheService.RemoveAllProductsCache();
        }
        public async Task<ProductPagiDTO> GetPagedProductsAsync(ProductQuery productQuery)
        {
            var (items, totalItems) = await _productRepository.GetPagedProductsAsync(productQuery);

            if (items == null || !items.Any())
            {
                return new ProductPagiDTO
                {
                    TotalItems = 0,
                    Items = new List<ProductSummaryDTO>()
                };
            }

            var productSummaries = new List<ProductSummaryDTO>();
            foreach (var product in items)
            {
                var dto = await product.ToSummaryDTO(_commentRepository);
                productSummaries.Add(dto);
            }

            var result = new ProductPagiDTO
            {
                TotalItems = totalItems,
                Items = productSummaries.ToList()
            };

            return result;
        }

        public async Task<ProductColorDTO> CreateProductColorAsync(int productId, CreateColorDTO productColorDTO)
        {
            if (productColorDTO.Images != null && productColorDTO.Images.Any())
            {
                foreach (var image in productColorDTO.Images)
                {
                    if (!image.IsImage())
                        throw new BadRequestException("Invalid image format");
                }
            }

            var product = await _productRepository.GetByIdAsync(productId) ?? throw new NotFoundException("Product not found");

            var productColor = new ProductColor
            {
                Name = productColorDTO.Name.Trim(),
                Quantity = productColorDTO.Quantity,
                ProductId = product.Id
            };

            var savedColor = await _productRepository.AddColorAsync(productColor);

            if (productColorDTO.Images != null && productColorDTO.Images.Any())
            {
                // Xác định vị trí ảnh chính
                int mainImageIndex = productColorDTO.MainImageIndex ?? 0;

                // Đảm bảo index nằm trong phạm vi hợp lệ
                if (mainImageIndex >= productColorDTO.Images.Count)
                {
                    mainImageIndex = 0;
                }
                var productImageTasks = productColorDTO.Images.Select(async (image, i) =>
                {
                    var filePath = await ImageUtils.SaveImageAsync(image, _env.WebRootPath, "products", 5 * 1024 * 1024);
                    return new ProductImage
                    {
                        ImagePath = filePath,
                        IsMain = i == mainImageIndex,
                        ColorId = savedColor.Id
                    };
                });
                var productImages = await Task.WhenAll(productImageTasks);
                await _productRepository.AddImagesAsync(productImages);
            }

            _cacheService.RemoveProductCache(productId);
            _cacheService.RemoveAllProductsCache();

            return savedColor.ToProductColorDTO();
        }

        public async Task<ProductDTO> ActivateProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product not found");
            var productLine = await _productLineRepository.GetProductLineByIdAsync(product.ProductLineId) ?? throw new NotFoundException("Product line not found");

            if (!productLine.IsActive)
                throw new BadRequestException("Cannot activate product because parent product line is inactive");

            if (product.IsActive)
                throw new BadRequestException("Product is already active");

            product.IsActive = true;
            product.ManuallyDeactivated = false;
            product.UpdatedAt = DateTime.Now;

            var result = await _productRepository.UpdateAsync(product);

            _cacheService.RemoveProductCache(id);
            _cacheService.RemoveAllProductsCache();

            return await result.ToProductDTO(_commentRepository);
        }

        public async Task<ProductDTO> DeactivateProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product not found");
            if (!product.IsActive)
                return await product.ToProductDTO(_commentRepository);

            product.IsActive = false;
            product.ManuallyDeactivated = true;
            product.UpdatedAt = DateTime.Now;

            var result = await _productRepository.UpdateAsync(product);

            _cacheService.RemoveProductCache(id);
            _cacheService.RemoveAllProductsCache();

            return result.ToProductDTO();
        }
        public async Task DeleteProductColorAsync(int productId, int colorId)
        {
            _ = await _productRepository.GetByIdAsync(productId) ?? throw new NotFoundException("Product not found");
            var productColor = await _productRepository.GetProductColorAsync(productId, colorId) ?? throw new NotFoundException("Product color not found");

            await Task.WhenAll(productColor.Images.Select(img =>
                ImageUtils.DeleteImageAsync(_env.WebRootPath + img.ImagePath)
            ));

            await _productRepository.DeleteColorAsync(productColor);

            _cacheService.RemoveProductCache(productId);
            _cacheService.RemoveAllProductsCache();
        }
    }
}