using System;
using HotelBooking.Core;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using System.Collections.Generic;

namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private IBookingManager bookingManager;
        private readonly Mock<IRepository<Booking>> bookingRepository;
        private readonly Mock<IRepository<Room>> roomRepository;

        public BookingManagerTests()
        {
            bookingRepository = new Mock<IRepository<Booking>>();
            roomRepository = new Mock<IRepository<Room>>();
            DateTime today = DateTime.Today;

            var bookings = new List<Booking>
        {
            new Booking { Id = 1, StartDate = today.AddDays(-4), EndDate = today.AddDays(-2), IsActive = false, RoomId = 2 },
            new Booking { Id = 2, StartDate = today.AddDays(-2), EndDate = today.AddDays(2), IsActive = true, RoomId = 1 },
            new Booking { Id = 3, StartDate = today.AddDays(3), EndDate = today.AddDays(5), IsActive = true, RoomId = 1 },
            new Booking { Id = 4, StartDate = today.AddDays(6), EndDate = today.AddDays(13), IsActive = true, RoomId = 1 },
            new Booking { Id = 5, StartDate = today.AddDays(14), EndDate = today.AddDays(16), IsActive = true, RoomId = 1 },
            new Booking { Id = 6, StartDate = today.AddDays(6), EndDate = today.AddDays(8), IsActive = true, RoomId = 2 },
            new Booking { Id = 7, StartDate = today.AddDays(9), EndDate = today.AddDays(11), IsActive = true, RoomId = 2 },
            new Booking { Id = 8, StartDate = today.AddDays(12), EndDate = today.AddDays(13), IsActive = true, RoomId = 2 }
        };

            var rooms = new List<Room>
            {
                new Room { Id = 1, Description = "A" },
                new Room { Id = 2, Description = "B" },
            };
            roomRepository.Setup(repo => repo.GetAllAsync())
                                  .ReturnsAsync(rooms);

            bookingRepository.Setup(repo => repo.GetAllAsync())
                                  .ReturnsAsync(bookings);

            bookingManager = new BookingManager(bookingRepository.Object, roomRepository.Object);
        }

        [Theory]
        [InlineData(1, 5, true)]
        [InlineData(1, 6, false)]
        [InlineData(2, 3, true)]
        [InlineData(2, 4, true)]
        [InlineData(6, 13, false)]
        [InlineData(5, 13, false)]
        [InlineData(5, 14, false)]
        [InlineData(9, 11, false)]
        [InlineData(14, 16, true)]
        [InlineData(17, 18, true)]
        [InlineData(1, 20, false)]
        public async Task CreateBooking_ReturnsCorrectValue_WithDifferentInput(int start, int end, bool expectedResult)
        {
            //Arrange
            DateTime today = DateTime.Today;
            DateTime startDate = today.AddDays(start);
            DateTime endDate = today.AddDays(end);
            Booking booking = new Booking { Id = 9, StartDate = today.AddDays(start), EndDate = today.AddDays(end) };

            //Act
            bool roomFound = await bookingManager.CreateBooking(booking);

            //Assert
            Assert.Equal(expectedResult, roomFound);
        }

        [Theory]
        [InlineData(1, 5, 2)]
        [InlineData(1, 6, -1)]
        [InlineData(2, 3, 2)]
        [InlineData(2, 4, 2)]
        [InlineData(6, 13, -1)]
        [InlineData(5, 13, -1)]
        [InlineData(5, 14, -1)]
        [InlineData(9, 11, -1)]
        [InlineData(14, 16, 2)]
        [InlineData(17, 18, 1)]
        [InlineData(1, 20, -1)]
        public async Task FindAvailableRoom_ReturnsCorrectValue_WithDifferentInput(int start, int end, int expected)
        {
            //Arrange
            DateTime today = DateTime.Today;
            DateTime startDate = today.AddDays(start);
            DateTime endDate = today.AddDays(end);

            //Act
            int roomId = await bookingManager.FindAvailableRoom(startDate, endDate);

            //Assert
            Assert.Equal(expected, roomId);
        }

        [Theory]
        [MemberData(nameof(GetFullyOccupiedDates_TestData))]
        public async Task GetFullyOccupiedDates_ReturnsCorrectValue_WithDifferentInput(int start, int end, List<DateTime> expectedDates)
        {
            // Arrange
            DateTime today = DateTime.Today;
            DateTime startDate = today.AddDays(start);
            DateTime endDate = today.AddDays(end);

            // Act
            List<DateTime> dates = await bookingManager.GetFullyOccupiedDates(startDate, endDate);

            // Assert
            Assert.Equal(expectedDates, dates);
        }

        public static IEnumerable<object[]> GetFullyOccupiedDates_TestData()
        {
            DateTime today = DateTime.Today;

            yield return new object[]
            {
                1, 5, new List<DateTime> {  }
            };

            yield return new object[]
            {
                6, 13, new List<DateTime>
                {   today.AddDays(6),
                    today.AddDays(7),
                    today.AddDays(8),
                    today.AddDays(9),
                    today.AddDays(10),
                    today.AddDays(11),
                    today.AddDays(12),
                    today.AddDays(13)
                }
            };

            yield return new object[]
            {
                14, 20, new List<DateTime> { }
            };
        }

        [Fact]
        public async Task FindAvailableRoom_StartDateNotInTheFuture_ThrowsArgumentException()
        {
            // Arrange
            DateTime date = DateTime.Today;

            // Act
            Task result() => bookingManager.FindAvailableRoom(date, date);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(result);
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);
            // Assert
            Assert.NotEqual(-1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_ReturnsAvailableRoom()
        {
            // This test was added to satisfy the following test design
            // principle: "Tests should have strong assertions".

            // Arrange
            DateTime date = DateTime.Today.AddDays(1);

            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);

            var bookingForReturnedRoomId = (await bookingRepository.Object.GetAllAsync()).
                Where(b => b.RoomId == roomId
                           && b.StartDate <= date
                           && b.EndDate >= date
                           && b.IsActive);

            // Assert
            Assert.Empty(bookingForReturnedRoomId);
        }

    }
}
