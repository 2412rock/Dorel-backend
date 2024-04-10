using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Models.Requests;
using DorelAppBackend.Models.Responses;
using DorelAppBackend.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Minio.Exceptions;
using Newtonsoft.Json.Linq;
using System.Runtime.ConstrainedExecution;
using static System.Net.Mime.MediaTypeNames;

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

        public async Task<Maybe<List<SearchResultResponse>>> GetServiciiForUserAsSearchResults(string email)
        {
            var maybe = new Maybe<List<SearchResultResponse>>();

            var user = await _dorelDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                maybe.SetException("User does not exist");
                return maybe;
            }
            var result = await _dorelDbContext.JunctionServiciuJudete.Where(x => x.UserID == user.UserID).ToListAsync();

            var listSearchResults = new List<SearchResultResponse>();
            foreach (var junction in result)
            {

                var serviciu = await _dorelDbContext.Servicii.FirstOrDefaultAsync(x => x.ID == junction.ServiciuIdID);
                var judet = await _dorelDbContext.Judete.FirstOrDefaultAsync(x => x.ID == junction.JudetID);
                var userOfServiciu = await _dorelDbContext.Users.FirstOrDefaultAsync(u => u.UserID == junction.UserID);

                if (serviciu != null && userOfServiciu != null && judet != null)
                {
                    var imagineCover = await _blobStorageService.DownloadImage(_blobStorageService.GetFileName(userOfServiciu.UserID, serviciu.ID, 0));
                    var searchResult = new SearchResultResponse() { UserName = userOfServiciu.Name, Descriere = junction.Descriere, ServiciuName = serviciu.Name, JudetName = judet.Name , StarsAverage = 5, ImagineCover = imagineCover, UserId = junction.UserID, ServiciuId = junction.ServiciuIdID, JudetId = junction.JudetID };
                    listSearchResults.Add(searchResult);
                }
            }
            maybe.SetSuccess(listSearchResults);

            return maybe;
        }

        public async Task<Maybe<string>> AssignServiciu(string userEmail, int serviciuId, int[] judeteIds,string descriere, Imagine[] imagini)
        {
            var response = new Maybe<string>();
            if(imagini.Length > 10 && descriere.Length > 250)
            {
                response.SetException("Exceed limit of images or description");
                return response;
            }
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

        public async Task<Maybe<string>> EditServiciu(string userEmail, int serviciuId, int[] judeteIds, string descriere, Imagine[] imagini)
        {
            var response = new Maybe<string>();
            response.SetSuccess("Ok");
            if (imagini.Length > 10 && descriere.Length > 250)
            {
                response.SetException("Exceed limit of images or description");
                return response;
            }
            var user = _dorelDbContext.Users.Where(u => u.Email == userEmail).FirstOrDefault();
            if (user != null)
            {
                var junctionExists = _dorelDbContext.JunctionServiciuJudete.Any(e => e.UserID == user.UserID && e.ServiciuIdID == serviciuId);
                if (junctionExists)
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
                        var judetEntryExists = _dorelDbContext.JunctionServiciuJudete.Any(e => e.JudetID == judetId && e.UserID == user.UserID && e.ServiciuIdID == serviciuId);
                        if (judetEntryExists)
                        {
                            _dorelDbContext.JunctionServiciuJudete.Update(junction);
                        }
                        else
                        {
                            _dorelDbContext.JunctionServiciuJudete.Add(junction);
                        }
                    }
                }
                else
                {
                    response.SetException("Invalid edit request");
                    return response;
                }
                try
                {
                    await PublishImagesForServiciu(imagini, serviciuId, user, true);
                }
                catch(Exception e)
                {
                    DiscardDbChanges();
                    response.SetException($"Something went wrong {e.Message}");
                    return response;
                }
                
                await _dorelDbContext.SaveChangesAsync();
            }
            else
            {
                response.SetException($"No user found with this email {userEmail}");
            }

            return response;
        }

        public Maybe<string> DeleteUserServiciu(string userEmail, int serviciuId) 
        {
            var response = new Maybe<string>();
            response.SetSuccess("Ok");
            var user = _dorelDbContext.Users.Where(u => u.Email == userEmail).FirstOrDefault();
            if (user != null)
            {
                var junctionRows = _dorelDbContext.JunctionServiciuJudete.Where(e => e.UserID == user.UserID && e.ServiciuIdID == serviciuId);
                if (junctionRows.Count() > 0)
                {
                    _dorelDbContext.JunctionServiciuJudete.RemoveRange(junctionRows);
                    _dorelDbContext.SaveChanges();
                    response.SetSuccess("Success deleting serviciu for user");
                }
                else
                {
                    response.SetException("No serviciu found to delte");
                }
            }
            else
            {
                response.SetException("No user found");
            }
            return response;
        }

        private void DiscardDbChanges()
        {
            foreach (var entry in _dorelDbContext.ChangeTracker.Entries().ToList())
            {
                entry.State = EntityState.Detached;
            }
        }

        public async Task<Maybe<List<Imagine>>> GetImaginiServiciuUser(int serviciuId, string userEmail)
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

        public async Task<Maybe<List<Imagine>>> GetImaginiServiciu(int serviciuId, int judetId,string userEmail)
        {
            var imgList = new List<Imagine>();
            var maybe = new Maybe<List<Imagine>>();
            var pictureIndex = 0;
            var user = _dorelDbContext.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user != null)
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
                    catch (Exception e)
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

        public async Task<Maybe<List<SearchResultResponse>>> GetServiciiForJudet(int serviciuId, int judetId, int pageNumber)
        {
            const int PAGE_SIZE = 20;
            var maybe = new Maybe<List<SearchResultResponse>>();
            List<JunctionServiciuJudete> result;
            if(serviciuId != -1 && judetId != -1) 
            {
                result = await _dorelDbContext.JunctionServiciuJudete.Where(x => x.ServiciuIdID == serviciuId && x.JudetID == judetId).Skip(pageNumber * PAGE_SIZE).Take(PAGE_SIZE).ToListAsync();

            }
            else if(judetId != -1)
            {
                result = await _dorelDbContext.JunctionServiciuJudete.Where(x => x.JudetID == judetId).Skip(pageNumber * PAGE_SIZE).Take(PAGE_SIZE).ToListAsync();
            }
            else if(serviciuId != -1)
            {
                result = await _dorelDbContext.JunctionServiciuJudete.Where(x => x.ServiciuIdID == serviciuId).Skip(pageNumber * PAGE_SIZE).Take(PAGE_SIZE).ToListAsync();
            }
            else if(serviciuId == -1 && judetId == -1)
            {
                result = await _dorelDbContext.JunctionServiciuJudete.ToListAsync();
            }
            else
            {
                maybe.SetException("Invalid query");
                return maybe;
            }
            var listSearchResults = new List<SearchResultResponse>();
            foreach (var junction in result)
            {
                var serviciu = await _dorelDbContext.Servicii.FirstOrDefaultAsync(x => x.ID == junction.ServiciuIdID);
                var judet = await _dorelDbContext.Judete.FirstOrDefaultAsync(x => x.ID == junction.JudetID);
                var userOfServiciu = await _dorelDbContext.Users.FirstOrDefaultAsync(u => u.UserID == junction.UserID);

                if (serviciu != null && userOfServiciu != null && judet != null)
                {
                    var reviews = _dorelDbContext.Reviews.Where(r => r.ServiciuId == junction.ServiciuIdID && r.ReviewedUserId == userOfServiciu.UserID);
                    var numberOfReviews = reviews.Count();
                    var imagineCover = await _blobStorageService.DownloadImage(_blobStorageService.GetFileName(userOfServiciu.UserID, serviciu.ID, 0));
                    var searchResult = new SearchResultResponse() { UserName = userOfServiciu.Name, Descriere = junction.Descriere,
                        ServiciuName = serviciu.Name, JudetName = judet.Name ,StarsAverage = 5,
                        ImagineCover = imagineCover, UserId = junction.UserID, ServiciuId = junction.ServiciuIdID,
                        JudetId = junction.JudetID, NumberOfReviews = numberOfReviews };
                    listSearchResults.Add(searchResult);
                }
            }
            maybe.SetSuccess(listSearchResults);

            return maybe;
        }

        public async Task<Maybe<List<Imagine>>> GetImaginiForServiciuOfUser(int serviciuId, int judetId, int userId)
        {
            var maybe = new Maybe<List<Imagine>>();
            var user = await _dorelDbContext.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user != null)
            {
                var result = await _dorelDbContext.JunctionServiciuJudete.FirstOrDefaultAsync(x => x.ServiciuIdID == serviciuId && x.JudetID == judetId && x.UserID == userId);
                var imagini = new List<Imagine>();
                if(result != null)
                {
                    var pictureIndex = 0;
                    while (true)
                    {
                        try
                        {
                            var fileName = _blobStorageService.GetFileName(user.UserID, serviciuId, pictureIndex);
                            var imagine = await _blobStorageService.DownloadImage(fileName);
                            imagini.Add(imagine);
                            pictureIndex++;
                        }
                        catch(ObjectNotFoundException e)
                        {
                            maybe.SetSuccess(imagini);
                            return maybe;

                        }
                        catch(Exception e)
                        {
                            maybe.SetException($"Could not download image {e.Message}");
                            return maybe;
                        }
                    }
                }
                maybe.SetException("Couldnt find junction");
                return maybe;
            }
            maybe.SetException("No user found with that id");

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


        private async Task PublishImagesForServiciu(Imagine[] imagini, int serviciuId, DBUserLoginInfoModel user, bool edit=false)
        {
            var pictureIndex = 0;
            if (edit)
            {
                //remove all pictures for serviciu
                while (true)
                {
                    var fileName = _blobStorageService.GetFileName(user.UserID, serviciuId, pictureIndex);
                    try
                    {
                        await _blobStorageService.DeleteImage(fileName);
                    }
                    catch(ObjectNotFoundException e)
                    {
                        break;
                    }
                    catch(Exception e)
                    {
                        throw;
                    }
                    
                    pictureIndex++;
                }
                
            }
            pictureIndex = 0;
            foreach (var imagine in imagini)
            {
                var fileName = _blobStorageService.GetFileName(user.UserID, serviciuId, pictureIndex);
                await _blobStorageService.UploadImage(fileName, imagine.FileType, imagine.FileContentBase64);
                pictureIndex++;
            }
        }
    }
}
