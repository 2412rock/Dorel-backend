using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Models.Responses;
using DorelAppBackend.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace DorelAppBackend.Services.Implementation
{
    public class DataService: IDataService
    {
        private DorelDbContext _dorelDbContext;

        public DataService(DorelDbContext dorelDbContext)
        {
            _dorelDbContext = dorelDbContext;
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

            // Add the junction entity to the respective collections
            user.JunctionServicii.Add(junction);
            serviciu.JunctionServicii.Add(junction);
        }

        private void AssignJudet(DBUserLoginInfoModel user, DBJudetModel judet)
        {
            // Create a new JunctionServicii instance
            var junction = new JunctionJudete()
            {
                UserID = user.UserID,
                JudetID = judet.JudetID
            };

            // Add the junction entity to the respective collections
            user.JunctionJudete.Add(junction);
            judet.JunctionJudete.Add(junction);

           // Save changes to the database
        }

        public Maybe<string> AssignUserServiciu(string userEmail, string[] servicii, string[] judete)
        {
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
                        _dorelDbContext.Servicii.Add(new DBServiciuModel() { Name = serviciuItem.ToLower() });
                        _dorelDbContext.SaveChanges();
                        serviciu = _dorelDbContext.Servicii.FirstOrDefault(s => s.Name.ToLower() == serviciuItem.ToLower());
                        if(serviciu != null)
                        {
                            AssignServiciu(user, serviciu);
                        }
                    }
                }

                foreach (var judetItem in judete)
                {
                    var judet = _dorelDbContext.Judete.FirstOrDefault(s => s.Name.ToLower() == judetItem.ToLower());

                    if (judet != null)
                    {
                        AssignJudet(user, judet);
                    }
                    else
                    {
                        result.SetException($"This judet does not exist {judetItem}");
                        _dorelDbContext.ChangeTracker.Clear();
                        return result;
                    }
                }

            }
            _dorelDbContext.SaveChanges();
            result.SetSuccess("Assigned success");
            return result;
        }
    }
}
