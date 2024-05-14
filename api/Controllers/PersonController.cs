using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.ExceptionLogging;
using StargateAPI.Business.Queries;
using System.Net;

namespace StargateAPI.Controllers
{
   
    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly StargateContext _context;
        private readonly ExceptionLogging _exceptionLogging;
        public PersonController(IMediator mediator, StargateContext context, ExceptionLogging exceptionLogging)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator)); // handle null mediator
            _context = context ?? throw new ArgumentNullException(nameof(context)); // handle null context
            _exceptionLogging = exceptionLogging ?? throw new ArgumentNullException(nameof(exceptionLogging)); // handle null exceptionLogging
        }

        [HttpGet("")]
        public async Task<IActionResult> GetPeople()
        {
            try
            {
                var result = await _mediator.Send(new GetPeople());
                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                // log and return exception
                await _exceptionLogging.LogAndReturnBadRequestAsync(ex, _context);
                return BadRequest("Error returning people.");
            }
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetPersonByName(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name)) // validate name before going further
                {
                    // log and return exception
                    await _exceptionLogging.LogAndReturnBadRequestAsync(new ArgumentException("Name length must be greater than 0."), _context);
                    return BadRequest("Name length must be greater than 0.");
                }

                var result = await _mediator.Send(new GetPersonByName() { Name = name });
                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                // log and return exception
                await _exceptionLogging.LogAndReturnBadRequestAsync(ex, _context);
                return BadRequest("Error returning people.");
            }
        }

        [HttpPost("")]
        public async Task<IActionResult> CreatePerson([FromBody] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name)) // validate name before going further
                {
                    // log and return exception
                    await _exceptionLogging.LogAndReturnBadRequestAsync(new ArgumentException("Name length must be greater than 0."), _context);
                    return BadRequest("Name length must be greater than 0.");
                }

                var result = await _mediator.Send(new CreatePerson() { Name = name });
                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                // log and return exception
                await _exceptionLogging.LogAndReturnBadRequestAsync(ex, _context);
                return BadRequest("Error creating person.");
            }
        }
    }
}