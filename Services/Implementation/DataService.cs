using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Models.Requests;
using DorelAppBackend.Models.Responses;
using DorelAppBackend.Services.Interface;
using Microsoft.EntityFrameworkCore;

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

        private void AssignServiciu(DBUserLoginInfoModel user, DBServiciuModel serviciu)
        {
            // Create a new JunctionServicii instance
            var junction = new JunctionServicii
            {
                UserID = user.UserID,
                ServiciuIdID = serviciu.ServiciuIdID
            };

            var junctionExists = _dorelDbContext.JunctionServicii.Any(e => e.UserID == junction.UserID && e.ServiciuIdID == junction.ServiciuIdID);
            if (!junctionExists)
            {
                user.JunctionServicii.Add(junction);
                serviciu.JunctionServicii.Add(junction);
            }
            else
            {
                throw new Exception("Junction serviciu exists");
            }
            
            
        }

        private void AssignJudet(DBUserLoginInfoModel user, DBJudetModel judet)
        {
            // Create a new JunctionServicii instance
            var junction = new JunctionJudete()
            {
                UserID = user.UserID,
                JudetID = judet.JudetID
            };

            var junctionExists = _dorelDbContext.JunctionJudete.Any(e => e.UserID == junction.UserID && e.JudetID == junction.JudetID);
            if (!junctionExists)
            {
                user.JunctionJudete.Add(junction);
                judet.JunctionJudete.Add(junction);
            }
            else
            {
                throw new Exception("Junction judet exists");
            }
            
        }

        private async Task PublishImagesForServiciu(ServiciiAndImagini[] serviciiAndImagini, string serviciuName ,int serviciuId, DBUserLoginInfoModel user)
        {
            foreach(var serviciuAndImagini in serviciiAndImagini)
            {
                foreach (var serviciu in serviciuAndImagini.Servicii)
                {
                    if (serviciu == serviciuName)
                    {
                        var pictureIndex = 0;
                        foreach (var imagine in serviciuAndImagini.Imagini)
                        {
                            var fileName = $"{user.UserID}-{serviciuId}-{pictureIndex}.{imagine.FileExtension}";
                            await _blobStorageService.UploadImage(fileName, imagine.FileExtension, imagine.FileType, imagine.FileContentBase64);
                            pictureIndex++;
                        }
                    }
                }
            }
        }

        public async Task<Maybe<string>> AssignUserServiciu(string token, string[] servicii, string[] judete, ServiciiAndImagini[] serviciiAndImagini)
        {
            var userEmail = _loginService.GetEmailFromToken(token);
            var result = new Maybe<string>();
            // Assuming you have instances of User and Serviciu entities
            var user = _dorelDbContext.Users.Where(u => u.Email == userEmail).FirstOrDefault(); // Fetch or create the user
            
            if(user != null)
            {
                foreach (var serviciuItem in servicii)
                {
                    var serviciu = _dorelDbContext.Servicii.FirstOrDefault(s => s.Name.ToLower() == serviciuItem.ToLower());

                    if (serviciu == null)
                    {
                        // Serviciu does not exist, create it
                        _dorelDbContext.Servicii.Add(new DBServiciuModel() { Name = serviciuItem.ToLower() });
                        _dorelDbContext.SaveChanges();
                        serviciu = _dorelDbContext.Servicii.FirstOrDefault(s => s.Name.ToLower() == serviciuItem.ToLower());
                        
                    }

                    if (serviciu != null)
                    {
                        try
                        {
                            AssignServiciu(user, serviciu);
                        }
                        catch(Exception e)
                        {
                            result.SetException($"Failed to assign serviciu: {e.Message}");
                            return result;
                        }
                        
                    }
                    else
                    {
                        result.SetException($"Serviciu could not be found after it was inserted: {serviciuItem}");
                        return result;
                    }

                    try
                    {
                        await PublishImagesForServiciu(serviciiAndImagini, serviciu.Name, serviciu.ServiciuIdID, user);
                    }
                    catch (Exception e)
                    {
                        result.SetException($"Exception publishing images for servicii {e.Message}");
                        return result;
                    }
                }

                foreach (var judetItem in judete)
                {
                    var judet = _dorelDbContext.Judete.FirstOrDefault(s => s.Name.ToLower() == judetItem.ToLower());

                    if (judet != null)
                    {
                        try
                        {
                            AssignJudet(user, judet);
                        }
                        catch(Exception e)
                        {
                            result.SetException($"Failed to assign judet: {e.Message}");
                            return result;
                        }
                    }
                    else
                    {
                        result.SetException($"This judet does not exist {judetItem}");
                        _dorelDbContext.ChangeTracker.Clear();
                        return result;
                    }
                } 
            }
            else
            {
                result.SetException("User does not exist");
                return result;
            }
            try
            {
                _dorelDbContext.SaveChanges();
            }
            catch(Exception e)
            {
                result.SetException($"Failed to save db changes {e.Message} {e.InnerException.Message}");
                return result;
            }
            
            result.SetSuccess("Assigned success");
            return result;
        }
    }
}
