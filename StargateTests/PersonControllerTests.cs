using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.ExceptionLogging;
using StargateAPI.Business.Queries;
using StargateAPI.Controllers;
using Xunit;

namespace StargateAPI.Tests
{
    public class PersonControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<StargateContext> _contextMock;
        private readonly Mock<ExceptionLogging> _exceptionLoggingMock;
        private readonly PersonController _controller;

        public PersonControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _contextMock = new Mock<StargateContext>();
            _exceptionLoggingMock = new Mock<ExceptionLogging>();
            _controller = new PersonController(_mediatorMock.Object, _contextMock.Object, _exceptionLoggingMock.Object);
        }

        [Fact]
        public async Task GetPeople_Returns_OkResult()
        {
            //var peopleResult = new GetPeopleResult { //insert data  };
            //_mediatorMock.Setup(x => x.Send(It.IsAny<GetPeople>(), default)).ReturnsAsync(peopleResult);

            //var result = await _controller.GetPeople();

            //var okResult = Assert.IsType<OkObjectResult>(result);
            //var returnedPeople = Assert.IsAssignableFrom<GetPeopleResult>(okResult.Value);
            //Assert.Equal(peopleResult, returnedPeople);
        }
    }
}
