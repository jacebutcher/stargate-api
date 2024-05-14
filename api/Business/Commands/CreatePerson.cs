using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    public class CreatePerson : IRequest<CreatePersonResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class CreatePersonPreProcessor : IRequestPreProcessor<CreatePerson>
    {
        private readonly StargateContext _context;

        public CreatePersonPreProcessor(StargateContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // handle null context
        }
        public async Task Process(CreatePerson request, CancellationToken cancellationToken) // make method async for consistency and possible future I/O/data operations
        {
            var existingPerson = await _context.People.AsNoTracking().FirstOrDefaultAsync(z => z.Name == request.Name, cancellationToken: cancellationToken); // check for duplicates
            if (existingPerson != null)
            {
                throw new BadHttpRequestException("Person with the same name already exists."); // duplicate found
            }
        }
    }

    public class CreatePersonHandler : IRequestHandler<CreatePerson, CreatePersonResult>
    {
        private readonly StargateContext _context;
        private readonly ExceptionLogging.ExceptionLogging _exceptionLogging;

        public CreatePersonHandler(StargateContext context, ExceptionLogging.ExceptionLogging exceptionLogging)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // handle null context
            _exceptionLogging = exceptionLogging ?? throw new ArgumentNullException(nameof(exceptionLogging)); // handle null exceptionLogging
        }
        public async Task<CreatePersonResult> Handle(CreatePerson request, CancellationToken cancellationToken)
        {
            try
            {
                var newPerson = new Person // create new person with desired name
                {
                    Name = request.Name
                };

                _context.People.Add(newPerson);
                await _context.SaveChangesAsync(cancellationToken);

                return new CreatePersonResult // successful add operation
                {
                    Success = true,
                    ResponseCode = StatusCodes.Status201Created,
                    Message = "Person created successfully.",
                    Id = newPerson.Id
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogging.SendExcepToDB(ex, _context);

                return new CreatePersonResult
                {
                    Success = false,
                    ResponseCode = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred while creating the person."
                };
            }

        }
    }

    public class CreatePersonResult : BaseResponse
    {
        public int Id { get; set; }
    }
}
