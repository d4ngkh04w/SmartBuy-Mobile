using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public abstract class BaseController : ControllerBase
    {
    }

    [ApiController]
    [Route("api/v1/user/auth")]
    public abstract class BaseUserAuthController : ControllerBase
    {
    }

    [ApiController]
    [Route("api/v1/admin/auth")]
    public abstract class BaseAdminAuthController : ControllerBase
    {
    }
}
