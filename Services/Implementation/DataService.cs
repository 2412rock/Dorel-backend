using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Models.Requests;
using DorelAppBackend.Models.Responses;
using DorelAppBackend.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Minio.Exceptions;
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
                maybe.SetException("No user found");
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

        public async Task<Maybe<List<Imagine>>> GetImaginiServiciu(int serviciuId, string userEmail)
        {
            var imgList = new List<Imagine>();
            var maybe = new Maybe<List<Imagine>>();
            var pictureIndex = 0;
            var user = _dorelDbContext.Users.FirstOrDefault(u => u.Email == userEmail);   
            if(user != null)
            {
                while (true)
                {
                    try
                    {
                        var fileName = _blobStorageService.GetFileName(user.UserID, serviciuId, pictureIndex);
                        var img = await _blobStorageService.DownloadImage(fileName);
                        imgList.Add(img);
                        pictureIndex++;
                    }
                    catch (ObjectNotFoundException e)
                    {
                        maybe.SetSuccess(imgList);
                        return maybe;
                    }
                    catch(Exception e)
                    {
                        maybe.SetException($"Something went wrong downloading images {e.Message}");
                        return maybe;
                    }
                }
            }
            maybe.SetException("No user found");
            return maybe;
        }

        public Maybe<List<DBJudetModel>> GetJudeteForServiciu(int serviciuId, string userEmail)
        {
            var maybe = new Maybe<List<DBJudetModel>>();
            var user = _dorelDbContext.Users.Where(u => u.Email == userEmail).FirstOrDefault();
            if(user != null)
            {
                var result = _dorelDbContext.JunctionServiciuJudete.Where(x => x.ServiciuIdID == serviciuId && x.UserID == user.UserID).ToArray();
                var judete = new List<DBJudetModel>();
                foreach(var item in result)
                {
                    var judet = _dorelDbContext.Judete.Where(x => x.ID == item.JudetID).FirstOrDefault();
                    if(judet != null)
                    {
                        judete.Add(judet);
                    }
                    
                }
                maybe.SetSuccess(judete);
                return maybe;
            }
            maybe.SetException($"No user with email {userEmail}");
            return maybe;
        }

        public Maybe<string> GetDescriereForServiciu(int serviciuId, string userEmail)
        {
            var maybe = new Maybe<string>();
            var user = _dorelDbContext.Users.Where(u => u.Email == userEmail).FirstOrDefault();
            if (user != null)
            {
                var result = _dorelDbContext.JunctionServiciuJudete.FirstOrDefault(x => x.ServiciuIdID == serviciuId && x.UserID == user.UserID);
                if(result != null)
                {
                    maybe.SetSuccess(result.Descriere);
                    return maybe;
                }

                maybe.SetException("No description found for user");
                return maybe;
            }
            maybe.SetException($"No user with email {userEmail}");
            return maybe;
        }


        private async Task PublishImagesForServiciu(Imagine[] imagini, int serviciuId, DBUserLoginInfoModel user)
        {
            var pictureIndex = 0;
            foreach (var imagine in imagini)
            {
                var fileName = _blobStorageService.GetFileName(user.UserID, serviciuId, pictureIndex);
                await _blobStorageService.UploadImage(fileName, imagine.FileType, imagine.FileContentBase64);
                pictureIndex++;
            }
        }
    }
}
