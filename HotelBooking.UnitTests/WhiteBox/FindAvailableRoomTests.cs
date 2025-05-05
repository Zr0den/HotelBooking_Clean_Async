using HotelBooking.Core;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HotelBooking.UnitTests.WhiteBox
{
    public class FindAvailableRoomTests
    {
        private readonly Mock<IRepository<Booking>> bookingRepoMock;
        private readonly Mock<IRepository<Room>> roomRepoMock;
        private readonly BookingManager bookingManager;

        public FindAvailableRoomTests()
        {
            bookingRepoMock = new Mock<IRepository<Booking>>();
            roomRepoMock = new Mock<IRepository<Room>>();
            bookingManager = new BookingManager(bookingRepoMock.Object, roomRepoMock.Object);
        }

        //Rød Path
        [Fact]
        public async Task FindAvailableRoom_ShouldThrow_WhenStartDateIsTodayOrEarlier()
        {
            var today = DateTime.Today;
            var startDate = today; // C1 TRUE
            var endDate = today.AddDays(2); // C2 FALSE

            await Assert.ThrowsAsync<ArgumentException>(() => bookingManager.FindAvailableRoom(startDate, endDate));
        }

        //Rød Path
        [Fact]
        public async Task FindAvailableRoom_ShouldThrow_WhenStartDateAfterOrEqualEndDate()
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(5); // C1 FALSE
            var endDate = today.AddDays(2); // C2 TRUE (startDate > endDate)

            await Assert.ThrowsAsync<ArgumentException>(() => bookingManager.FindAvailableRoom(startDate, endDate));
        }

        //Blå Path
        [Fact]
        public async Task FindAvailableRoom_ShouldNotThrow_WhenDatesAreValid()
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(2);
            var endDate = today.AddDays(4); // C1 FALSE, C2 FALSE

            bookingRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Booking>());
            roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room> { new Room { Id = 1 } });

            var roomId = await bookingManager.FindAvailableRoom(startDate, endDate);
            Assert.Equal(1, roomId); // some room id returned
        }

        //Blå Path
        [Fact]
        public async Task FindAvailableRoom_ShouldFindRoom_WhenBookingIsBeforeExisting()
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(1); // C3 TRUE (before existing booking)
            var endDate = today.AddDays(2);

            var existingBooking = new Booking { StartDate = today.AddDays(5), EndDate = today.AddDays(7), IsActive = true, RoomId = 1 };
            bookingRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Booking> { existingBooking });
            roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room> { new Room { Id = 1 } });

            var roomId = await bookingManager.FindAvailableRoom(startDate, endDate);
            Assert.Equal(1, roomId);
        }

        //Blå Path
        [Fact]
        public async Task FindAvailableRoom_ShouldFindRoom_WhenBookingIsAfterExisting()
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(10); // C4 TRUE (after existing booking)
            var endDate = today.AddDays(12);

            var existingBooking = new Booking { StartDate = today.AddDays(5), EndDate = today.AddDays(7), IsActive = true, RoomId = 1 };
            bookingRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Booking> { existingBooking });
            roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room> { new Room { Id = 1 } });

            var roomId = await bookingManager.FindAvailableRoom(startDate, endDate);
            Assert.Equal(1, roomId);
        }

        //Gul Path
        [Fact]
        public async Task FindAvailableRoom_ShouldReturnMinusOne_WhenOverlappingBookingExists()
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(6); // Overlaps with existing
            var endDate = today.AddDays(7);

            var existingBooking = new Booking { StartDate = today.AddDays(5), EndDate = today.AddDays(8), IsActive = true, RoomId = 1 };
            bookingRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Booking> { existingBooking });
            roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room> { new Room { Id = 1 } });

            var roomId = await bookingManager.FindAvailableRoom(startDate, endDate);
            Assert.Equal(-1, roomId); // no available room
        }

        //Gul Path - Loop Coverage 1 (0)
        [Fact]
        public async Task FindAvailableRoom_ShouldReturnMinusOne_WhenForeachEmpty()
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(6); // Overlaps with existing
            var endDate = today.AddDays(7);

            var existingBooking = new Booking { StartDate = today.AddDays(5), EndDate = today.AddDays(8), IsActive = true, RoomId = 1 };
            bookingRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Booking> { existingBooking });
            roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room> { });

            var roomId = await bookingManager.FindAvailableRoom(startDate, endDate);
            Assert.Equal(-1, roomId); // no available room
        }

        //Gul Path - Loop Coverage 2 (N)
        [Fact]
        public async Task FindAvailableRoom_ShouldReturnFirstRoom_WhenForeachContainsManyElements()
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(10);
            var endDate = today.AddDays(12);

            var existingBooking = new Booking { StartDate = today.AddDays(5), EndDate = today.AddDays(7), IsActive = true, RoomId = 1 };
            bookingRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Booking> { existingBooking });
            List<Room> liste = new List<Room> { new Room { Id = 1 }, new Room { Id = 2 }, new Room { Id = 3 }, new Room { Id = 4 } };
            liste.OrderBy(x => x.Id);
            roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(liste);

            var roomId = await bookingManager.FindAvailableRoom(startDate, endDate);
            Assert.Equal(1, roomId);
        }
    }
}
