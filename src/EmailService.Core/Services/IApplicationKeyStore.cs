using System;
using System.Threading.Tasks;

namespace EmailService.Core.Services
{
    public interface IApplicationKeyStore
    {
        Task<Tuple<byte[], byte[]>> GetKeysAsync(Guid applicationId);
    }
}
