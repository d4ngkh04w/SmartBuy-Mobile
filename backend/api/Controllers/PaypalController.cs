using api.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    public class PaypalController : BaseController
    {
        private readonly IPaypalService _paypalService;

        public PaypalController(IPaypalService paypalService)
        {
            _paypalService = paypalService;
        }

        [HttpGet("token")]
        public async Task<IActionResult> GetToken()
        {
            var token = await _paypalService.GetAccessTokenAsync();
            return Ok(new { access_token = token });
        }
    }
}
