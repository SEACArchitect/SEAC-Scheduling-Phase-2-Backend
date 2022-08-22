using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Nri_Webapplication_Backend.DTO;
using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Managers
{
    public interface IUserRoleManager
    {
        Task<List<UserRoleModel>> FindAsync();
        Task<List<ModManagerModel>> FindModManager();

    }
    public class UserRoleManager : IUserRoleManager
    {
        public ConnectionHelper Db { get; set; }
        public UserRoleManager(ConnectionHelper Db)
        {
            this.Db = Db;
        }

        public async Task<List<UserRoleModel>> FindAsync()
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var query = (from ur in await context.UsersRole.ToListAsync()
                                 select new UserRoleModel()
                                 {
                                     id = ur.UserRoleId,
                                     value = ur.UserRoleName

                                 }).ToList();

                    return query;

                }
            }
            catch (Exception ex)
            {
                this.Db.connection.Close();
                throw ex;
            }
        }

        public async Task<List<ModManagerModel>> FindModManager()
        {
            MySqlCommand cmd = new MySqlCommand()
            {
                CommandText = "select users.* from users inner join users_role on users.UserRoleID2 = users_role.UserRoleID where users_role.UserRoleName = 'Moderator Manager' and users.IsActive = 1; ",
                Connection = Db.connection
            };
            try
            {
                MySqlDataReader reader;
                await Db.connection.OpenAsync();
                reader = await cmd.ExecuteReaderAsync();
                List<ModManagerModel> lmodM = new List<ModManagerModel>();
                while(await reader.ReadAsync())
                {
                    ModManagerModel modM = new ModManagerModel()
                    {
                        Email = reader.IsDBNull("UserName") ? null : reader["UserName"].ToString(),
                        RoleId2 = (int)reader["UserRoleID2"]
                    };
                    lmodM.Add(modM);
                }
                await reader.CloseAsync();

                return lmodM;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                await this.Db.connection.CloseAsync();
            }
        }



    }
}
