﻿using DorelAppBackend.Models;
using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Models.Requests;
using DorelAppBackend.Models.Responses;
using DorelAppBackend.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Minio.Exceptions;
using Newtonsoft.Json.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
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
                result.SetSuccess(_dorelDbContext.Judete.Take(4).ToArray());
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
                result.SetSuccess(_dorelDbContext.Servicii.Take(4).ToArray());
            }
            else
            {
                result.SetSuccess(_dorelDbContext.Servicii.Where(element => element.Name.ToLower().StartsWith(startsWith.ToLower())).ToArray());
            }
            return result;
        }

        public Maybe<DBServiciuModel[]> GetServiciiForUser(string email, bool ofer)
        {
            var maybe = new Maybe<DBServiciuModel[]>();
            var user = _dorelDbContext.Users.Where(user => user.Email == email).FirstOrDefault();
            if(user != null)
            {
                var serviciiIds = _dorelDbContext.JunctionServiciuJudete
                .Where(jsj => jsj.UserID == user.UserID && jsj.Ofer == ofer)
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

        public async Task<Maybe<List<SearchResultResponse>>> GetServiciiForUserAsSearchResults(string email, bool ofer)
        {
            var maybe = new Maybe<List<SearchResultResponse>>();

            var user = await _dorelDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                maybe.SetException("User does not exist");
                return maybe;
            }
            var result = await _dorelDbContext.JunctionServiciuJudete.Where(x => x.UserID == user.UserID && x.Ofer == ofer).ToListAsync();

            var listSearchResults = new List<SearchResultResponse>();
            foreach (var junction in result)
            {

                var serviciu = await _dorelDbContext.Servicii.FirstOrDefaultAsync(x => x.ID == junction.ServiciuIdID);
                var judet = await _dorelDbContext.Judete.FirstOrDefaultAsync(x => x.ID == junction.JudetID);
                var userOfServiciu = await _dorelDbContext.Users.FirstOrDefaultAsync(u => u.UserID == junction.UserID);

                if (serviciu != null && userOfServiciu != null && judet != null)
                {
                    var imagineCover = await _blobStorageService.DownloadImage(_blobStorageService.GetFileName(userOfServiciu.UserID, serviciu.ID, ofer,0));

                    var searchResult = new SearchResultResponse() { UserName = userOfServiciu.Name, Descriere = junction.Descriere, ServiciuName = serviciu.Name, JudetName = judet.Name , StarsAverage = junction.Rating != null ? (decimal)junction.Rating : 0, ImagineCover = imagineCover, UserId = junction.UserID, ServiciuId = junction.ServiciuIdID, JudetId = junction.JudetID, Ofer = ofer };
                    listSearchResults.Add(searchResult);
                }
            }
            maybe.SetSuccess(listSearchResults);

            return maybe;
        }

        public async Task<Maybe<string>> AssignServiciu(string userEmail, int serviciuId, int[] judeteIds,string descriere, Imagine[] imagini, bool ofer, string phone, string contactEmail)
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
                var junctionExists = _dorelDbContext.JunctionServiciuJudete.Any(e => e.UserID == user.UserID && e.ServiciuIdID == serviciuId && e.Ofer == ofer);
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
                            Ofer = ofer,
                            Phone = phone,
                            Email = contactEmail
                        };
                        _dorelDbContext.JunctionServiciuJudete.Add(junction);
                    }
                }
                else
                {
                    response.SetException("Serviciu already added");
                }
                try
                {
                    await _dorelDbContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw;
                }
                
                await PublishImagesForServiciu(imagini, serviciuId, user, ofer);
            }
            else
            {
                response.SetException($"No user found with this email {userEmail}");
            }
            
            return response;
        }

        public async Task<Maybe<string>> EditServiciu(string userEmail, int serviciuId, int[] newJudeteIds, string descriere, Imagine[] imagini, bool ofer, string phone, string contactEmail)
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
                var existingJunctions = await _dorelDbContext.JunctionServiciuJudete.Where(e => e.UserID == user.UserID && e.ServiciuIdID == serviciuId && e.Ofer == ofer).ToListAsync();
                if (existingJunctions.Count() > 0)
                {
                    decimal rating = 0;
                    foreach (var junction in existingJunctions)
                    {
                        rating = junction.Rating.HasValue ? junction.Rating.Value : 0;
                        _dorelDbContext.JunctionServiciuJudete.Remove(junction);
                    }
                    foreach(var judetId in newJudeteIds)
                    {
                        var junction = new JunctionServiciuJudete()
                        {
                            Descriere = descriere,
                            ServiciuIdID = serviciuId,
                            UserID = user.UserID,
                            JudetID = judetId,
                            Rating = rating,
                            Ofer = ofer,
                            Phone = phone,
                            Email = contactEmail
                        };
                        await _dorelDbContext.AddAsync(junction);
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

        private async Task DeletePictures(DBUserLoginInfoModel user, JunctionServiciuJudete junction, bool ofer)
        {
            var pictureIndex = 0;
            while (true)
            {
                try
                {
                    await _blobStorageService.DeleteImage(_blobStorageService.GetFileName(user.UserID, junction.ServiciuIdID, ofer, pictureIndex));
                }
                catch (ObjectNotFoundException e)
                {
                    break;
                }
                catch
                {
                    throw;
                }
                pictureIndex++;
            }
        }

        public async Task<Maybe<string>> DeleteUserServiciu(string userEmail, int serviciuId, bool ofer) 
        {
            var response = new Maybe<string>();
            response.SetSuccess("Ok");
            var user = _dorelDbContext.Users.Where(u => u.Email == userEmail).FirstOrDefault();
            if (user != null)
            {
                var junctionRows = await _dorelDbContext.JunctionServiciuJudete.Where(e => e.UserID == user.UserID && e.ServiciuIdID == serviciuId && e.Ofer == ofer).ToListAsync();
                foreach (var junctionRow in junctionRows)
                {
                    try
                    {
                        await DeletePictures(user, junctionRow, ofer);
                    }
                    catch(Exception e)
                    {
                        response.SetException("Something went wrong deleting data");
                        DiscardDbChanges();
                        return response;
                    }
                }
                _dorelDbContext.JunctionServiciuJudete.RemoveRange(junctionRows);
                await _dorelDbContext.SaveChangesAsync();
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

        public async Task<Maybe<List<Imagine>>> GetImaginiServiciuUser(int serviciuId, string userEmail, bool ofer)
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
                        var fileName = _blobStorageService.GetFileName(user.UserID, serviciuId, ofer,pictureIndex);
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

        public async Task<Maybe<List<SearchResultResponse>>> GetServiciiForJudet(int serviciuId, int judetId, int pageNumber, bool ofer)
        {
            const int PAGE_SIZE = 20;
            var maybe = new Maybe<List<SearchResultResponse>>();
            List<JunctionServiciuJudete> result;

            if (serviciuId != -1 && judetId != -1) 
            {
                result = await _dorelDbContext.JunctionServiciuJudete.Where(x => x.ServiciuIdID == serviciuId && x.JudetID == judetId && x.Ofer == ofer).Skip(pageNumber * PAGE_SIZE).Take(PAGE_SIZE).ToListAsync();

            }
            else if(judetId != -1)
            {
                result = await _dorelDbContext.JunctionServiciuJudete.Where(x => x.JudetID == judetId && x.Ofer == ofer).Skip(pageNumber * PAGE_SIZE).Take(PAGE_SIZE).ToListAsync();
            }
            else if(serviciuId != -1)
            {
                result = await _dorelDbContext.JunctionServiciuJudete.Where(x => x.ServiciuIdID == serviciuId && x.Ofer == ofer).Skip(pageNumber * PAGE_SIZE).Take(PAGE_SIZE).ToListAsync();
            }
            else if(serviciuId == -1 && judetId == -1)
            {
                Console.WriteLine("Getting data");
                result = await _dorelDbContext.JunctionServiciuJudete.Where(x => x.Ofer == ofer).ToListAsync();
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
                    var imagineCover = await _blobStorageService.DownloadImage(_blobStorageService.GetFileName(userOfServiciu.UserID, serviciu.ID, ofer,0));
                    var searchResult = new SearchResultResponse() { UserName = userOfServiciu.Name, Descriere = junction.Descriere, Ofer = ofer,
                        ServiciuName = serviciu.Name, JudetName = judet.Name ,StarsAverage = junction.Rating != null ? (decimal)junction.Rating : 0,
                        ImagineCover = imagineCover, UserId = junction.UserID, UserEmail = userOfServiciu.Email, ServiciuId = junction.ServiciuIdID,
                        JudetId = junction.JudetID, NumberOfReviews = numberOfReviews, Phone = junction.Phone, Email = junction.Email };
                    listSearchResults.Add(searchResult);
                }
            }
            maybe.SetSuccess(listSearchResults);
            return maybe;
        }



        public async Task<List<JunctionServiciuJudete>> GetAllJunctions()
        {
            return await _dorelDbContext.JunctionServiciuJudete.ToListAsync();
        }

        public async Task<Maybe<List<Imagine>>> GetImaginiForServiciuOfUser(int serviciuId, int userId, bool ofer)
        {
            var maybe = new Maybe<List<Imagine>>();
            var user = await _dorelDbContext.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user != null)
            {
                var result = await _dorelDbContext.JunctionServiciuJudete.FirstOrDefaultAsync(x => x.ServiciuIdID == serviciuId && x.UserID == userId && x.Ofer == ofer);
                var imagini = new List<Imagine>();
                if(result != null)
                {
                    var pictureIndex = 0;
                    while (true)
                    {
                        try
                        {
                            var fileName = _blobStorageService.GetFileName(user.UserID, serviciuId, ofer,pictureIndex);
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

        public Maybe<DescriereAndContact> GetDescriereAndContactForServiciu(int serviciuId, string userEmail)
        {
            var maybe = new Maybe<DescriereAndContact>();
            var user = _dorelDbContext.Users.Where(u => u.Email == userEmail).FirstOrDefault();
            if (user != null)
            {
                var result = _dorelDbContext.JunctionServiciuJudete.FirstOrDefault(x => x.ServiciuIdID == serviciuId && x.UserID == user.UserID);
                if(result != null)
                {
                    var data = new DescriereAndContact()
                    {
                        Descriere = result.Descriere,
                        Phone = result.Phone,
                        Email = result.Email
                    };
                    maybe.SetSuccess(data);
                    return maybe;
                }

                maybe.SetException("No description found for user");
                return maybe;
            }
            maybe.SetException($"No user with email {userEmail}");
            return maybe;
        }


        private async Task PublishImagesForServiciu(Imagine[] imagini, int serviciuId, DBUserLoginInfoModel user, bool ofer,bool edit=false)
        {
            var pictureIndex = 0;
            if (edit)
            {
                //remove all pictures for serviciu
                while (true)
                {
                    var fileName = _blobStorageService.GetFileName(user.UserID, serviciuId, ofer, pictureIndex);
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
                var fileName = _blobStorageService.GetFileName(user.UserID, serviciuId, ofer,pictureIndex);
                await _blobStorageService.UploadImage(fileName, imagine.FileType, imagine.FileContentBase64);
                pictureIndex++;
            }
        }

        public async Task<Maybe<List<string>>> GetAllUsers()
        {
            var result = new Maybe<List<string>>();
            var users = await _dorelDbContext.Users.Select(x => x.Email).ToListAsync();
            result.SetSuccess(users);
            return result;
        }
    }
}
