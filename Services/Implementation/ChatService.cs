using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Models.Responses;
using DorelAppBackend.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace DorelAppBackend.Services.Implementation
{
    public class ChatService: IChatService
    {
        DorelDbContext _dorelDbContext;

        public ChatService(DorelDbContext dorelDbContext)
        {
            _dorelDbContext = dorelDbContext;
        }

        public async Task<Maybe<string>> SeenMessage(string email, int senderId)
        {
            var result = new Maybe<string>();
            var user = await _dorelDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if(user != null)
            {
                var messages = _dorelDbContext.Messages.Where(x => x.ReceipientId == user.UserID && x.SenderId == senderId);
                foreach (var message in messages)
                {
                    message.Seen = true;
                    _dorelDbContext.Messages.Update(message);
                }

                await _dorelDbContext.SaveChangesAsync();
                result.SetSuccess("Seen ");
            }
            else
            {
                result.SetException("No user found");
            }

            return result;
        }

        public async Task<Maybe<List<int>>> HasUnseenMessages(string email)
        {
            var result = new Maybe<List<int>>();
            var user = await _dorelDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if(user != null)
            {
                var notSeenMessags = await _dorelDbContext.Messages.Where(x => (x.Seen == null || x.Seen == false) && x.ReceipientId == user.UserID).ToListAsync();
                var notSeenMessagesFrom = new List<int>();
                foreach(var message in notSeenMessags)
                {
                    if(!notSeenMessagesFrom.Any(x => x == message.SenderId))
                    {
                        notSeenMessagesFrom.Add(message.SenderId);
                    }
                }
                result.SetSuccess(notSeenMessagesFrom);
            }
            else
            {
                result.SetException("No user found");
            }
            return result;
        }

        public async Task<Maybe<string>> SaveMessage(string email, int receiptId, string message)
        {
            var result = new Maybe<string>();
            var user = await _dorelDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            //var receipt = await _dorelDbContext.Users.FirstOrDefaultAsync(u => u.Email == receiptEmila);
            if (user != null)
            {
                var dbModel = new DBMessage() { SenderId= user.UserID, ReceipientId = receiptId, Message = message, SentTime = DateTime.Now };
    
               await _dorelDbContext.Messages.AddAsync(dbModel);
                try
                {
                    await _dorelDbContext.SaveChangesAsync();
                    result.SetSuccess("Ok");
                }
                catch(Exception e)
                {
                    result.SetException("Error entity framework");
                }
            }
            else
            {
                result.SetException("No user found");
            }
            return result;
        }

        public async Task<Maybe<List<Group>>> GetMessages(string email)
        {
            var result = new Maybe<List<Group>>();
            var user = await _dorelDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            var groups = new List<Group>();
            if(user != null)
            {
                var mesajeTrimise = await _dorelDbContext.Messages.Where(msg => msg.SenderId == user.UserID && msg.ReceipientId != user.UserID).ToArrayAsync();
                var mesajePrimite = await _dorelDbContext.Messages.Where(msg => msg.SenderId != user.UserID && msg.ReceipientId == user.UserID).ToArrayAsync();
                foreach(var mesajTrimis in mesajeTrimise)
                {
                    var group = groups.FirstOrDefault(e => e.WithUser == mesajTrimis.ReceipientId);
                    if (group == null)
                    {
                        var messages = new List<Message>();
                        messages.Add(new Message() { MessageText = mesajTrimis.Message, Receipt = mesajTrimis.ReceipientId, SenderId = mesajTrimis.SenderId, SentTime = mesajTrimis.SentTime });
                        var withUserName = await _dorelDbContext.Users.FirstOrDefaultAsync(u => u.UserID == mesajTrimis.ReceipientId);
                        groups.Add(new Group() { WithUser = mesajTrimis.ReceipientId, WithUserName = withUserName.Name, Messages = messages });
                    }
                    else
                    {
                        group.Messages.Add(new Message() { SenderId = user.UserID, Receipt = mesajTrimis.ReceipientId, MessageText = mesajTrimis.Message, SentTime = mesajTrimis.SentTime });
                    }
                }
                foreach (var mesajPrimit in mesajePrimite)
                {
                    var group = groups.FirstOrDefault(e => e.WithUser == mesajPrimit.SenderId);
                    if (group == null)
                    {
                        var messages = new List<Message>();
                        messages.Add(new Message() { MessageText = mesajPrimit.Message, Receipt = user.UserID, SenderId = mesajPrimit.SenderId, SentTime = mesajPrimit.SentTime });
                        var withUserName = await _dorelDbContext.Users.FirstOrDefaultAsync(u => u.UserID == mesajPrimit.SenderId);
                        groups.Add(new Group() { WithUser = mesajPrimit.SenderId, Messages = messages, WithUserName = withUserName.Name });
                    }
                    else
                    {
                        group.Messages.Add(new Message() { SenderId = mesajPrimit.SenderId, Receipt = user.UserID, MessageText = mesajPrimit.Message, SentTime = mesajPrimit.SentTime });

                    }
                }
                foreach(var group in groups)
                {
                    var messagesSorted = group.Messages.OrderBy(msg => msg.SentTime).ToList();
                    group.Messages = messagesSorted;
                }
                result.SetSuccess(groups);
            }
            else
            {
                result.SetException("user not found");
            }
            return result;
        }
    }

    public class Group
    {
        public int WithUser { get; set; }

        public string WithUserName { get; set; }

        public List<Message> Messages { get; set; }
    }

    public class Message
    {
        public int SenderId { get; set; }

        public int Receipt { get; set; }
        public string MessageText { get; set; }

        public DateTime SentTime { get; set; }
    }
}
