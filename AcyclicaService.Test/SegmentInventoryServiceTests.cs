using AcyclicaService.Repository;
using AcyclicaService.Repository.Interfaces;
using AcyclicaService.Services.Database;
using Microsoft.Extensions.Logging;

namespace AcyclicaService.Test
{
    public class SegmentInventoryServiceTests
    {
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllRecords_WhenCalledAsync()
        {

            var mockRepository = new Mock<IAcyclicaSegmentInventoryRepository>();
            var mockedLogger = new Mock<ILogger<SegmentInventoryDocument>>();
            var expectedRecords = new List<SegmentInventoryDocument> { /* some records */ };
            mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedRecords);

            var service = new SegmentInventoryService(mockRepository.Object, mockedLogger.Object);


            var actualRecords = await service.GetAllAsync();


            actualRecords.Should().BeEquivalentTo(expectedRecords);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoRecordsExistAsync()
        {
            var mockRepository = new Mock<IAcyclicaSegmentInventoryRepository>();
            var mockedLogger = new Mock<ILogger<SegmentInventoryDocument>>();
            var dummySegmentInventoryDocument = new SegmentInventoryDocument();
            mockRepository.Setup(r => r.GetByIdAsync(dummySegmentInventoryDocument.Id)).ReturnsAsync(dummySegmentInventoryDocument);
            mockRepository.Setup(r => r.Add(dummySegmentInventoryDocument)).Verifiable();


            var service = new SegmentInventoryService(mockRepository.Object, mockedLogger.Object);


            var actualRecords = await service.GetAllAsync();


            actualRecords.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAsync_ShouldReturnRecord_WhenExistsAsync()
        {

            var mockRepository = new Mock<IAcyclicaSegmentInventoryRepository>();
            var mockedLogger = new Mock<ILogger<SegmentInventoryDocument>>();

            var expectedRecord = new SegmentInventoryDocument { /* some record */ };
            mockRepository.Setup(r => r.GetByIdAsync(expectedRecord.Id)).ReturnsAsync(expectedRecord);

            var service = new SegmentInventoryService(mockRepository.Object, mockedLogger.Object);


            var actualRecord = await service.GetAsync(expectedRecord.Id);


            actualRecord.Should().BeEquivalentTo(expectedRecord);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnNewRecord_WhenDoesNotExistAsync()
        {

            var mockRepository = new Mock<IAcyclicaSegmentInventoryRepository>();
            var mockedLogger = new Mock<ILogger<SegmentInventoryDocument>>();
            var dummySegmentInventoryDocument = new SegmentInventoryDocument();
            mockRepository.Setup(r => r.GetByIdAsync(dummySegmentInventoryDocument.Id)).ReturnsAsync(dummySegmentInventoryDocument);


            var service = new SegmentInventoryService(mockRepository.Object, mockedLogger.Object);


            var actualRecord = await service.GetAsync(dummySegmentInventoryDocument.Id);


            actualRecord.Should().BeEquivalentTo(new SegmentInventoryDocument());
        }

        [Fact]
        public async Task CreateAsync_ShouldInsertNewRecord_WhenDoesNotExistAsync()
        {

            var mockRepository = new Mock<IAcyclicaSegmentInventoryRepository>();
            var mockedLogger = new Mock<ILogger<SegmentInventoryDocument>>();
            var newRecord = new SegmentInventoryDocument { Id = Guid.NewGuid(), SegmentId = 123 };
            var dummySegmentInventoryDocument = new SegmentInventoryDocument();
            mockRepository.Setup(r => r.GetByIdAsync(newRecord.Id)).ReturnsAsync(dummySegmentInventoryDocument);
            mockRepository.Setup(r => r.Add(newRecord)).Verifiable();

            var service = new SegmentInventoryService(mockRepository.Object, mockedLogger.Object);


            await service.CreateAsync(newRecord);


            mockRepository.Verify(r => r.Add(newRecord), Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_ShouldReplaceExistingRecord_WhenExistsAsync()
        {

            var mockRepository = new Mock<IAcyclicaSegmentInventoryRepository>();
            var mockedLogger = new Mock<ILogger<SegmentInventoryDocument>>();
            var existingRecord = new SegmentInventoryDocument { Id = Guid.NewGuid(), SegmentId = 123 };
            mockRepository.Setup(r => r.GetByIdAsync(existingRecord.Id)).ReturnsAsync(existingRecord);
            mockRepository.Setup(r => r.Update(existingRecord)).Verifiable();

            var service = new SegmentInventoryService(mockRepository.Object, mockedLogger.Object);


            await service.UpdateAsync(existingRecord);


            mockRepository.Verify(r => r.Update(existingRecord), Times.Once());
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteExistingRecord_WhenExistsAsync()
        {

            var mockRepository = new Mock<IAcyclicaSegmentInventoryRepository>();
            var mockedLogger = new Mock<ILogger<SegmentInventoryDocument>>();
            var existingRecord = new SegmentInventoryDocument { Id = Guid.NewGuid(), SegmentId = 123 };
            mockRepository.Setup(r => r.Update(existingRecord)).Verifiable();

            var service = new SegmentInventoryService(mockRepository.Object, mockedLogger.Object);


            await service.DeleteAsync(existingRecord.Id);


            mockRepository.Verify(r => r.Remove(existingRecord.Id), Times.Once());
        }
    }
}
