using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;

namespace StargateAPI.Business.ExceptionLogging
{
    public class ExceptionLogging
    {

        public async Task LogAndReturnBadRequestAsync(Exception ex, StargateContext context)
        {
            // Log the BadRequest scenario to the database
            await SendExcepToDB(ex, context);
        }

        public async Task SendExcepToDB(Exception exdb, StargateContext context)
        {
            var stackCut = exdb.StackTrace;

            if (stackCut != null && stackCut.Length > 2500)
            {
                stackCut = stackCut[..2499];
            }
            try
            {
                var exceptionLog = new API_Exceptions
                {
                    ExceptionMsg = exdb.Message.ToString(),
                    ExceptionType = exdb.GetType().Name.ToString(),
                    ExceptionURL = exdb.HelpLink ?? "",
                    ExceptionSource = stackCut ?? "",
                    Logdate = DateTime.Now
                };

                context.Exceptions.Add(exceptionLog);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while logging exception: " + ex.Message); // log secondary exception
                throw;
            }
        }
    }
}
