using api.DTOs.ProductLine;
using api.Exceptions;
using api.Helpers;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;
using api.Queries;
using api.Utils;

namespace api.Services
{
    public class ProductLineService : IProductLineService
    {
        private readonly IProductLineRepository _productLineRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _env;
        private readonly ICacheService _cacheService;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);
        public ProductLineService(
            IProductLineRepository productLineRepository,
            IProductRepository productRepository,
            IBrandRepository brandRepository,
            IWebHostEnvironment webEnvironment,
            ICacheService cacheService)
        {
            _productLineRepository = productLineRepository;
            _brandRepository = brandRepository;
            _productRepository = productRepository;
            _env = webEnvironment;
            _cacheService = cacheService;
        }

        public async Task<ProductLineDTO> CreateProductLineAsync(CreateProductLineDTO productLineDTO)
        {
            if (productLineDTO.Image != null && !productLineDTO.Image.IsImage())
            {
                throw new BadRequestException("Invalid image format");
            }

            _ = await _brandRepository.GetBrandByIdAsync(productLineDTO.BrandId) ?? throw new NotFoundException("Brand not found");
            bool exists = await _productLineRepository.ProductLineExistAsync(productLineDTO.Name.Trim());
            if (exists)
            {
                throw new AlreadyExistsException("Product line already exists");
            }

            var productLine = productLineDTO.ToModel();
            if (productLineDTO.Image != null)
            {
                var filePath = await ImageUtils.SaveImageAsync(productLineDTO.Image, _env.WebRootPath, "product-lines", 5 * 1024 * 1024);
                productLine.Image = filePath;
            }

            var createdProductLine = await _productLineRepository.CreateProductLineAsync(productLine);

            _cacheService.RemoveAllProductLinesCache();

            return createdProductLine.ToDTO();
        }

        public async Task<ProductLineDTO> GetProductLineByIdAsync(int id, ProductLineQuery? query = null)
        {
            string cacheKey = CacheKeyManager.GetProductLineKey(id, query);

            if (_cacheService.TryGetValue(cacheKey, out ProductLineDTO? cachedProductLine) && cachedProductLine != null)
            {
                return cachedProductLine;
            }

            var productLine = await _productLineRepository.GetProductLineByIdAsync(id, query) ?? throw new NotFoundException("Product line not found");
            var productLineDto = productLine.ToDTO();

            _cacheService.Set(cacheKey, productLineDto, _cacheDuration);

            return productLineDto;
        }
        public async Task<IEnumerable<ProductLineDTO>> GetProductLinesAsync(ProductLineQuery query)
        {
            string cacheKey = CacheKeyManager.GetAllProductLinesKey(query);

            if (_cacheService.TryGetValue(cacheKey, out IEnumerable<ProductLineDTO>? cachedProductLines) && cachedProductLines != null)
            {
                return cachedProductLines;
            }

            var productLines = await _productLineRepository.GetProductLinesAsync(query);
            if (productLines == null || !productLines.Any())
            {
                throw new NotFoundException("Not found any product lines");
            }

            var productLineDtos = productLines.Select(pl => pl.ToDTO()).ToList();

            _cacheService.Set(cacheKey, productLineDtos, _cacheDuration);

            return productLineDtos;
        }

        public async Task<ProductLineDTO> UpdateProductLineAsync(int id, UpdateProductLineDTO productLineDTO)
        {
            var productLine = await _productLineRepository.GetProductLineByIdAsync(id) ?? throw new NotFoundException("Product line not found");

            var newName = productLineDTO.Name?.Trim();
            if (!string.IsNullOrEmpty(newName))
            {
                productLine.Name = newName;
            }

            if (!string.IsNullOrEmpty(productLineDTO.Description?.Trim()))
            {
                productLine.Description = productLineDTO.Description.Trim();
            }

            if (productLineDTO.BrandId.HasValue)
            {
                productLine.BrandId = productLineDTO.BrandId.Value;
            }

            if (productLineDTO.Image != null)
            {
                if (!productLineDTO.Image.IsImage())
                {
                    throw new BadRequestException("Invalid image format");
                }

                if (!string.IsNullOrEmpty(productLine.Image))
                {
                    await ImageUtils.DeleteImageAsync(_env.WebRootPath + productLine.Image);
                }
                var filePath = await ImageUtils.SaveImageAsync(productLineDTO.Image, _env.WebRootPath, "product-lines", 5 * 1024 * 1024);

                productLine.Image = filePath;
            }

            productLine.UpdatedAt = DateTime.Now;

            var result = await _productLineRepository.UpdateProductLineAsync(productLine);

            _cacheService.RemoveProductLineCache(id);
            _cacheService.RemoveAllProductLinesCache();

            return result.ToDTO();
        }

        public async Task<ProductLineDTO> ActivateProductLineAsync(int id)
        {
            var productLine = await _productLineRepository.GetProductLineByIdAsync(id) ?? throw new NotFoundException("Product line not found");
            var brand = await _brandRepository.GetBrandByIdAsync(productLine.BrandId) ?? throw new NotFoundException("Parent brand not found");
            if (!brand.IsActive)
                throw new BadRequestException("Cannot activate product line because parent brand is inactive");

            if (productLine.IsActive)
                return productLine.ToDTO();

            productLine.IsActive = true;
            productLine.ManuallyDeactivated = false;
            productLine.UpdatedAt = DateTime.Now;

            await _productLineRepository.UpdateProductLineAsync(productLine);

            var products = await _productRepository.GetProductsByProductLineIdAsync(id);
            if (products != null && products.Any())
            {
                foreach (var product in products)
                {
                    if (!product.IsActive && !product.ManuallyDeactivated)
                    {
                        product.IsActive = true;
                        product.UpdatedAt = DateTime.Now;
                        await _productRepository.UpdateAsync(product);
                    }
                }
            }

            _cacheService.RemoveProductLineCache(id);
            _cacheService.RemoveAllProductLinesCache();

            return productLine.ToDTO();
        }

        public async Task<ProductLineDTO> DeactivateProductLineAsync(int id)
        {
            var productLine = await _productLineRepository.GetProductLineByIdAsync(id) ?? throw new NotFoundException("Product line not found");
            if (!productLine.IsActive)
                return productLine.ToDTO();

            productLine.IsActive = false;
            productLine.ManuallyDeactivated = true;
            productLine.UpdatedAt = DateTime.Now;

            await _productLineRepository.UpdateProductLineAsync(productLine);

            var products = await _productRepository.GetProductsByProductLineIdAsync(id);
            if (products != null && products.Any())
            {
                foreach (var product in products)
                {
                    if (product.IsActive)
                    {
                        product.IsActive = false;
                        product.UpdatedAt = DateTime.Now;
                        await _productRepository.UpdateAsync(product);
                    }
                }
            }

            _cacheService.RemoveProductLineCache(id);
            _cacheService.RemoveAllProductLinesCache();

            return productLine.ToDTO();
        }
    }
}