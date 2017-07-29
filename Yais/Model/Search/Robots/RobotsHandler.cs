using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yais.Model.Search.Robots
{
    public class RobotsHandler
    {
        const string UserAgent = "User-agent: ";
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
            var parts = absolutePath.Split('?');
            bool isPathAllowed = IsPathAllowed(parts[0]);

            if (parts.Length == 1)
                return isPathAllowed;

            return IsPathWithParametersAllowed(isPathAllowed, parts[0], parts[1]);
        }

        private bool IsPathWithParametersAllowed(bool isPathAllowed, string path, string parameters)
        {
            bool isAllowed = isPathAllowed;

            string root = path + "?";
            foreach (var item in GetPossibleParameters(parameters))
            {
                var toCheck = root + item;
                if (_allowed.Contains(toCheck))
                    isAllowed = true;
                else if (_disAllowed.Contains(toCheck))
                    isAllowed = false;
            }
            return isAllowed;
        }

        private List<string> GetPossibleParameters(string parameters)
        {
            List<string> result = new List<string>();
            result.Add("");

            var allParams = parameters.Split('&');
            for (int i = 0; i < allParams.Length; ++i)
            {
                var parameter = allParams[i];
                bool isLast = i == allParams.Length - 1;

                int n = result.Count;
                for (int j = 0; j < n; j++)
                {
                    string prefix = result[j];
                    foreach (var item in GetPossibleValues(parameter, isLast))
                    {
                        if (prefix.Length==0)
                            result.Add(item);
                        else
                            result.Add(prefix + "&" + item);
                    }
                }

            }
            return result;
        }

        private IEnumerable<string> GetPossibleValues(string parameter, bool isLast)
        {
           // yield return "";
            yield return "*";

            var paramParts = parameter.Split('=');
            var name = paramParts[0];
             
            yield return $"{name}=";
            yield return $"{name}=*";
            yield return $"{parameter}";

            if (isLast)
            {
                yield return $"{name}=*$";
                yield return $"{parameter}$";
            }
        }



        private bool IsPathAllowed(string absolutePath)
        {
            var parts = absolutePath.TrimStart('/').Split('/');

            bool isAllowed = true; // Assume root is allowed
            string toCheck = "";

            for (int i = 0; i < parts.Length; ++i)
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
