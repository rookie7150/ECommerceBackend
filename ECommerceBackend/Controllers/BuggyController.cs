using Microsoft.AspNetCore.Mvc;

namespace ECommerceBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuggyController : ControllerBase
    {
        // 測試 1: 故意找不到資源 (404)
        [HttpGet("not-found")]
        public ActionResult GetNotFound()
        {
            return NotFound();
        }

        // 測試 2: 故意拋出伺服器錯誤 (500)
        [HttpGet("server-error")]
        public ActionResult GetServerError()
        {
            // 這裡模擬程式寫爛了，拋出例外
            throw new Exception("這是伺服器壞掉的測試錯誤！");
        }
    }
}