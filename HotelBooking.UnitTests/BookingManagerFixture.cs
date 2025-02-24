using HotelBooking.Core;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.UnitTests
{
    public class BookingManagerFixture
    {
        public IBookingManager BookingManager { get; }
        public Mock<IRepository<Booking>> BookingRepository { get; }
        public Mock<IRepository<Room>> RoomRepository { get; }

        public BookingManagerFixture()
        {
            BookingRepository = new Mock<IRepository<Booking>>();
            RoomRepository = new Mock<IRepository<Room>>();
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

            RoomRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(rooms);
            BookingRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(bookings);

            BookingManager = new BookingManager(BookingRepository.Object, RoomRepository.Object);
        }
    }

}
