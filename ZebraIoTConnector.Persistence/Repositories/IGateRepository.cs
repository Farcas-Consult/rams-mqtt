using System.Collections.Generic;
using ZebraIoTConnector.Persistence.Entities;

namespace ZebraIoTConnector.Persistence.Repositories
{
    public interface IGateRepository
    {
        Gate? GetById(int id);
        Gate? GetByName(string name);
        Gate? GetByReaderName(string readerName);
        List<Gate> GetAll();
        void Create(Gate gate);
        void Update(Gate gate);
        void Delete(int id);
        int GetTotalCount();
        int GetActiveCount();
    }
}



