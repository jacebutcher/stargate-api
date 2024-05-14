using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.ExceptionLogging;
using StargateAPI.Business.Queries;
using StargateAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AstronautDutyController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly StargateContext _context;
    private readonly ExceptionLogging _exceptionLogging;

    public AstronautDutyController(IMediator mediator, StargateContext context, ExceptionLogging exceptionLogging)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator)); // handle null mediator
        _context = context ?? throw new ArgumentNullException(nameof(context)); // handle null context
        _exceptionLogging = exceptionLogging ?? throw new ArgumentNullException(nameof(exceptionLogging)); // handle null exceptionLogging
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> GetAstronautDutiesByName(string name)
    {

        if (string.IsNullOrWhiteSpace(name)) // validate parameter
        {
            // log and return exception
            await _exceptionLogging.LogAndReturnBadRequestAsync(new ArgumentException("Name length must be greater than 0."), _context);
            return BadRequest("Name length must be greater than 0.");
        }

        try
        {
            var result = await _mediator.Send(new GetAstronautDutiesByName
            {
                Name = name
            });

            return this.GetResponse(result);
        }
        catch (Exception ex)
        {
            // log and return exception
            await _exceptionLogging.LogAndReturnBadRequestAsync(ex, _context);
            return BadRequest("An error occurred while processing your request.");
        }
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateAstronautDuty([FromBody] CreateAstronautDuty request)
    {
        if (request == null) // parameter validation
        {
            await _exceptionLogging.LogAndReturnBadRequestAsync(new ArgumentException("Request body is null."), _context);
            return BadRequest("Request body is null.");
        }

        try
        {
            var result = await _mediator.Send(request);
            return this.GetResponse(result);
        }
        catch (Exception ex)
        {
            // log and return exception
            await _exceptionLogging.LogAndReturnBadRequestAsync(ex, _context);
            return BadRequest("An error occurred while creating astronaut duty.");
        }
    }
}
