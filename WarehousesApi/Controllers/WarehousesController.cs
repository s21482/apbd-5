using WarehousesApi.Models;
using WarehousesApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Warehouses.Controllers
{
    [ApiController]
    [Route("api/warehouses")]
    public class WarehousesController : ControllerBase
    {
        private readonly IDatabaseService _dbService;

        public WarehousesController(IDatabaseService dbService)
        {
            _dbService = dbService;
        }


        [HttpPost]
        public async Task<IActionResult> PostProductWarehouse(ProductWarehouse productWarehouse)
        {
            var response = await _dbService.AddProductToWarehouse(productWarehouse);
            switch (response.Status)
            {
                case 200:
                    return Ok(response.Message);
                case 400:
                    return BadRequest(response.Message);
                case  404:
                    return NotFound(response.Message);
                default:
                    return StatusCode(500);
            }
        }


    }
}
