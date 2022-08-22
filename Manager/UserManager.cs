using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using MySqlConnector;
using Nri_Webapplication_Backend.DTO;
using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.Models;
using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Nri_Webapplication_Backend.Managers
{
    public interface IUserManager
    {
        Task<UserManagementModel> FindByIDAsync(int userID);
        Task<List<UserManagementModel>> FindAsync();
        Task<Boolean> InsertAsync(UserManagementModel entity);
        Task<Boolean> UpdateAsync(UserManagementModel entity, int userID);
        Task<Boolean> DeleteAsync(UserManagementModel entity, int userID);
        Task<UserManagementModel> SignInAsync(string username);
        Task<Boolean> UpdatePasswordAsync(UserManagementModel entity, int userID);
        Task<string> FindByIDViewerAsync(int userID);
        Task<Boolean> InsertLoginHistoryAsync(int userID);
    }
    public class UserManager : IUserManager
    {
        public ConnectionHelper Db { get; set; }
        public UserManager(ConnectionHelper Db)
        {

        }


        public async Task<UserManagementModel> FindByIDAsync(int userID)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var query = (from us in  context.Users
                                 from usr in context.UsersRole.Where(role1 => role1.UserRoleId == us.UserRoleId).DefaultIfEmpty()
                                 from usr2 in context.UsersRole.Where(role2 => role2.UserRoleId == us.UserRoleId2).DefaultIfEmpty()
                                 where us.UserId == userID
                                 select new UserManagementModel()
                                 {
                                     userID = us.UserId,
                                     username = us.UserName,
                                     password = us.Password,
                                     isActive = us.IsActive,
                                     userRoleID = us.UserRoleId,
                                     userRoleName = usr.UserRoleName,
                                     createdDate = us.CreatedDate,
                                     updatedDate = us.UpdatedDate,
                                     updatedBy = us.UpdatedBy,
                                     userRoleID2 = us.UserRoleId2,
                                     userRoleName2 = usr2.UserRoleName
                                 });

                    return query.FirstOrDefault();
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<string> FindByIDViewerAsync(int userID)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {
                    var query = (from us in await context.Users.ToListAsync()
                                 join usr in await context.UsersRole.ToListAsync()
                                 on us.UserRoleId equals usr.UserRoleId
                                 where us.UserId == userID && usr.UserRoleId == 3
                                 select new UserManagementModel()
                                 {
                                     userID = us.UserId,
                                     username = us.UserName,
                                     password = us.Password,
                                     isActive = us.IsActive,
                                     userRoleID = us.UserRoleId,
                                     userRoleName = usr.UserRoleName,
                                     createdDate = us.CreatedDate,
                                     updatedDate = us.UpdatedDate,
                                     updatedBy = us.UpdatedBy

                                 }).ToList();

                    if (query.Count() == 0)
                    {
                        return null;
                    }

                    return query.FirstOrDefault().username;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<UserManagementModel>> FindAsync()
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var query = (from us in context.Users
                                 from usr in context.UsersRole.Where(role1 => role1.UserRoleId == us.UserRoleId).DefaultIfEmpty()
                                 from usr2 in context.UsersRole.Where(role2 => role2.UserRoleId == us.UserRoleId2).DefaultIfEmpty()
                                 select new UserManagementModel()
                                 {
                                     userID = us.UserId,
                                     username = us.UserName,
                                     password = us.Password,
                                     isActive = us.IsActive,
                                     userRoleID = us.UserRoleId,
                                     userRoleName = usr.UserRoleName,
                                     createdDate = us.CreatedDate,
                                     updatedDate = us.UpdatedDate,
                                     updatedBy = us.UpdatedBy,
                                     userRoleID2 = us.UserRoleId2,
                                     userRoleName2 = usr2.UserRoleName

                                 }).ToList();

                    if (query != null)
                    {
                        foreach (var result in query)
                        {
                            var getLastLogin = await context.UserLoginHistory.Where(o => o.UserId == result.userID).ToListAsync();

                            if (getLastLogin.Count() > 0)
                            {
                                result.lastLogin = getLastLogin.LastOrDefault().LogInDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                        }
                    }

                    return query;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Boolean> InsertAsync(UserManagementModel entity)
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {
                    var Users = new Users
                    {
                        UserId = entity.userID,
                        UserName = entity.username,
                        Password = entity.password,
                        IsActive = entity.isActive,
                        UserRoleId = entity.userRoleID,
                        CreatedDate = entity.createdDate,
                        CreatedBy = entity.createdBy,
                        UpdatedDate = entity.updatedDate,
                        UpdatedBy = entity.updatedBy,
                        UserRoleId2 = entity.userRoleID2
                    };

                    await context.AddAsync(Users);

                    await context.SaveChangesAsync();
                }

                return true;


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<Boolean> UpdateAsync(UserManagementModel entity, int userID)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var user = await context.Users.Where(o => o.UserId == userID).FirstOrDefaultAsync();

                    user.UserRoleId = entity.userRoleID;
                    user.IsActive = entity.isActive;
                    user.UpdatedDate = DateTime.Now;
                    user.UpdatedBy = entity.updatedBy;
                    user.UserRoleId2 = entity.userRoleID2;

                    context.SaveChanges();

                    return true;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<Boolean> DeleteAsync(UserManagementModel entity, int userID)
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {

                    var user = await context.Users.Where(o => o.UserId == userID).FirstOrDefaultAsync();

                    user.IsActive = entity.isActive;
                    user.UpdatedDate = DateTime.Now;
                    user.UpdatedBy = entity.updatedBy;

                    context.SaveChanges();

                    return true;
                }


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //Sign in
        public async Task<UserManagementModel> SignInAsync(string username)
        {
            try
            {
                using (var context = new seac_webapplicationContext())
                {

                    var query = (from us in await context.Users.ToListAsync()
                                 join usr in await context.UsersRole.ToListAsync()
                                 on us.UserRoleId equals usr.UserRoleId
                                 where us.UserName == username && us.IsActive == 1
                                 select new UserManagementModel()
                                 {
                                     userID = us.UserId,
                                     username = us.UserName,
                                     password = us.Password,
                                     isActive = us.IsActive,
                                     userRoleID = us.UserRoleId,
                                     userRoleName = usr.UserRoleName,
                                     createdDate = us.CreatedDate,
                                     updatedDate = us.UpdatedDate,
                                     updatedBy = us.UpdatedBy


                                 }).FirstOrDefault();



                    return query;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Boolean> InsertLoginHistoryAsync(int userID)
        {
            try
            {



                using (var context = new seac_webapplicationContext())
                {
                    var Users = new UserLoginHistory
                    {
                        UserId = userID,
                        LogInDateTime = DateTime.Now
                    };


                    await context.AddAsync(Users);

                    await context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Update password
        public async Task<Boolean> UpdatePasswordAsync(UserManagementModel entity, int userID)
        {
            try
            {

                using (var context = new seac_webapplicationContext())
                {

                    var user = await context.Users.Where(o => o.UserId == userID).FirstOrDefaultAsync();

                    user.Password = entity.password;

                    context.SaveChanges();

                    return true;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }



    }
}



