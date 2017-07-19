using System;
using System.IO;
using System.Text;

namespace SalesForceOAuth.Web_API_Helper_Code
{
    public class CreateLogFiles
    {
        private string sLogFormat;
        private string sErrorTime;

        public CreateLogFiles()
        {
            //sLogFormat used to create log files format :
            // dd/mm/yyyy hh:mm:ss AM/PM ==> Log Message
            sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";

            //this variable used to create log filename format "
            //for example filename : ErrorLogYYYYMMDD
            string sYear = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString();
            string sDay = DateTime.Now.Day.ToString();
            sErrorTime = sYear + sMonth + sDay;
        }

        public void ErrorLog(string sErrMsg)
        {
            StreamWriter sw = new StreamWriter(System.Web.Hosting.HostingEnvironment.MapPath("~/LogFile/file.log"), true);
            sw.WriteLine(sLogFormat + sErrMsg + "\r");
            sw.Flush();
            sw.Close();
        }
    }
}