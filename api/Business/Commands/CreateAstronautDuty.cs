using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
    {
        public required string Name { get; set; }
        public required string Rank { get; set; }
        public required string DutyTitle { get; set; }
        public DateTime DutyStartDate { get; set; }
    }

    public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyPreProcessor(StargateContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // make method async for consistency and possible future I/O/data operations
        }

        public async Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            var person = await _context.People.FirstOrDefaultAsync(z => z.Name == request.Name)
                ?? throw new BadHttpRequestException("Exception: Person not found."); // check for person

            //var previousDuty = await _context.AstronautDuties.FirstOrDefaultAsync(z => z.DutyTitle == request.DutyTitle && z.DutyStartDate == request.DutyStartDate)
                //?? throw new BadHttpRequestException("Exception: Specified astronaut has duplicate duty."); // check for duplicates TODO: debug this always throwing exception
        }
    }

    public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
    {
        private readonly StargateContext _context;
        private readonly ExceptionLogging.ExceptionLogging _exceptionLogging;

        public CreateAstronautDutyHandler(StargateContext context, ExceptionLogging.ExceptionLogging exceptionLogging)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // handle null context
            _exceptionLogging = exceptionLogging ?? throw new ArgumentNullException(nameof(exceptionLogging)); // handle null exceptionLogging
        }
        public async Task<CreateAstronautDutyResult> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            try
            {
                // Retrieve person based on name
                var person = await _context.People.FirstOrDefaultAsync(z => z.Name == request.Name, cancellationToken)
                    ?? throw new InvalidOperationException("Person not found."); // this should never happen because of the Process method's validation

                var astronautDetail = await _context.AstronautDetails.FirstOrDefaultAsync(z => z.PersonId == person.Id, cancellationToken); // determine add or update
                if (astronautDetail == null) // add
                {
                    astronautDetail = new AstronautDetail
                    {
                        PersonId = person.Id,
                        CurrentDutyTitle = request.DutyTitle,
                        CurrentRank = request.Rank,
                        CareerStartDate = request.DutyStartDate.Date,
                        CareerEndDate = request.DutyTitle == "RETIRED" ? request.DutyStartDate.Date : null
                    };
                    _context.AstronautDetails.Add(astronautDetail);
                }
                else // update
                {
                    astronautDetail.CurrentDutyTitle = request.DutyTitle;
                    astronautDetail.CurrentRank = request.Rank;
                    astronautDetail.CareerEndDate = request.DutyTitle == "RETIRED" ? request.DutyStartDate.Date : null;
                    _context.AstronautDetails.Update(astronautDetail);
                }

                var previousDuty = await _context.AstronautDuties.FirstOrDefaultAsync(z => z.PersonId == person.Id && z.DutyEndDate == null, cancellationToken); // update previous end date (see rules)
                if (previousDuty != null)
                {
                    previousDuty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;
                    _context.AstronautDuties.Update(previousDuty);
                }

                var newAstronautDuty = new AstronautDuty // create duty
                {
                    PersonId = person.Id,
                    Rank = request.Rank,
                    DutyTitle = request.DutyTitle,
                    DutyStartDate = request.DutyStartDate.Date,
                    DutyEndDate = null
                };
                _context.AstronautDuties.Add(newAstronautDuty);

                await _context.SaveChangesAsync(cancellationToken); // send to db

                return new CreateAstronautDutyResult
                {
                    Id = newAstronautDuty.Id
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogging.SendExcepToDB(ex, _context);

                return new CreateAstronautDutyResult
                {
                    Success = false,
                    ResponseCode = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred while creating the duty."
                };
            }
        }
    }

    public class CreateAstronautDutyResult : BaseResponse
    {
        public int? Id { get; set; }
    }
}
