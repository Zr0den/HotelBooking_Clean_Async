using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using HotelBooking.Core; // Adjust namespace if needed

public class BookingManagerTests
{
    private readonly Mock<IRepository<Booking>> _mockBookingRepository;
    private readonly Mock<IRepository<Room>> _mockRoomRepository;
    private readonly BookingManager _bookingManager;

    public BookingManagerTests()
    {
        _mockBookingRepository = new Mock<IRepository<Booking>>();
        _mockRoomRepository = new Mock<IRepository<Room>>();
        _bookingManager = new BookingManager(_mockBookingRepository.Object, _mockRoomRepository.Object);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_StartDateAfterEndDate_ThrowsException()
    {
        // TC1
        // Arrange
        var mockRoomRepo = new Mock<IRepository<Room>>();
        var mockBookingRepo = new Mock<IRepository<Booking>>();

        DateTime startDate = new DateTime(2025, 5, 5);
        DateTime endDate = new DateTime(2025, 5, 1);
        var manager = new BookingManager(mockBookingRepo.Object, mockRoomRepo.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
        manager.GetFullyOccupiedDates(startDate, endDate));
    }

    [Fact]
    public async Task GetFullyOccupiedDates_StartDateBeforeEndDate_NoException()
    {
        // TC2
        // Arrange
        var mockRoomRepo = new Mock<IRepository<Room>>();
        var mockBookingRepo = new Mock<IRepository<Booking>>();

        DateTime startDate = new DateTime(2025, 5, 1);
        DateTime endDate = new DateTime(2025, 5, 5);

        mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room>());
        mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(new List<Booking>());
        var manager = new BookingManager(mockBookingRepo.Object, mockRoomRepo.Object);

        // Act
        var result = await manager.GetFullyOccupiedDates(startDate, endDate);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_NoBookingsExist_ReturnsEmptyList()
    {
        // TC3
        // Arrange
        var mockRoomRepo = new Mock<IRepository<Room>>();
        var mockBookingRepo = new Mock<IRepository<Booking>>();

        var roomList = new List<Room> 
        { 
            new Room(),
            new Room()
        };

        mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(roomList);
        mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(new List<Booking>());
        var manager = new BookingManager(mockBookingRepo.Object, mockRoomRepo.Object);

        // Act
        var result = await manager.GetFullyOccupiedDates(DateTime.Today, DateTime.Today);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_SomeBookingsExist_EvaluatesDatesCorrectly()
    {
        // TC4
        // Arrange
        var mockRoomRepo = new Mock<IRepository<Room>>();
        var mockBookingRepo = new Mock<IRepository<Booking>>();

        DateTime startDate = new DateTime(2025, 5, 1);
        DateTime endDate = new DateTime(2025, 5, 3);

        var roomList = new List<Room> 
        { 
            new Room(), 
            new Room() 
        };

        var booking = new List<Booking>{new Booking 
        { 
            StartDate = new DateTime(2025, 5, 1), 
            EndDate = new DateTime(2025, 5, 2), 
            IsActive = true, 
            RoomId = 1 
        }};

        mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(roomList);
        mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(booking);
        var manager = new BookingManager(mockBookingRepo.Object, mockRoomRepo.Object);

        // Act
        var result = await manager.GetFullyOccupiedDates(startDate, endDate);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_SingleDayRange_ProcessesCorrectly()
    {
        // TC5
        // Arrange
        var mockRoomRepo = new Mock<IRepository<Room>>();
        var mockBookingRepo = new Mock<IRepository<Booking>>();

        var roomList = new List<Room>
        {
        new Room { Id = 1 },
        new Room { Id = 2 }
        };

        var bookingList = new List<Booking>
        {
        new Booking { StartDate = DateTime.Today, EndDate = DateTime.Today, RoomId = 1, IsActive = true },
        new Booking { StartDate = DateTime.Today, EndDate = DateTime.Today, RoomId = 2, IsActive = true }
        };

        mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(roomList);
        mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(bookingList);

        var manager = new BookingManager(mockBookingRepo.Object, mockRoomRepo.Object);
        DateTime startDate = DateTime.Today;
        DateTime endDate = DateTime.Today;

        // Act
        var result = await manager.GetFullyOccupiedDates(startDate, endDate);

        // Assert
        Assert.Single(result);
        Assert.Equal(DateTime.Today, result.First());
    }

    [Fact]
    public async Task GetFullyOccupiedDates_MultipleDayRange_ProcessesAllDays()
    {
        // TC6
        // Arrange
        var mockRoomRepo = new Mock<IRepository<Room>>();
        var mockBookingRepo = new Mock<IRepository<Booking>>();
        var roomList = new List<Room>
        {
        new Room { Id = 1 },
        new Room { Id = 2 }
        };

        var bookingList = new List<Booking>
        {
        new Booking { StartDate = new DateTime(2025, 5, 1), EndDate = new DateTime(2025, 5, 2), RoomId = 1, IsActive = true },
        new Booking { StartDate = new DateTime(2025, 5, 1), EndDate = new DateTime(2025, 5, 2), RoomId = 2, IsActive = true }
        };

        mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(roomList);
        mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(bookingList);

        var manager = new BookingManager(mockBookingRepo.Object, mockRoomRepo.Object);
        DateTime startDate = new DateTime(2025, 5, 1);
        DateTime endDate = new DateTime(2025, 5, 3);  // Extends one more day to test different outcomes

        // Act
        var result = await manager.GetFullyOccupiedDates(startDate, endDate);

        // Assert
        Assert.Equal(2, result.Count);  // May 1 and May 2 fully occupied, May 3 is not
        Assert.Contains(new DateTime(2025, 5, 1), result);
        Assert.Contains(new DateTime(2025, 5, 2), result);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_NoFullyOccupiedDays_ReturnsEmptyList()
    {
        // TC7
        // Arrange
        var mockRoomRepo = new Mock<IRepository<Room>>();
        var mockBookingRepo = new Mock<IRepository<Booking>>();

        var roomList = new List<Room>
        {
        new Room { Id = 1 },
        new Room { Id = 2 },
        new Room { Id = 3 }
        };

        var bookingList = new List<Booking>
        {
        new Booking { StartDate = new DateTime(2025, 5, 1), EndDate = new DateTime(2025, 5, 1), RoomId = 1, IsActive = true },
        new Booking { StartDate = new DateTime(2025, 5, 2), EndDate = new DateTime(2025, 5, 2), RoomId = 2, IsActive = true }
        };

        mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(roomList);
        mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(bookingList);

        var manager = new BookingManager(mockBookingRepo.Object, mockRoomRepo.Object);
        DateTime startDate = new DateTime(2025, 5, 1);
        DateTime endDate = new DateTime(2025, 5, 3);

        // Act
        var result = await manager.GetFullyOccupiedDates(startDate, endDate);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_SomeFullyOccupiedDays_ReturnsCorrectDates()
    {
        // TC8
        // Arrange
        var mockRoomRepo = new Mock<IRepository<Room>>();
        var mockBookingRepo = new Mock<IRepository<Booking>>();

        var roomList = new List<Room>
        {
        new Room { Id = 1 },
        new Room { Id = 2 },
        new Room { Id = 3 }
        };

        var bookingList = new List<Booking>
        {
        new Booking { StartDate = new DateTime(2025, 5, 1), EndDate = new DateTime(2025, 5, 3), RoomId = 1, IsActive = true },
        new Booking { StartDate = new DateTime(2025, 5, 1), EndDate = new DateTime(2025, 5, 3), RoomId = 2, IsActive = true },
        new Booking { StartDate = new DateTime(2025, 5, 2), EndDate = new DateTime(2025, 5, 2), RoomId = 3, IsActive = true }
        };

        mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(roomList);
        mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(bookingList);

        var manager = new BookingManager(mockBookingRepo.Object, mockRoomRepo.Object);

        DateTime startDate = new DateTime(2025, 5, 1);
        DateTime endDate = new DateTime(2025, 5, 5);

        // Act
        var result = await manager.GetFullyOccupiedDates(startDate, endDate);

        // Assert
        Assert.Single(result);  // Only May 2 is fully occupied
        Assert.Equal(new DateTime(2025, 5, 2), result.First());
    }




    // Sjældne men reelle tests
    [Fact]
    public async Task GetFullyOccupiedDates_NoRoomsExist_ReturnsEmptyList()
    {
        // TC9
        // Arrange
        var mockRoomRepo = new Mock<IRepository<Room>>();
        var mockBookingRepo = new Mock<IRepository<Booking>>();


        mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room>());
        mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(new List<Booking>());

        var manager = new BookingManager(mockBookingRepo.Object, mockRoomRepo.Object);
        var startDate = new DateTime(2025, 5, 1);
        var endDate = new DateTime(2025, 5, 5);

        // Act
        var result = await manager.GetFullyOccupiedDates(startDate, endDate);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_AllBookingsInactive_ReturnsEmptyList()
    {
        // TC10
        // Arrange
        var mockRoomRepo = new Mock<IRepository<Room>>();
        var mockBookingRepo = new Mock<IRepository<Booking>>();

        var roomList = new List<Room>
        {
        new Room { Id = 1 },
        new Room { Id = 2 }
        };

        var bookingList = new List<Booking>
        {
        new Booking 
        { 
            RoomId = 1, 
            StartDate = new DateTime(2025, 5, 1), 
            EndDate = new DateTime(2025, 5, 2), 
            IsActive = false 
        },
        new Booking 
        { 
            RoomId = 2, 
            StartDate = new DateTime(2025, 5, 2), 
            EndDate = new DateTime(2025, 5, 3), 
            IsActive = false 
        }};

        mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(roomList);
        mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(bookingList);

        var manager = new BookingManager(mockBookingRepo.Object, mockRoomRepo.Object);
        var startDate = new DateTime(2025, 5, 1);
        var endDate = new DateTime(2025, 5, 5);

        // Act
        var result = await manager.GetFullyOccupiedDates(startDate, endDate);

        // Assert
        Assert.Empty(result);
    }
}
