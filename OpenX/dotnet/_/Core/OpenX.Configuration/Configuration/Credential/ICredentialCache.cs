using System;
using System.Net;

namespace OpenX.Configuration
{
    public interface ICredentialCache
    {
        void Add(Uri uri, ICredentials credentials);
        ICredentials GetCredentials(Uri uri);
    }
}
