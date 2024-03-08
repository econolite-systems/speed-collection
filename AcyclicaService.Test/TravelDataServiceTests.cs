using AcyclicaService.Repository;
using AcyclicaService.Repository.Interfaces;
using AcyclicaService.Services.Database;
using Microsoft.Extensions.Logging;

namespace AcyclicaService.Test
{
    public class TravelDataServiceTests
    {
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllRecords_WhenCalledAsync()
        {

            var mockRepository = new Mock<IAcyclicaTravelDataRepository>();
            var mockedLogger = new Mock<ILogger<TravelDataDocument>>();
            var expectedRecords = new List<TravelDataDocument> { new TravelDataDocument (){ Id = Guid.NewGuid(),
                Time = 1234567890L,
                Strength = 100,
                First = 1,
                Last = 10,
                Minimum = 1,
                Maximum = 10 }
            };
            mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedRecords);

            var service = new TravelDataService(mockRepository.Object, mockedLogger.Object);


            var actualRecords = await service.GetAllAsync();


            actualRecords.Should().BeEquivalentTo(expectedRecords);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoRecordsExistAsync()
        {

            var mockRepository = new Mock<IAcyclicaTravelDataRepository>();
            var mockedLogger = new Mock<ILogger<TravelDataDocument>>();
            var dummyTravelDataDtoList = new List<TravelDataDocument>();
            mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(dummyTravelDataDtoList);


            var service = new TravelDataService(mockRepository.Object, mockedLogger.Object);


            var actualRecords = await service.GetAllAsync();


            actualRecords.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAsync_ShouldReturnRecord_WhenExistsAsync()
        {

            var mockRepository = new Mock<IAcyclicaTravelDataRepository>();
            var mockedLogger = new Mock<ILogger<TravelDataDocument>>();
            var expectedRecord = new TravelDataDocument
            {
                Id = Guid.NewGuid(),
                Time = 1234567890L,
                Strength = 100,
                First = 1,
                Last = 10,
                Minimum = 1,
                Maximum = 10
            };
            mockRepository.Setup(r => r.GetByIdAsync(expectedRecord.Id)).ReturnsAsync(expectedRecord);

            var service = new TravelDataService(mockRepository.Object, mockedLogger.Object);


            var actualRecord = await service.GetAsync(expectedRecord.Id);


            actualRecord.Should().BeEquivalentTo(expectedRecord);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnNewRecord_WhenDoesNotExistAsync()
        {

            var mockRepository = new Mock<IAcyclicaTravelDataRepository>();
            var mockedLogger = new Mock<ILogger<TravelDataDocument>>();
            var dummyTravelDataDtoList = new List<TravelDataDocument>();
            mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(dummyTravelDataDtoList);


            var service = new TravelDataService(mockRepository.Object, mockedLogger.Object);


            var actualRecord = await service.GetAsync(Guid.NewGuid());


            actualRecord.Should().BeEquivalentTo(new TravelDataDocument());
        }

        [Fact]
        public async Task CreateAsync_ShouldInsertNewRecord_WhenDoesNotExistAsync()
        {

            var mockRepository = new Mock<IAcyclicaTravelDataRepository>();
            var mockedLogger = new Mock<ILogger<TravelDataDocument>>();
            var newRecord = new TravelDataDocument { Id = Guid.NewGuid(), Time = 123 };
            var dummyTravelDataDtoList = new List<TravelDataDocument>();
            mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(dummyTravelDataDtoList);
            ;
            mockRepository.Setup(r => r.Add(newRecord)).Verifiable();

            var service = new TravelDataService(mockRepository.Object, mockedLogger.Object);


            await service.CreateAsync(newRecord);


            mockRepository.Verify(r => r.Add(newRecord), Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_ShouldReplaceExistingRecord_WhenExistsAsync()
        {

            var mockRepository = new Mock<IAcyclicaTravelDataRepository>();
            var mockedLogger = new Mock<ILogger<TravelDataDocument>>();
            var existingRecord = new TravelDataDocument { Id = Guid.NewGuid(), Time = 123 };
            mockRepository.Setup(r => r.GetByIdAsync(existingRecord.Id)).ReturnsAsync(existingRecord);
            mockRepository.Setup(r => r.Update(existingRecord)).Verifiable();

            var service = new TravelDataService(mockRepository.Object, mockedLogger.Object);


            await service.UpdateAsync(existingRecord);


            mockRepository.Verify(r => r.Update(existingRecord), Times.Once());
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteExistingRecord_WhenExistsAsync()
        {

            var mockRepository = new Mock<IAcyclicaTravelDataRepository>();
            var mockedLogger = new Mock<ILogger<TravelDataDocument>>();
            var existingRecord = new TravelDataDocument { Id = Guid.NewGuid(), Time = 123 };
            mockRepository.Setup(r => r.Update(existingRecord)).Verifiable();

            var service = new TravelDataService(mockRepository.Object, mockedLogger.Object);


            await service.DeleteAsync(existingRecord.Id);


            mockRepository.Verify(r => r.Remove(existingRecord.Id), Times.Once());
        }
    }
}
