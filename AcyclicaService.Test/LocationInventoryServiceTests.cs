using AcyclicaService.Repository;
using AcyclicaService.Repository.Interfaces;
using AcyclicaService.Services.Database;
using Microsoft.Extensions.Logging;

namespace AcyclicaService.Test
{
    public class LocationInventoryServiceTests
    {
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllRecords_WhenCalledAsync()
        {
            var mockRepository = new Mock<IAcyclicaLocationInventoryRepository>();
            var mockedLogger = new Mock<ILogger<LocationInventoryDocument>>();
            var expectedRecords = new List<LocationInventoryDocument> { /* some records */ };
            mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedRecords);

            var service = new LocationInventoryService(mockRepository.Object, mockedLogger.Object);

            var actualRecords = await service.GetAllAsync();

            actualRecords.Should().BeEquivalentTo(expectedRecords);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoRecordsExistAsync()
        {
            var mockRepository = new Mock<IAcyclicaLocationInventoryRepository>();
            var mockedLogger = new Mock<ILogger<LocationInventoryDocument>>();
            var dummyLocationInventoryDocument = new LocationInventoryDocument();
            mockRepository.Setup(r => r.GetByIdAsync(dummyLocationInventoryDocument.Id)).ReturnsAsync(dummyLocationInventoryDocument);
            mockRepository.Setup(r => r.Add(dummyLocationInventoryDocument)).Verifiable();

            var service = new LocationInventoryService(mockRepository.Object, mockedLogger.Object);

            var actualRecords = await service.GetAllAsync();

            actualRecords.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAsync_ShouldReturnRecord_WhenExistsAsync()
        {
            var mockRepository = new Mock<IAcyclicaLocationInventoryRepository>();
            var mockedLogger = new Mock<ILogger<LocationInventoryDocument>>();
            var expectedRecord = new LocationInventoryDocument { /* some record */ };
            mockRepository.Setup(r => r.GetByIdAsync(expectedRecord.Id)).ReturnsAsync(expectedRecord);

            var service = new LocationInventoryService(mockRepository.Object, mockedLogger.Object);

            var actualRecord = await service.GetAsync(expectedRecord.Id);

            actualRecord.Should().BeEquivalentTo(expectedRecord);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnNewRecord_WhenDoesNotExistAsync()
        {
            var mockRepository = new Mock<IAcyclicaLocationInventoryRepository>();
            var mockedLogger = new Mock<ILogger<LocationInventoryDocument>>();
            var dummyLocationInventoryDocument = new LocationInventoryDocument();
            mockRepository.Setup(r => r.GetByIdAsync(dummyLocationInventoryDocument.Id)).ReturnsAsync(dummyLocationInventoryDocument);

            var service = new LocationInventoryService(mockRepository.Object, mockedLogger.Object);

            var actualRecord = await service.GetAsync(dummyLocationInventoryDocument.Id);

            actualRecord.Should().BeEquivalentTo(new LocationInventoryDocument());
        }

        [Fact]
        public async Task CreateAsync_ShouldInsertNewRecord_WhenDoesNotExistAsync()
        {
            var mockRepository = new Mock<IAcyclicaLocationInventoryRepository>();
            var mockedLogger = new Mock<ILogger<LocationInventoryDocument>>();
            var newRecord = new LocationInventoryDocument { LocationId = 123 };
            var dummyLocationInventoryDocument = new LocationInventoryDocument();
            mockRepository.Setup(r => r.GetByIdAsync(newRecord.Id)).ReturnsAsync(dummyLocationInventoryDocument);
            mockRepository.Setup(r => r.Add(newRecord)).Verifiable();

            var service = new LocationInventoryService(mockRepository.Object, mockedLogger.Object);

            await service.CreateAsync(newRecord);

            mockRepository.Verify(r => r.Add(newRecord), Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_ShouldReplaceExistingRecord_WhenExistsAsync()
        {
            var mockRepository = new Mock<IAcyclicaLocationInventoryRepository>();
            var mockedLogger = new Mock<ILogger<LocationInventoryDocument>>();
            var existingRecord = new LocationInventoryDocument { LocationId = 123 };
            mockRepository.Setup(r => r.GetByIdAsync(existingRecord.Id)).ReturnsAsync(existingRecord);
            mockRepository.Setup(r => r.Update(existingRecord)).Verifiable();

            var service = new LocationInventoryService(mockRepository.Object, mockedLogger.Object);

            await service.UpdateAsync(existingRecord);

            mockRepository.Verify(r => r.Update(existingRecord), Times.Once());
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteExistingRecord_WhenExistsAsync()
        {
            var mockRepository = new Mock<IAcyclicaLocationInventoryRepository>();
            var mockedLogger = new Mock<ILogger<LocationInventoryDocument>>();
            var existingRecord = new LocationInventoryDocument { LocationId = 123 };
            mockRepository.Setup(r => r.Remove(existingRecord.Id)).Verifiable();

            var service = new LocationInventoryService(mockRepository.Object, mockedLogger.Object);

            await service.DeleteAsync(existingRecord.Id);

            mockRepository.Verify(r => r.Remove(existingRecord.Id), Times.Once());
        }

    }
}
