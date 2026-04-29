using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Models;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryItemController : ControllerBase
    {
        private readonly InventoryItemService _service;

        public InventoryItemController(InventoryItemService service)
        {
            _service = service;
        }
    }
}