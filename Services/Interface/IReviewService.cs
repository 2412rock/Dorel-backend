using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Models.Responses;

namespace DorelAppBackend.Services.Interface
{
    public interface IReviewService
    {
        public Task<Maybe<string>> PostReview(string userEmail, int reviewedUserId, int serviciuId, decimal rating, string description, bool update = false);

        public Task<Maybe<DBReviewModel[]>> GetReviews(int reviewedUserId, int serviciuId, int pageNumber);

        public Task<Maybe<DBReviewModel>> GetReviewOfUser(int reviewedUserId, int serviciuId, int reviewerId);

        public Task<Maybe<string>> DeleteReview(string userEmail, int reviewedUserId, int serviciuId);
    }
}
