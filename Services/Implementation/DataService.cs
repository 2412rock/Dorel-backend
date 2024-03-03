using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Models.Requests;
using DorelAppBackend.Models.Responses;
using DorelAppBackend.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace DorelAppBackend.Services.Implementation
{
    public class DataService: IDataService
    {
        private DorelDbContext _dorelDbContext;
        private IBlobStorageService _blobStorageService;
        private ILoginService _loginService;

        public DataService(DorelDbContext dorelDbContext, IBlobStorageService blobStorageService, ILoginService loginService)
        {
            _dorelDbContext = dorelDbContext;
            _blobStorageService = blobStorageService;
            _loginService = loginService;
        }

        public Maybe<DBJudetModel[]> GetJudete(string startsWith)
        {
            var result = new Maybe<DBJudetModel[]>();
            if(startsWith.Trim() == "")
            {
                result.SetSuccess(_dorelDbContext.Judete.ToArray());
            }
            else
            {
                result.SetSuccess(_dorelDbContext.Judete.Where(element => element.Name.ToLower().StartsWith(startsWith.ToLower())).ToArray());
            }
            return result;
        }

        public Maybe<DBServiciuModel[]> GetServicii(string startsWith)
        {
            var result = new Maybe<DBServiciuModel[]>();
            if (startsWith.Trim() == "")
            {
                result.SetSuccess(_dorelDbContext.Servicii.ToArray());
            }
            else
            {
                result.SetSuccess(_dorelDbContext.Servicii.Where(element => element.Name.ToLower().StartsWith(startsWith.ToLower())).ToArray());
            }
            return result;
        }

        public Maybe<DBServiciuModel[]> GetServiciiForUser(string email)
        {
            var maybe = new Maybe<DBServiciuModel[]>();
            var user = _dorelDbContext.Users.Where(user => user.Email == email).FirstOrDefault();
            if(user != null)
            {
                var serviciiIds = _dorelDbContext.JunctionServiciuJudete
                .Where(jsj => jsj.UserID == user.UserID)
                .Select(jsj => jsj.ServiciuIdID)
                .ToList();

                var resultList = _dorelDbContext.Servicii
                    .Where(serviciu => serviciiIds.Contains(serviciu.ID))
                    .ToList();

                if (resultList.Count > 0)
                {
                    maybe.SetSuccess(resultList.ToArray());
                }
                else
                {
                    maybe.SetSuccess(new DBServiciuModel[0]);
                }
            }
            else
            {
                maybe.SetException("No user with such email");
            }

            return maybe;
        }

        public async Task<Maybe<string>> AssignServiciu(string userEmail, int serviciuId, int[] judeteIds,string descriere, Imagine[] imagini)
        {
            var response = new Maybe<string>();
            response.SetSuccess("Ok");
            var user = _dorelDbContext.Users.Where(u => u.Email == userEmail).FirstOrDefault();
            if(user != null)
            {
                var junctionExists = _dorelDbContext.JunctionServiciuJudete.Any(e => e.UserID == user.UserID && e.ServiciuIdID == serviciuId);
                if (!junctionExists)
                {
                    foreach (var judetId in judeteIds)
                    {
                        var junction = new JunctionServiciuJudete
                        {
                            UserID = user.UserID,
                            ServiciuIdID = serviciuId,
                            JudetID = judetId,
                            Descriere = descriere,
                        };
                        _dorelDbContext.JunctionServiciuJudete.Add(junction);
                    }
                }
                else
                {
                    response.SetException("Serviciu already added");
                }
                await _dorelDbContext.SaveChangesAsync();
                await PublishImagesForServiciu(imagini, serviciuId, user);
            }
            else
            {
                response.SetException($"No user found with this email {userEmail}");
            }
            
            return response;
        }


        private async Task PublishImagesForServiciu(Imagine[] imagini, int serviciuId, DBUserLoginInfoModel user)
        {
            var pictureIndex = 0;
            foreach (var imagine in imagini)
            {
                var fileName = $"{user.UserID}-{serviciuId}-{pictureIndex}.{imagine.FileExtension}";
                await _blobStorageService.UploadImage(fileName, imagine.FileExtension, imagine.FileType, imagine.FileContentBase64);
                pictureIndex++;
            }
        }
    }
}
