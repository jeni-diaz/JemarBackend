using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jemar.Presentation.Controllers
{
    // Indica que esta clase es un controlador de API REST
    [ApiController]

    // Define la ruta base: api/shipment
    [Route("api/[controller]")]

    // Obliga a que todos los endpoints requieran autenticación
    [Authorize]
    public class ShipmentController : ControllerBase
    {
        // Servicio inyectado mediante Dependency Injection. Contiene la lógica de negocio de los envíos
        private readonly IShipmentService _shipmentService;

        // Obtiene el UserId guardado en HttpContext por el middleware de autenticación. Si no existe, devuelve Guid.Empty.
        private Guid UserId =>
            HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;

        //Obtiene el rol del usuario autenticado. Si no existe, devuelve string vacío.
        private string UserRole =>
            HttpContext.Items["userRole"] as string ?? string.Empty;

        // Constructor del controlador. ASP.NET Core inyecta automáticamente una implementación de IShipmentService.
        public ShipmentController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }


        // GET: api/shipment. Obtiene todos los envíos permitidos para el usuario
        [HttpGet]
        public async Task<ActionResult<List<ShipmentResponse>>> GetAll()
        {
            // Llama al servicio para obtener todos los envíos
            var shipments = await _shipmentService.GetAllAsync(UserId, UserRole);

            // Devuelve HTTP 200 con la lista de envíos
            return Ok(shipments);
        }


        // GET: api/shipment/{id}. Obtiene un envío específico por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ShipmentResponse>> GetById(Guid id)
        {
            try
            {
                // Busca el envío solicitado
                var shipment = await _shipmentService.GetByIdAsync(id, UserId, UserRole);

                // Si no existe devuelve 404
                if (shipment == null)
                    return NotFound();

                // Si existe devuelve 200 con los datos
                return Ok(shipment);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Si el usuario no tiene permiso devuelve 403
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
        }

        // POST: api/shipment. Crea un nuevo envío
        [HttpPost]
        public async Task<ActionResult<ShipmentResponse>> Create(
            CreateShipmentRequest request)
        {
            try
            {
                // Crea el envío usando los datos recibidos
                var shipment = await _shipmentService.CreateAsync(request, UserId);

                // Devuelve HTTP 200 con el envío creado
                return CreatedAtAction(nameof(GetById), new { id = shipment.Id }, shipment);
            }
            catch (ArgumentException ex)
            {
                // Si los datos son inválidos devuelve 400
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/shipment/{id}/status. Actualiza el estado de un envío
        // Ruta: PUT api/shipment/{id}/status
        [HttpPut("{id}/status")]

        // Solo empleados o roles superiores pueden acceder
        [Authorize(Policy = "EmployeeOrAbove")]
        public async Task<IActionResult> UpdateStatus(Guid id, UpdateShipmentRequest updateRequest)
        {
            try
            {
                // Solicita al servicio actualizar el estado
                var result =
                    await _shipmentService.UpdateStatusAsync(id, updateRequest, UserId, UserRole);

                // Si no encontró el envío devuelve 404
                if (!result)
                    return NotFound("Shipment not found");

                // Actualización exitosa. HTTP 204 = éxito sin devolver contenido
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                // Usuario sin permisos
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (ArgumentException ex)
            {
                // Error de validación
                return BadRequest(ex.Message);
            }
        }


        // DELETE: api/shipment/{id}. Elimina un envío
        // Ruta: DELETE api/shipment/{id}
        [HttpDelete("{id}")]

        // Solo empleados o superiores pueden eliminar
        [Authorize(Policy = "EmployeeOrAbove")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // Solicita al servicio eliminar el envío
                var result = await _shipmentService.DeleteAsync(id, UserId, UserRole);

                // Si el envío no existe devuelve 404
                if (!result)
                    return NotFound("Shipment not found");

                // Eliminación exitosa
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                // Usuario sin permisos
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (ArgumentException ex)
            {
                // Error de validación
                return BadRequest(ex.Message);
            }
        }
    }
}