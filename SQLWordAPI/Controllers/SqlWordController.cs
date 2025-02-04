using Microsoft.AspNetCore.Mvc;
using SQLWordAPI.MiddlewareExtensions;
using SQLWordAPI.ResourceModels;
using SQLWordAPI.Services;
using System.ComponentModel.DataAnnotations;

namespace SQLWordAPI.Controllers
{
    [Route("api/[controller]")]
    public class SqlWordController : BaseApiController
    {
        private readonly ISqlWordService _sqlWordService;

        public SqlWordController(ISqlWordService sqlWordService)
        {
            _sqlWordService = sqlWordService;
        }

        /// <summary>
        /// This endpiont is responsbile for getting a single and list of SQL words.
        /// </summary>
        /// <param name="sqlWordId"></param>
        /// <returns></returns>
        [HttpGet()]
        [ProducesResponseType(typeof(ResourceResponse<IEnumerable<SqlWordResource>>), 200)]
        [ProducesResponseType(typeof(ErrorResource), 400)]
        public async Task<IActionResult> GetSqlWords([FromQuery] Guid? sqlWordId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.GetErrorMessages());

            var result = await _sqlWordService.GetSqlWordsAsync(sqlWordId);

            return result.Success ? StatusCode(200, result) : BadRequest(new ErrorResource(result.Message!));
        }

        /// <summary>
        /// This endpiont is responsible for deleting a SQL word resource.
        /// </summary>
        /// <param name="sqlWordId"></param>
        /// <returns></returns>
        [HttpDelete("{sqlWordId}")]
        [ProducesResponseType(typeof(ResourceResponse), 200)]
        [ProducesResponseType(typeof(ErrorResource), 400)]
        public async Task<IActionResult> DeleteSqlWord(Guid sqlWordId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.GetErrorMessages());

            var result = await _sqlWordService.DeleteSqlWordAsync(sqlWordId);

            return result.Success ? StatusCode(200, result) : BadRequest(new ErrorResource(result.Message!));
        }

        /// <summary>
        /// This endpiont is responsbile for updating a SQL word resource.
        /// </summary>
        /// <param name="sqlWordId"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        [HttpPut("{sqlWordId}")]
        [ProducesResponseType(typeof(ResourceResponse), 200)]
        [ProducesResponseType(typeof(ErrorResource), 400)]
        public async Task<IActionResult> PutSqlWord(Guid sqlWordId, [FromBody] SaveSqlWordResource resource)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.GetErrorMessages());

            var result = await _sqlWordService.SaveSqlWordAsync(resource, sqlWordId);

            return result.Success ? StatusCode(200, result) : BadRequest(new ErrorResource(result.Message!));
        }

        /// <summary>
        /// This endpiont is responsible for creating a new SQL word resource.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        [HttpPost()]
        [ProducesResponseType(typeof(ResourceResponse), 201)]
        [ProducesResponseType(typeof(ErrorResource), 400)]
        public async Task<IActionResult> PostSqlWord([FromBody] SaveSqlWordResource resource)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.GetErrorMessages());

            var result = await _sqlWordService.SaveSqlWordAsync(resource);

            return result.Success ? StatusCode(201, result) : BadRequest(new ErrorResource(result.Message!));
        }

        /// <summary>
        /// This endpiont is responsible for performing the business logic that is nessesary for replacing
        /// sensitive keywords with a predefined list of keywords as stored within the database.
        /// </summary>
        /// <param name="sqlWord"></param>
        /// <returns></returns>
        [HttpGet("HideSensitiveWords")]
        [ProducesResponseType(typeof(ResourceResponse<String>), 200)]
        [ProducesResponseType(typeof(ErrorResource), 400)]
        public async Task<IActionResult> HideSensitiveWords([FromQuery][Required] string sqlWord)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.GetErrorMessages());

            var result = await _sqlWordService.RemoveSensitiveWords(sqlWord);

            return result.Success ? StatusCode(200, result) : BadRequest(new ErrorResource(result.Message!));
        }
    }
}
