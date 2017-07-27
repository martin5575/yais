using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yais.Model.Search.Robots
{
    public class RobotsHandler
    {
        const string UserAgent = "User-Agent: ";
        const string Disallow = "Disallow: ";
        const string Allow = "Allow: ";

        HashSet<string> _allowed = new HashSet<string>();
        HashSet<string> _disAllowed = new HashSet<string>();


        public RobotsHandler(string robotsTxt)
        {
            Init(robotsTxt);
        }

        private void Init(string input)
        {
            var rows = input.Split('\r', '\n');
            bool inSectionForAll = false;
            for (int i = 0; i < rows.Length; i++)
            {
                string row = rows[i];
                if (row.StartsWith(UserAgent))
                    inSectionForAll = row.Substring(UserAgent.Length).Trim() == "*";

                if (!inSectionForAll)
                    continue;

                if (row.StartsWith(Allow))
                    _allowed.Add(row.Substring(Allow.Length).Trim());
                else if (row.StartsWith(Disallow))
                    _disAllowed.Add(row.Substring(Disallow.Length).Trim());
            }
        }

        public bool IsUriAllowed(Uri uri)
        {
            string path = uri.AbsolutePath;
            return IsUriAllowed(path);
        }

        public bool IsUriAllowed(string absolutePath)
        {
            var parts = absolutePath.Split('/');

            string toCheck = "/";
            bool isAllowed = _allowed.Contains(toCheck);

            for (int i=0; i<parts.Length;++i)
            {
                toCheck += "/" + parts[i];
                if (_allowed.Contains(toCheck))
                    isAllowed = true;
                else if (_disAllowed.Contains(toCheck))
                    isAllowed = false;
            }
            return isAllowed;
        }
    }
}
