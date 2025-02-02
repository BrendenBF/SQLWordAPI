using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SQLWordAPI.Constants;
using SQLWordAPI.Models;
using SQLWordAPI.Repositories;
using SQLWordAPI.ResourceModels;
using SQLWordAPI.Services;

namespace SQLWordTests
{
    [TestClass]
    public class SqlWordServiceTests
    {
        private Mock<ISqlWordRepository>? _sqlWordRepositoryMock;
        private SqlWordService? _sqlWordService;
        private IMemoryCache? _memoryCache;

        [TestInitialize]
        public void SetUp()
        {
            var services = new ServiceCollection();
            services.AddMemoryCache();
            var serviceProvider = services.BuildServiceProvider();

            _memoryCache = serviceProvider.GetService<IMemoryCache>()!;

            _sqlWordRepositoryMock = new Mock<ISqlWordRepository>();
            _sqlWordService = new SqlWordService(_sqlWordRepositoryMock.Object, _memoryCache!);
        }

        [TestMethod]
        public async Task DeleteSqlWordAsync_CheckSqlWordExists_Fails_ReturnsError()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var errorMsg = "Sql word does not exist in the system.";

            var checkResult = new BaseResult { Success = false, ErrorMsg = errorMsg };

            _sqlWordRepositoryMock!
                .Setup(repo => repo.CheckSqlWordExistsAsync(id))
                .ReturnsAsync(checkResult);
            
            // Act
            var result = await _sqlWordService!.DeleteSqlWordAsync(id);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(errorMsg, result.Message);
            _sqlWordRepositoryMock.Verify(repo => repo.CheckSqlWordExistsAsync(id), Times.Once);
            _sqlWordRepositoryMock.Verify(repo => repo.DeleteSqlWordAsync(It.IsAny<Guid>()), Times.Never);
        }

        [TestMethod]
        public async Task DeleteSqlWordAsync_ShouldReturnErrorMessage_WhenCheckSqlWordExistsFails()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var errorMsg = "Sql word does not exist.";

            // Mock the CheckSqlWordExistsAsync to return a failed result
            _sqlWordRepositoryMock!.Setup(repo => repo.CheckSqlWordExistsAsync(id))
                .ReturnsAsync(new BaseResult { Success = false, ErrorMsg = errorMsg });

            // Act
            var result = await _sqlWordService!.DeleteSqlWordAsync(id);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(errorMsg, result.Message);
            _sqlWordRepositoryMock.Verify(repo => repo.CheckSqlWordExistsAsync(id), Times.Once);
            _sqlWordRepositoryMock.Verify(repo => repo.DeleteSqlWordAsync(It.IsAny<Guid>()), Times.Never);
        }

        [TestMethod]
        public async Task DeleteSqlWordAsync_ShouldReturnSuccess_WhenDeleteSqlWordSucceeds()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var successMsg = "Sql word deleted successfully.";

            // Mock CheckSqlWordExistsAsync to return a success result
            _sqlWordRepositoryMock!.Setup(repo => repo.CheckSqlWordExistsAsync(id))
                .ReturnsAsync(new BaseResult { Success = true });

            // Mock DeleteSqlWordAsync to return a success result
            _sqlWordRepositoryMock.Setup(repo => repo.DeleteSqlWordAsync(id))
                .ReturnsAsync(new BaseResult { Success = true, Message = successMsg });

            // Act
            var result = await _sqlWordService!.DeleteSqlWordAsync(id);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(successMsg, result.Message);
            _sqlWordRepositoryMock.Verify(repo => repo.CheckSqlWordExistsAsync(id), Times.Once);
            _sqlWordRepositoryMock.Verify(repo => repo.DeleteSqlWordAsync(It.IsAny<Guid>()), Times.Once);
        }

        [TestMethod]
        public async Task DeleteSqlWordAsync_ShouldReturnErrorMessage_WhenDeleteSqlWordFails()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var deleteErrorMsg = "Failed to delete sql word.";

            // Mock CheckSqlWordExistsAsync to return a success result
            _sqlWordRepositoryMock!.Setup(repo => repo.CheckSqlWordExistsAsync(id))
                .ReturnsAsync(new BaseResult { Success = true });

            // Mock DeleteSqlWordAsync to return a failed result
            _sqlWordRepositoryMock!.Setup(repo => repo.DeleteSqlWordAsync(id))
                .ReturnsAsync(new BaseResult { Success = false, ErrorMsg = deleteErrorMsg });

            // Act
            var result = await _sqlWordService!.DeleteSqlWordAsync(id);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(deleteErrorMsg, result.Message);
            _sqlWordRepositoryMock.Verify(repo => repo.CheckSqlWordExistsAsync(id), Times.Once);
            _sqlWordRepositoryMock.Verify(repo => repo.DeleteSqlWordAsync(It.IsAny<Guid>()), Times.Once);
        }

        [TestMethod]
        public async Task RemoveSensitiveWords_ShouldReturnTransformedString_WhenSensitiveWordFound()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var sentence = "This is a sentence with create in it.";

            var sensitiveWords = new List<SqlWordDto>
            {
                new SqlWordDto { Id = id.ToString().Trim(), SqlWord = "create", DateCreated = DateTime.Now, DateUpdated = DateTime.Now }
            };
                
            // Mock GetSqlWordsAsync to return sensitive words
            _sqlWordRepositoryMock!.Setup(repo => repo.GetSqlWordsAsync(null))
                .ReturnsAsync(new BaseResult<IEnumerable<SqlWordDto>>(msg: "Sql words successfully retrieved.") { Response = sensitiveWords });

            // Act
            var result = await _sqlWordService!.RemoveSensitiveWords(sentence);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("This is a sentence with ****** in it.", result.Resource);
            Assert.AreEqual("Successfully transformed the string and removed the senentive characters where it was deemed applicable.", result.Message);
            _sqlWordRepositoryMock.Verify(repo => repo.GetSqlWordsAsync(null), Times.Once);
        }

        [TestMethod]
        public async Task GetSqlWordsAsync_ShouldReturnWordsFromCache_WhenCacheContainsData()
        {
            // Arrange
            var id = Guid.NewGuid();
            var cacheKey = string.Format(CacheKeyConstants.CacheKeySingle, id);

            var cachedWords = new BaseResult<IEnumerable<SqlWordDto>>
            {
                Success = true,
                Response = new List<SqlWordDto>
                {
                    new SqlWordDto { Id = id.ToString().Trim(), SqlWord = "example", DateCreated = DateTime.Now, DateUpdated = DateTime.Now }
                }
            };

            // Mock CheckSqlWordExistsAsync to return a success result
            _sqlWordRepositoryMock!.Setup(repo => repo.CheckSqlWordExistsAsync(id))
                .ReturnsAsync(new BaseResult { Success = true });

            // Mock repository to return data
            _sqlWordRepositoryMock.Setup(repo => repo.GetSqlWordsAsync(id))
                .ReturnsAsync(cachedWords);

            _memoryCache!.Set(cacheKey, cachedWords);

            // Act
            var result = await _sqlWordService!.GetSqlWordsAsync(id);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.Resource!.Count());
            Assert.AreEqual("example", result.Resource!.First().SqlWord);
            _sqlWordRepositoryMock!.Verify(repo => repo.CheckSqlWordExistsAsync(id), Times.Never);
            _sqlWordRepositoryMock!.Verify(repo => repo.GetSqlWordsAsync(It.IsAny<Guid?>()), Times.Never);
        }

        [TestMethod]
        public async Task GetSqlWordsAsync_ShouldReturnWordsFromRepository_WhenCacheMiss()
        {
            // Arrange
            var id = Guid.NewGuid();

            var wordsFromRepository = new BaseResult<IEnumerable<SqlWordDto>>
            {
                Success = true,
                Response = new List<SqlWordDto>
                {
                    new SqlWordDto { Id = id.ToString().Trim(), SqlWord = "example", DateCreated = DateTime.Now, DateUpdated = DateTime.Now }
                }
            };

            // Mock CheckSqlWordExistsAsync to return a success result
            _sqlWordRepositoryMock!.Setup(repo => repo.CheckSqlWordExistsAsync(id))
                .ReturnsAsync(new BaseResult { Success = true });

            // Mock repository to return data
            _sqlWordRepositoryMock.Setup(repo => repo.GetSqlWordsAsync(id))
                .ReturnsAsync(wordsFromRepository);

            // Act
            var result = await _sqlWordService!.GetSqlWordsAsync(id);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.Resource!.Count());
            Assert.AreEqual("example", result.Resource!.First().SqlWord);
            _sqlWordRepositoryMock.Verify(repo => repo.CheckSqlWordExistsAsync(id), Times.Once);
            _sqlWordRepositoryMock.Verify(repo => repo.GetSqlWordsAsync(id), Times.Once);
        }

        [TestMethod]
        public async Task GetSqlWordsAsync_ShouldReturnError_WhenRepositoryFails()
        {
            // Arrange
            var id = Guid.NewGuid();

            var wordsFromRepository = new BaseResult<IEnumerable<SqlWordDto>>
            {
                Success = false,
                ErrorMsg = "Repository error"
            };

            // Mock CheckSqlWordExistsAsync to return a success result
            _sqlWordRepositoryMock!.Setup(repo => repo.CheckSqlWordExistsAsync(id))
                .ReturnsAsync(new BaseResult { Success = true });

            // Mock repository to return an error
            _sqlWordRepositoryMock.Setup(repo => repo.GetSqlWordsAsync(id))
                .ReturnsAsync(wordsFromRepository);

            // Act
            var result = await _sqlWordService!.GetSqlWordsAsync(id);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Repository error", result.Message);
            _sqlWordRepositoryMock.Verify(repo => repo.GetSqlWordsAsync(id), Times.Once);
            _sqlWordRepositoryMock.Verify(repo => repo.CheckSqlWordExistsAsync(id), Times.Once);
        }

        [TestMethod]
        public async Task GetSqlWordsAsync_ShouldReturnEmptyList_WhenNoWordsFoundInRepository()
        {
            // Arrange
            var id = Guid.NewGuid();

            var wordsFromRepository = new BaseResult<IEnumerable<SqlWordDto>>
            {
                Success = true,
                Response = new List<SqlWordDto>()
            };

            // Mock CheckSqlWordExistsAsync to return a success result
            _sqlWordRepositoryMock!.Setup(repo => repo.CheckSqlWordExistsAsync(id))
                .ReturnsAsync(new BaseResult { Success = true });

            // Mock repository to return an empty list
            _sqlWordRepositoryMock.Setup(repo => repo.GetSqlWordsAsync(id))
                .ReturnsAsync(wordsFromRepository);

            // Act
            var result = await _sqlWordService!.GetSqlWordsAsync(id);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.Resource!.Count());
            _sqlWordRepositoryMock.Verify(repo => repo.GetSqlWordsAsync(id), Times.Once);
            _sqlWordRepositoryMock.Verify(repo => repo.CheckSqlWordExistsAsync(id), Times.Once);
        }

        [TestMethod]
        public async Task SaveSqlWordAsync_ShouldReturnError_WhenCheckSqlWordExistsFails()
        {
            // Arrange
            var id = Guid.NewGuid();

            var sqlWordDto = new SqlWordDto();

            var sqlWordResource = new SaveSqlWordResource() { SqlWord = "testSqlWord" };

            // Mock CheckSqlWordExistsAsync to return a failure
            _sqlWordRepositoryMock!.Setup(repo => repo.CheckSqlWordExistsAsync(id))
                .ReturnsAsync(new BaseResult { Success = false, ErrorMsg = "Sql word not found." });

            // Act
            var result = await _sqlWordService!.SaveSqlWordAsync(sqlWordResource, id);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Sql word not found.", result.Message);
            _sqlWordRepositoryMock.Verify(repo => repo.SaveSqlWordAsync(sqlWordDto), Times.Never);
            _sqlWordRepositoryMock.Verify(repo => repo.CheckSqlWordExistsAsync(id), Times.Once);
        }

        [TestMethod]
        public async Task SaveSqlWordAsync_ShouldSkipCheckSqlWordExists_WhenIdIsNull()
        {
            // Arrange
            var saveSqlWordResource = new SaveSqlWordResource() { SqlWord = "TestSqlWord" };

            // Mock CheckSqlWordExistsAsync to return a failure
            _sqlWordRepositoryMock!.Setup(repo => repo.CheckSqlWordExistsAsync(null))
                .ReturnsAsync(new BaseResult { Success = false, ErrorMsg = "Sql word not found." });

            // Mock SaveSqlWordAsync to return a success result
            _sqlWordRepositoryMock.Setup(repo => repo.SaveSqlWordAsync(It.IsAny<SqlWordDto>()))
                .ReturnsAsync(new BaseResult { Success = true, Message = "Sql word saved." });

            // Act
            var result = await _sqlWordService!.SaveSqlWordAsync(saveSqlWordResource, null);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Sql word saved.", result.Message);
            _sqlWordRepositoryMock.Verify(repo => repo.CheckSqlWordExistsAsync(null), Times.Never);
            _sqlWordRepositoryMock.Verify(repo => repo.SaveSqlWordAsync(It.IsAny<SqlWordDto>()), Times.Once);
        }

        [TestMethod]
        public async Task SaveSqlWordAsync_ShouldNotSkipCheckSqlWordExists_WhenIdExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var saveSqlWordResource = new SaveSqlWordResource() { SqlWord = "TestSqlWord" };

            // Mock CheckSqlWordExistsAsync to return a failure
            _sqlWordRepositoryMock!.Setup(repo => repo.CheckSqlWordExistsAsync(id))
                .ReturnsAsync(new BaseResult { Success = true, ErrorMsg = "Sql word found." });

            // Mock SaveSqlWordAsync to return a success result
            _sqlWordRepositoryMock.Setup(repo => repo.SaveSqlWordAsync(It.IsAny<SqlWordDto>()))
                .ReturnsAsync(new BaseResult { Success = true, Message = "Sql word saved." });

            // Act
            var result = await _sqlWordService!.SaveSqlWordAsync(saveSqlWordResource, id);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Sql word saved.", result.Message);
            _sqlWordRepositoryMock.Verify(repo => repo.CheckSqlWordExistsAsync(id), Times.Once);
            _sqlWordRepositoryMock.Verify(repo => repo.SaveSqlWordAsync(It.IsAny<SqlWordDto>()), Times.Once);
        }
    }
}
