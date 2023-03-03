using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.PS.DataAccess
{
    public class Util
    {
        public static string GetInnerExcepcion(Exception exc, string message)
        {
            message = string.Format("({0})) : {1}.  ", exc.GetType().FullName, message);

            if (exc.InnerException != null)
            {
                return message += GetInnerExcepcion(exc.InnerException, exc.InnerException.Message);
            }
            else
            {
                return string.Format("({0}) : {1}", exc.GetType().FullName, exc.Message);
            }

        }
    }
}
