using DorelAppBackend.Filters;
using DorelAppBackend.Models.Requests;
using DorelAppBackend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DorelAppBackend.Controllers
{
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [AuthorizationFilter]
        [HttpPost]
        [Route("api/postReview")]
        public async Task<IActionResult> PostReview([FromBody] PostReviewRequest request)
        {
            var result = await _reviewService.PostReview((string)HttpContext.Items["Email"], request.ReviwedUserId, request.ServiciuId, request.Rating, request.Description);
            
            return Ok(result);
        }

        [HttpGet]
        [Route("api/getReviews")]
        public async Task<IActionResult> GetReviews([FromQuery] int reviewedUserId, int serviciuId, int pageNumber)
        {
            var result = await _reviewService.GetReviews(reviewedUserId, serviciuId, pageNumber);

            return Ok(result);
        }

        [HttpGet]
        [Route("api/getReviewOfUser")]
        public async Task<IActionResult> GetReviewOfUser([FromQuery] int reviewedUserId, int serviciuId, int reviewerId)
        {
            var result = await _reviewService.GetReviewOfUser(reviewedUserId, serviciuId, reviewerId);

            return Ok(result);
        }

        [HttpDelete]
        [AuthorizationFilter]
        [Route("api/deleteReview")]
        public async Task<IActionResult> DeleteReview([FromQuery] int reviewedUserId, int serviciuId)
        {
            var result = await _reviewService.DeleteReview((string)HttpContext.Items["Email"], reviewedUserId, serviciuId);

            return Ok(result);
        }

        [AuthorizationFilter]
        [HttpPost]
        [Route("api/editReview")]
        public async Task<IActionResult> EditReview([FromBody] PostReviewRequest request)
        {
            var result = await _reviewService.PostReview((string)HttpContext.Items["Email"], request.ReviwedUserId, request.ServiciuId, request.Rating, request.Description, true);

            return Ok(result);
        }
    }
}
