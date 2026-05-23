using FluentAssertions;
using Moq;
using TurnOS.Application.DTOs.Business;
using TurnOS.Application.Services;
using TurnOS.Domain.Entities;
using TurnOS.Domain.Interfaces;

namespace TurnOS.UnitTests.Services;

public class BusinessServiceTests
{
    private readonly Mock<IBusinessRepository> _businessRepoMock;
    private readonly Mock<IServiceRepository> _serviceRepoMock;
    private readonly BusinessService _sut;

    private static readonly Guid OwnerId = Guid.NewGuid();

    public BusinessServiceTests()
    {
        _businessRepoMock = new Mock<IBusinessRepository>();
        _serviceRepoMock = new Mock<IServiceRepository>();
        _sut = new BusinessService(_businessRepoMock.Object, _serviceRepoMock.Object);
    }

    [Fact]
    public async Task CreateBusiness_ShouldAssignOwnerIdAndCallAdd()
    {
        // Arrange
        var request = new CreateBusinessRequest
        {
            Name = "Barbería Test",
            Address = "Calle 1",
            Phone = "555-0001"
        };

        Business? captured = null;
        _businessRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Business>()))
            .Callback<Business>(b => captured = b)
            .Returns(Task.CompletedTask);

        _businessRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(() => captured);

        // Act
        var result = await _sut.CreateAsync(request, OwnerId);

        // Assert
        result.OwnerId.Should().Be(OwnerId);
        result.Name.Should().Be("Barbería Test");
        _businessRepoMock.Verify(r => r.AddAsync(It.IsAny<Business>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBusiness_WhenCalledByOwner_ShouldUpdateAndReturn()
    {
        // Arrange
        var business = new Business
        {
            Id = Guid.NewGuid(),
            Name = "Nombre Viejo",
            OwnerId = OwnerId,
            Owner = new User { FullName = "Owner" }
        };

        _businessRepoMock.Setup(r => r.GetByIdAsync(business.Id)).ReturnsAsync(business);

        var request = new UpdateBusinessRequest { Name = "Nombre Nuevo", Address = "Nueva Dir" };

        // Act
        var result = await _sut.UpdateAsync(business.Id, request, OwnerId);

        // Assert
        result.Name.Should().Be("Nombre Nuevo");
        _businessRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Business>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBusiness_WhenCalledByNonOwner_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var business = new Business { Id = Guid.NewGuid(), OwnerId = OwnerId };
        _businessRepoMock.Setup(r => r.GetByIdAsync(business.Id)).ReturnsAsync(business);

        var nonOwner = Guid.NewGuid();

        // Act
        var act = () => _sut.UpdateAsync(business.Id, new UpdateBusinessRequest { Name = "X" }, nonOwner);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*dueño*");
    }

    [Fact]
    public async Task DeleteBusiness_WhenCalledByOwner_ShouldCallDelete()
    {
        // Arrange
        var business = new Business { Id = Guid.NewGuid(), OwnerId = OwnerId };
        _businessRepoMock.Setup(r => r.GetByIdAsync(business.Id)).ReturnsAsync(business);

        // Act
        await _sut.DeleteAsync(business.Id, OwnerId);

        // Assert
        _businessRepoMock.Verify(r => r.DeleteAsync(business), Times.Once);
    }

    [Fact]
    public async Task DeleteBusiness_WhenNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _businessRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Business?)null);

        // Act
        var act = () => _sut.DeleteAsync(Guid.NewGuid(), OwnerId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetById_WhenBusinessExists_ShouldReturnResponse()
    {
        // Arrange
        var business = new Business
        {
            Id = Guid.NewGuid(),
            Name = "Mi Negocio",
            OwnerId = OwnerId,
            Owner = new User { FullName = "Owner Name" }
        };
        _businessRepoMock.Setup(r => r.GetByIdAsync(business.Id)).ReturnsAsync(business);

        // Act
        var result = await _sut.GetByIdAsync(business.Id);

        // Assert
        result.Id.Should().Be(business.Id);
        result.Name.Should().Be("Mi Negocio");
        result.OwnerName.Should().Be("Owner Name");
    }
}
