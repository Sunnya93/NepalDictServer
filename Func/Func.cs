namespace NepalDictServer
{
    public class Func
    {
        public static string ConvertDateString(string DateEightString)
        {
            string rtnString = "";

            if(DateEightString.Length == 8)
            {
                rtnString = DateEightString.Substring(0, 4) + "-" + DateEightString.Substring(4, 2) + "-" + DateEightString.Substring(6, 2);
            }
            else if(DateEightString.Length < 8)
            {
                rtnString = "2021-05-01";
            }

            return rtnString;
        }
    }
}
