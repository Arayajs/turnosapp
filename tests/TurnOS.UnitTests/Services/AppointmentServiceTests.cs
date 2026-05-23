using FluentAssertions;
using Moq;
using TurnOS.Application.DTOs.Appointment;
using TurnOS.Application.Services;
using TurnOS.Domain.Entities;
using TurnOS.Domain.Enums;
using TurnOS.Domain.Interfaces;

namespace TurnOS.UnitTests.Services;

public class AppointmentServiceTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepoMock;
    private readonly Mock<IServiceRepository> _serviceRepoMock;
    private readonly Mock<IBusinessRepository> _businessRepoMock;
    private readonly Mock<IEmailService> _emailMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly AppointmentService _sut;

    private static readonly Guid BusinessId = Guid.NewGuid();
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid ServiceId = Guid.NewGuid();
    private static readonly Guid ClientId = Guid.NewGuid();

    public AppointmentServiceTests()
    {
        _appointmentRepoMock = new Mock<IAppointmentRepository>();
        _serviceRepoMock = new Mock<IServiceRepository>();
        _businessRepoMock = new Mock<IBusinessRepository>();
        _emailMock = new Mock<IEmailService>();
        _userRepoMock = new Mock<IUserRepository>();

        _sut = new AppointmentService(
            _appointmentRepoMock.Object,
            _serviceRepoMock.Object,
            _businessRepoMock.Object,
            _emailMock.Object,
            _userRepoMock.Object);
    }

    [Fact]
    public async Task CreateAppointment_WhenSlotAvailable_ShouldSucceedAndSendEmail()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddDays(1).Date.AddHours(10);
        var request = new CreateAppointmentRequest
        {
            ServiceId = ServiceId,
            BusinessId = BusinessId,
            StartTime = startTime
        };

        var service = new Service { Id = ServiceId, BusinessId = BusinessId, DurationMinutes = 30 };
        var client = new User { Id = ClientId, Email = "client@test.com" };
        var savedAppointment = BuildSavedAppointment(startTime, service);

        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId)).ReturnsAsync(service);
        _appointmentRepoMock.Setup(r => r.HasConflictAsync(BusinessId, startTime, startTime.AddMinutes(30)))
            .ReturnsAsync(false);
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(savedAppointment);
        _userRepoMock.Setup(r => r.GetByIdAsync(ClientId)).ReturnsAsync(client);

        // Act
        var result = await _sut.CreateAsync(request, ClientId);

        // Assert
        result.Should().NotBeNull();
        _appointmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Appointment>()), Times.Once);
        _emailMock.Verify(e => e.SendConfirmationAsync("client@test.com", It.IsAny<Appointment>()), Times.Once);
    }

    [Fact]
    public async Task CreateAppointment_WhenSlotTaken_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddDays(1).Date.AddHours(10);
        var request = new CreateAppointmentRequest
        {
            ServiceId = ServiceId,
            BusinessId = BusinessId,
            StartTime = startTime
        };

        var service = new Service { Id = ServiceId, BusinessId = BusinessId, DurationMinutes = 30 };
        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId)).ReturnsAsync(service);
        _appointmentRepoMock.Setup(r => r.HasConflictAsync(BusinessId, startTime, startTime.AddMinutes(30)))
            .ReturnsAsync(true);

        // Act
        var act = () => _sut.CreateAsync(request, ClientId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*disponible*");
    }

    [Fact]
    public async Task CreateAppointment_WhenStartTimeIsInPast_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var service = new Service { Id = ServiceId, BusinessId = BusinessId, DurationMinutes = 30 };
        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId)).ReturnsAsync(service);

        var request = new CreateAppointmentRequest
        {
            ServiceId = ServiceId,
            BusinessId = BusinessId,
            StartTime = DateTime.UtcNow.AddHours(-1)
        };

        // Act
        var act = () => _sut.CreateAsync(request, ClientId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*pasado*");
    }

    [Fact]
    public async Task CreateAppointment_WhenServiceBelongsToDifferentBusiness_ShouldThrow()
    {
        // Arrange
        var otherBusinessId = Guid.NewGuid();
        var service = new Service { Id = ServiceId, BusinessId = otherBusinessId, DurationMinutes = 30 };
        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId)).ReturnsAsync(service);

        var request = new CreateAppointmentRequest
        {
            ServiceId = ServiceId,
            BusinessId = BusinessId,
            StartTime = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var act = () => _sut.CreateAsync(request, ClientId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetAvailableSlots_ShouldReturnSlotsFilledByDuration()
    {
        // Arrange
        var service = new Service { Id = ServiceId, BusinessId = BusinessId, DurationMinutes = 60 };
        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId)).ReturnsAsync(service);
        _appointmentRepoMock
            .Setup(r => r.HasConflictAsync(BusinessId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Act
        var result = await _sut.GetAvailableSlotsAsync(BusinessId, ServiceId, date);

        // Assert
        result.Slots.Should().NotBeEmpty();
        result.Slots.Should().AllSatisfy(s => s.IsAvailable.Should().BeTrue());
        // 09:00–19:00 con slots de 60 min = 10 slots
        result.Slots.Should().HaveCount(10);
    }

    [Fact]
    public async Task UpdateStatus_WhenNotOwner_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            BusinessId = BusinessId,
            Status = AppointmentStatus.Pending
        };
        var business = new Business { Id = BusinessId, OwnerId = OwnerId };

        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointment.Id)).ReturnsAsync(appointment);
        _businessRepoMock.Setup(r => r.GetByIdAsync(BusinessId)).ReturnsAsync(business);

        var notOwner = Guid.NewGuid();

        // Act
        var act = () => _sut.UpdateStatusAsync(
            appointment.Id,
            new UpdateAppointmentStatusRequest { Status = AppointmentStatus.Confirmed },
            notOwner);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task CancelAppointment_WhenNotClient_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            ClientId = ClientId,
            Status = AppointmentStatus.Pending
        };

        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointment.Id)).ReturnsAsync(appointment);

        // Act
        var act = () => _sut.CancelAsync(appointment.Id, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static Appointment BuildSavedAppointment(DateTime startTime, Service service) =>
        new()
        {
            Id = Guid.NewGuid(),
            StartTime = startTime,
            EndTime = startTime.AddMinutes(service.DurationMinutes),
            Status = AppointmentStatus.Pending,
            ClientId = ClientId,
            Client = new User { FullName = "Test Client", Email = "client@test.com" },
            ServiceId = ServiceId,
            Service = service,
            BusinessId = BusinessId,
            Business = new Business { Name = "Test Business" }
        };
}
