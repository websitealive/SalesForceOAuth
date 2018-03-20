using System;
using System.IO;
using System.Security;
using System.Configuration;

namespace SalesForceOAuth.Web_API_Helper_Code
{
    public class Configuration
    {
        #region Properties
        /// <summary>
        /// The root address of the Dynamics CRM service.
        /// </summary>
        /// <example>https://myorg.crm.dynamics.com</example>
        public string ServiceUrl { get; set; }


        /// <summary>
        /// The client ID that was generated when the application was registered in Microsoft Azure
        /// Active Directory or AD FS.
        /// </summary>
        /// <remarks>Required only with a web service configured for OAuth authentication.</remarks>
        public string ClientId { get; set; }
        /// <summary>
        /// The user name of the logged on user or null.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///  The password of the logged on user or null.
        /// </summary>
        public SecureString Password { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructs a configuration object.
        /// </summary>
        public Configuration() { }
        public Configuration(string uname, SecureString pwd, string serviceurl,string clientid)
        {
            Username = uname;
            Password = pwd;
            ServiceUrl = serviceurl;
            ClientId = clientid; 
        }

        #endregion Constructors
    }

}