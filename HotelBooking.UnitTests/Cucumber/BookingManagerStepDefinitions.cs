using HotelBooking.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Reqnroll;
using Moq;
using HotelBooking.Infrastructure.Repositories;

namespace HotelBooking.UnitTests.Cucumber
{
    [Binding]
    public class BookingCreationSteps
    {
        private readonly BookingManagerFixture fixture;
        private Booking booking;
        private bool bookingCreated;
        private Exception exception;

        public BookingCreationSteps(BookingManagerFixture fixture)
        {
            this.fixture = fixture;
            this.booking = new Booking();
        }

        [Given(@"the booking start date is ""(.*)""")]
        public void GivenTheBookingStartDateIs(string startDate)
        {
            booking.StartDate = DateTime.Parse(startDate);
        }

        [Given(@"the booking end date is ""(.*)""")]
        public void GivenTheBookingEndDateIs(string endDate)
        {
            booking.EndDate = DateTime.Parse(endDate);
        }

        [Given(@"the booking object is null")]
        public void GivenTheBookingObjectIsNull()
        {
            booking = null;
        }

        [Given(@"a room is available for the specified dates")]
        public void GivenARoomIsAvailableForTheSpecifiedDates()
        {
            // Mocking that a room is available for the specified dates
            fixture.RoomRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Room>
        {
            new Room { Id = 1, Description = "Room 1" },
            new Room { Id = 2, Description = "Room 2" }
        });
        }

        [Given(@"only 1 room is available for the specified dates")]
        public void GivenOnlyOneRoomIsAvailableForTheSpecifiedDates()
        {
            var rooms = new List<Room>
            {
                 new Room { Id = 1, Description = "The Last Room" } 
            };

            fixture.RoomRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(rooms);
        }

        [Given(@"no room is available for the specified dates")]
        public void GivenNoRoomIsAvailableForTheSpecifiedDates()
        {
            // Mocking that no room is available for the specified dates
            fixture.RoomRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Room>());
        }

        [When(@"the user creates the booking")]
        public async Task WhenTheUserCreatesTheBooking()
        {
            try
            {
                bookingCreated = await fixture.BookingManager.CreateBooking(booking);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }

        //Same as [When(@"the user creates the booking")] but worded to indicate it's going to fail
        [When(@"the user tries to create the booking")]
        public async Task WhenTheUserTriesToCreateTheBooking()
        {
            try
            {
                bookingCreated = await fixture.BookingManager.CreateBooking(booking);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }

        [Then(@"the booking should be created successfully")]
        public void ThenTheBookingShouldBeCreatedSuccessfully()
        {
            Assert.True(bookingCreated);
        }

        [Then(@"the booking should not be created")]
        public void ThenTheBookingShouldNotBeCreated()
        {
            Assert.False(bookingCreated);
        }

        [Then(@"the booking should throw an ArgumentException")]
        public void ThenTheBookingShouldThrowAnArgumentException()
        {
            Assert.IsType<ArgumentException>(exception);
        }

        [Then(@"the booking should throw a NullReferenceException")]
        public void ThenTheBookingShouldThrowANullReferenceException()
        {
            Assert.IsType<NullReferenceException>(exception);
        }
    }

}
