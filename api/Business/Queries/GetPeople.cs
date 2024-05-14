using System.Net;
using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetPeopleResult : BaseResponse
    {
        public List<PersonAstronaut> People { get; set; } = new List<PersonAstronaut> { };
    }

    public class GetPeople : IRequest<GetPeopleResult>
    {

    }

    public class GetPeopleHandler : IRequestHandler<GetPeople, GetPeopleResult>
    {
        public readonly StargateContext _context;
        private readonly ExceptionLogging.ExceptionLogging _exceptionLogging;
        public GetPeopleHandler(StargateContext context, ExceptionLogging.ExceptionLogging exceptionLogging)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // handle null context
            _exceptionLogging = exceptionLogging ?? throw new ArgumentNullException(nameof(exceptionLogging)); // handle null exceptionLogging
        }

        public async Task<GetPeopleResult> Handle(GetPeople request, CancellationToken cancellationToken)
        {
            var result = new GetPeopleResult();

            try
            {
                const string query = @"
                    SELECT a.ID as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate 
                    FROM Person a 
                    LEFT JOIN AstronautDetail b ON b.PersonID = a.ID
                    ORDER BY a.ID ASC"; // parameterize query, add ORDER BY for user sort

                var people = await _context.Connection.QueryAsync<PersonAstronaut>(query);

                result.People = people?.AsList() ?? new List<PersonAstronaut>(); // ensure people is not null, use AsList for better performance. return empty list if people is null with "??"

                return result;
            }
            catch (Exception ex)
            {
                await _exceptionLogging.SendExcepToDB(ex, _context);

                return new GetPeopleResult
                {
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occurred while retrieving people."
                };
            }
        }
    }
}
