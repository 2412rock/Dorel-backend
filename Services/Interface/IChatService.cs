using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Models.Responses;
using DorelAppBackend.Services.Implementation;

namespace DorelAppBackend.Services.Interface
{
    public interface IChatService
    {
        public Task<Maybe<string>> SaveMessage(string email, int receiptId, string message);

        public Task<Maybe<List<Group>>> GetMessages(string email);

        public Task<Maybe<string>> SeenMessage(string email);

        public Task<Maybe<bool>> HasUnseenMessages(string email);
    }
}
