using System.Net;
using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.ExceptionLogging;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetPersonByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
    }

    public class GetPersonByName : IRequest<GetPersonByNameResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class GetPersonByNameHandler : IRequestHandler<GetPersonByName, GetPersonByNameResult>
    {
        private readonly StargateContext _context;
        private readonly ExceptionLogging.ExceptionLogging _exceptionLogging;
        public GetPersonByNameHandler(StargateContext context, ExceptionLogging.ExceptionLogging exceptionLogging)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // handle null context
            _exceptionLogging = exceptionLogging ?? throw new ArgumentNullException(nameof(exceptionLogging)); // handle null exceptionLogging
        }
        public async Task<GetPersonByNameResult> Handle(GetPersonByName request, CancellationToken cancellationToken)
        {
            var result = new GetPersonByNameResult();

            try
            {
                string query = @"
                    SELECT a.Id As PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate 
                    FROM Person a 
                    LEFT JOIN AstronautDetail b ON a.Id = b.PersonId 
                    WHERE a.Name = @Name"; // parameterize query

                var parameters = new { request.Name };
                var person = await _context.Connection.QueryFirstOrDefaultAsync<PersonAstronaut>(query, parameters); // return first row, pass in Name as only parameter

                if (person == null) // successful call, but no person found
                {
                    await _exceptionLogging.SendExcepToDB(new Exception("No person found with that name."), _context); // log exception, don't throw
                    result.Success = false;
                    result.ResponseCode = (int)HttpStatusCode.NotFound;
                    result.Message = "No person found with that name.";
                    return result;
                }

                result.Person = person;

                return result;
            }
            catch (Exception ex)
            {
                await _exceptionLogging.SendExcepToDB(ex, _context);

                return new GetPersonByNameResult
                {
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occurred while retrieving person."
                };
            }
        }
    }
}
