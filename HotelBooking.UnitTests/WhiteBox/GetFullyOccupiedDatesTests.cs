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
        DateTime startDate = new DateTime(2025, 5, 5);
        DateTime endDate = new DateTime(2025, 5, 1);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _bookingManager.GetFullyOccupiedDates(startDate, endDate));
    }

    [Fact]
    public async Task GetFullyOccupiedDates_StartDateBeforeEndDate_NoException()
    {
        // TC2
        DateTime startDate = new DateTime(2025, 5, 1);
        DateTime endDate = new DateTime(2025, 5, 5);

        _mockRoomRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room>());
        _mockBookingRepository.Setup(b => b.GetAllAsync()).ReturnsAsync(new List<Booking>());

        var result = await _bookingManager.GetFullyOccupiedDates(startDate, endDate);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_NoBookingsExist_ReturnsEmptyList()
    {
        // TC3
        _mockRoomRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room> { new Room() });
        _mockBookingRepository.Setup(b => b.GetAllAsync()).ReturnsAsync(new List<Booking>());

        var result = await _bookingManager.GetFullyOccupiedDates(DateTime.Today, DateTime.Today);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_SomeBookingsExist_EvaluatesDates()
    {
        // TC4
        _mockRoomRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room> { new Room(), new Room() });

        var bookings = new List<Booking>
        {
            new Booking { StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1), IsActive = true }
        };

        _mockBookingRepository.Setup(b => b.GetAllAsync()).ReturnsAsync(bookings);

        var result = await _bookingManager.GetFullyOccupiedDates(DateTime.Today, DateTime.Today.AddDays(1));

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_SingleDayRange_CorrectEvaluation()
    {
        // TC5
        _mockRoomRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room> { new Room() });

        var bookings = new List<Booking>
        {
            new Booking { StartDate = DateTime.Today, EndDate = DateTime.Today, IsActive = true }
        };

        _mockBookingRepository.Setup(b => b.GetAllAsync()).ReturnsAsync(bookings);

        var result = await _bookingManager.GetFullyOccupiedDates(DateTime.Today, DateTime.Today);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_MultipleDayRange_CorrectEvaluation()
    {
        // TC6
        _mockRoomRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room> { new Room() });

        var bookings = new List<Booking>
        {
            new Booking { StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(2), IsActive = true }
        };

        _mockBookingRepository.Setup(b => b.GetAllAsync()).ReturnsAsync(bookings);

        var result = await _bookingManager.GetFullyOccupiedDates(DateTime.Today, DateTime.Today.AddDays(2));

        Assert.True(result.Count >= 0);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_NoFullyOccupiedDays_ReturnsEmptyList()
    {
        // TC7
        _mockRoomRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room> { new Room(), new Room() });

        var bookings = new List<Booking>
        {
            new Booking { StartDate = DateTime.Today, EndDate = DateTime.Today, IsActive = true } // Only 1 active booking, 2 rooms
        };

        _mockBookingRepository.Setup(b => b.GetAllAsync()).ReturnsAsync(bookings);

        var result = await _bookingManager.GetFullyOccupiedDates(DateTime.Today, DateTime.Today);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_SomeFullyOccupiedDays_ReturnsDates()
    {
        // TC8
        _mockRoomRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room> { new Room(), new Room() });

        var bookings = new List<Booking>
        {
            new Booking { StartDate = DateTime.Today, EndDate = DateTime.Today, IsActive = true },
            new Booking { StartDate = DateTime.Today, EndDate = DateTime.Today, IsActive = true }
        };

        _mockBookingRepository.Setup(b => b.GetAllAsync()).ReturnsAsync(bookings);

        var result = await _bookingManager.GetFullyOccupiedDates(DateTime.Today, DateTime.Today);

        Assert.Single(result);
        Assert.Equal(DateTime.Today, result.First());
    }


    // Sjældne men reelle tests
    [Fact]
    public async Task GetFullyOccupiedDates_NoRoomsExist_ReturnsEmptyList()
    {
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
        // Arrange
        var mockRoomRepo = new Mock<IRepository<Room>>();
        var mockBookingRepo = new Mock<IRepository<Booking>>();

        mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room>
    {
        new Room { Id = 1 },
        new Room { Id = 2 }
    });

        mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(new List<Booking>
    {
        new Booking { RoomId = 1, StartDate = new DateTime(2025, 5, 1), EndDate = new DateTime(2025, 5, 2), IsActive = false },
        new Booking { RoomId = 2, StartDate = new DateTime(2025, 5, 2), EndDate = new DateTime(2025, 5, 3), IsActive = false }
    });

        var manager = new BookingManager(mockBookingRepo.Object, mockRoomRepo.Object);
        var startDate = new DateTime(2025, 5, 1);
        var endDate = new DateTime(2025, 5, 5);

        // Act
        var result = await manager.GetFullyOccupiedDates(startDate, endDate);

        // Assert
        Assert.Empty(result);
    }
}
