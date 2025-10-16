using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseTransaction.Dto;
using PurchaseTransaction.Services;
using PurchaseTransaction.Utils;
using System.ComponentModel.DataAnnotations;

namespace PurchaseTransaction.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseController : ControllerBase
    {
        private readonly ILogger<PurchaseController> _logger;
        private readonly IPurchaseService _purchaseService;

        public PurchaseController(ILogger<PurchaseController> logger, IPurchaseService purchaseService)
        {
            _logger = logger;
            _purchaseService = purchaseService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(PurchaseResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAsync([FromBody] CreatePurchaseRequestDto createPurchaseRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            try
            {
                var result = await _purchaseService.CreateAsync(createPurchaseRequestDto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch(ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error creating purchase");
                return BadRequest(new { message = "Make sure you have Valid data" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the purchase.");
            }
        }

        [HttpGet("{id:guid}", Name = nameof(GetById))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(PurchaseResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] string? targetCurrencyCode = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(targetCurrencyCode))
                {
                    var result = await _purchaseService.GetAsync(id);
                    if (result == null)
                    {
                        return NotFound();
                    }
                    return Ok(result);
                }
                else
                {
                    var result = await _purchaseService.GetConvertedAsync(id, CountryCurrencyDescConverter.Normalize(targetCurrencyCode));
                    if (result == null)
                    {
                        return NotFound(new {message = "Purchase not found. "});
                    }
                    return Ok(result);
                }
            }
            catch (InvalidOperationException ex)
            {
                // This can happen if the target currency is invalid or if no exchange rate is found.
                _logger.LogWarning(ex, "Invalid operation while retrieving purchase with ID {PurchaseId}", id);
                return BadRequest(new {message = $"Invalid operation while retrieving purchase with ID {id}" });
            }
            catch (ArgumentException ex)
            {
                // This can happen if the target currency is invalid.
                _logger.LogWarning(ex, "Invalid argument while retrieving purchase with ID {PurchaseId}", id);
                return BadRequest(new { message = $"Invalid argument while retrieving purchase with ID {id}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase with ID {PurchaseId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the purchase.");
            }
        }

    }
}
