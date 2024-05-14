using System.Net;
using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetAstronautDutiesByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
        public List<AstronautDuty> AstronautDuties { get; set; } = new List<AstronautDuty>();
    }

    public class GetAstronautDutiesByName : IRequest<GetAstronautDutiesByNameResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GetAstronautDutiesByNameHandler : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
    {
        private readonly StargateContext _context;
        private readonly ExceptionLogging.ExceptionLogging _exceptionLogging;
        public GetAstronautDutiesByNameHandler(StargateContext context, ExceptionLogging.ExceptionLogging exceptionLogging)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // handle null context
            _exceptionLogging = exceptionLogging ?? throw new ArgumentNullException(nameof(exceptionLogging)); // handle null exceptionLogging
        }

        public async Task<GetAstronautDutiesByNameResult> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {
            try
            {
                var result = new GetAstronautDutiesByNameResult();

                var personQuery = @"
                    SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate 
                    FROM Person a 
                    LEFT JOIN AstronautDetail b on b.PersonId = a.Id 
                    WHERE a.Name = @Name"; // get person info

                var personParameters = new { request.Name };
                var person = await _context.Connection.QueryFirstOrDefaultAsync<PersonAstronaut>(personQuery, personParameters);

                if (person == null) // no people found
                {
                    return new GetAstronautDutiesByNameResult
                    {
                        Success = false,
                        ResponseCode = (int)HttpStatusCode.NotFound,
                        Message = "No astronaut found with that name."
                    };
                }

                result.Person = person;

                var dutiesQuery = @"
                    SELECT * 
                    FROM AstronautDuty 
                    WHERE PersonId = @PersonId 
                    ORDER BY DutyStartDate DESC"; // get duties

                var dutiesParameters = new { person.PersonId };
                var duties = await _context.Connection.QueryAsync<AstronautDuty>(dutiesQuery, dutiesParameters);

                result.AstronautDuties = duties.AsList();

                return result;
            }
            catch (Exception ex)
            {
                await _exceptionLogging.SendExcepToDB(ex, _context);

                return new GetAstronautDutiesByNameResult
                {
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occurred while retrieving astronaut duties."
                };
            }
        }
    }
}