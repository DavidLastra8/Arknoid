using Arknoid.Core.Models;

namespace Arknoid.Core.Services
{
    public interface IRunRepository
    {
        bool TryLoad(out RunData run);
        void Save(RunData run);
        void Clear();
    }
}
