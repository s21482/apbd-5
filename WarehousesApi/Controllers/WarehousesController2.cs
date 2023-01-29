using WarehousesApi.Models;
using WarehousesApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Warehouses2.Controllers
{
    [ApiController]
    [Route("api/warehouses2")]
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
            return Ok(await _dbService.AddProductToWarehouseByStoredProcedure(productWarehouse));
        }


    }
}
