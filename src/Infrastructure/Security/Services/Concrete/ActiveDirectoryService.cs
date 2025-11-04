
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Security.Entities;
using Security.Services.Contracts;
using Shared.Exceptions;

namespace Security.Services.Concrete
{
    public class ActiveDirectoryService : IActiveDirectoryService
    {
        private readonly ILogger<ActiveDirectoryService> _logger;

        public ActiveDirectoryService(ILogger<ActiveDirectoryService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Method to authenticate the user in Domain Active Directory
        /// </summary>
        /// <param name="email">Domain user email</param>
        /// <param name="password">Domain Password</param>
        /// <returns>True if user is Valid or False is user is not valid</returns>
        public async Task<bool> Authenticate(string email, string password)
        {
            try
            {
                var host = email.Split('@')[1];

                using var pc = new PrincipalContext(ContextType.Domain!, host!);

                var isValid = await Task.Run(() => pc.ValidateCredentials(email, password, ContextOptions.Negotiate)).ConfigureAwait(false);

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validate if user Mail Exist in the Azure Active Directory
        /// </summary>
        /// <param name="userMail">User Mail to check </param>
        /// <returns>true if exist and false if not</returns>
        public async Task<bool> UserExist(string usermail)
        {
            try
            {
                _ = new DirectoryEntry("LDAP://mardom-dc-fs03");
                var retProps = new[] { "SamAccountName", "mail" };
                var search = await Task.Run(() =>
                    new DirectorySearcher { Filter = string.Format("(mail={0})", usermail) }
                    ).ConfigureAwait(false);
                search.PropertiesToLoad.AddRange(retProps);

                var result = await Task.Run(() => search.FindOne()).ConfigureAwait(false);

                return result != null;
            }
            catch (COMException ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Get user data from Azure Active Directory
        /// </summary>
        /// <param name="usrName">User email</param>
        /// <returns>[UserADDto] Active Directory user Data</returns>
        public async Task<UserAzureAD> GetUser(string username)
        {
            try
            {
                _ = new DirectoryEntry("LDAP://mardom-dc-fs03");

                var retProps = new[] { "SamAccountName", "mail", "DisplayName", "description", "company", "CN" };

                var search = await Task.Run(() =>
                    new DirectorySearcher { Filter = string.Format("(mail={0})", username) }
                    ).ConfigureAwait(false);

                search.PropertiesToLoad.AddRange(retProps);

                var result = search.FindOne();

                return new UserAzureAD
                {
                    Usua = result!.Properties!.Contains("SamAccountName") ? (string)result.Properties["SamAccountName"][0] : string.Empty,
                    DisNam = result.Properties.Contains("DisplayName") ? (string)result.Properties["DisplayName"][0] ?? "" : string.Empty,
                    Mail = result.Properties.Contains("mail") ? (string)result.Properties["mail"][0] ?? "" : string.Empty,
                    Cod = result.Properties.Contains("description") ? (string)result.Properties["description"][0] : string.Empty
                };
            }
            catch (Exception ex)
            {
                throw new ActiveDirectoryCOMExceptions(ex);
            }
        }
    }
}
