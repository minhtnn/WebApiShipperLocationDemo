using DeliveryVHGP.Data;
using DeliveryVHGP.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliveryVHGP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShippersControllerAsync : ControllerBase
    {
        private readonly ILogger<ShippersControllerAsync> _logger;
        private readonly ICacheService _cache;
        private readonly ShipperContext _context;

        public ShippersControllerAsync(ILogger<ShippersControllerAsync> logger, ICacheService cache, ShipperContext context)
        {
            _logger = logger;
            _cache = cache;
            _context = context;
        }

        [HttpGet("shippers")]
        public async Task<IActionResult> Get()
        {
            //check cache data
            var cacheData = _cache.GetData<IEnumerable<Shipper>>("shippers");
            if (cacheData != null && cacheData.Count() > 0)
            {
                return Ok(cacheData);
            }
            cacheData = await _context.Shippers.ToListAsync();
            //Set expiryTime
            var expiryTime = DateTimeOffset.Now.AddSeconds(30);
            _cache.SetData<IEnumerable<Shipper>>("shippers", cacheData, expiryTime);
            return Ok(cacheData);
        }

        [HttpPost("AddShipper")]
        public async Task<IActionResult> Post(Shipper shipper)
        {
            var addedObj = await _context.Shippers.AddAsync(shipper);
            var expiryTime = DateTimeOffset.Now.AddSeconds(30);
            _cache.SetData<Shipper>($"shipper{shipper.Id}", addedObj.Entity, expiryTime);

            await _context.SaveChangesAsync();
            return Ok(addedObj.Entity);
        }

        [HttpDelete("DeleteShipper")]
        public async Task<IActionResult> Delete(int id)
        {
            var exist = await _context.Shippers.SingleOrDefaultAsync(x => x.Id == id);
            if (exist != null)
            {
                _context.Shippers.Remove(exist);
                _cache.RemoveData($"shipper{id}");
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NoContent();
        }

        //private readonly IShipperRepository _shipperRepo;

        //public ShippersControllerAsync(IShipperRepository repo)
        //{
        //    _shipperRepo = repo;
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetAllShipper()
        //{
        //    try
        //    {
        //        return Ok(await _shipperRepo.GetAllShippersAsync());
        //    }
        //    catch
        //    {
        //        return BadRequest();
        //    }
        //}

        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetShipperById(int id)
        //{
        //    var shipper = await _shipperRepo.GetShippersAsyncById(id);
        //    return shipper == null ? NotFound() : Ok(shipper);
        //}

        //[HttpPost]
        //public async Task<IActionResult> AddNewShipperLocation(ShipperModel shipper)
        //{
        //    try
        //    {
        //        var newShipperId = await _shipperRepo.AddShipperAsync(shipper);
        //        var newShipper = await _shipperRepo.GetShippersAsyncById(newShipperId);
        //        return newShipper == null ? NotFound() : Ok(newShipper);
        //    }
        //    catch
        //    {
        //        return BadRequest();
        //    }
        //}

        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateShipperLocation(int id, ShipperModel shipper)
        //{
        //    try
        //    {
        //        await _shipperRepo.UpdateShipperAsync(id, shipper);
        //        return Ok();
        //    }
        //    catch
        //    {
        //        return BadRequest();
        //    }
        //}

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteShipperLocation([FromRoute] int id)
        //{
        //    try
        //    {
        //        var check = await _shipperRepo.DeleteShipperAsync(id);
        //        return check ? Ok() : NotFound();
        //    }
        //    catch
        //    {
        //        return BadRequest();
        //    }

        //}
    }
}
