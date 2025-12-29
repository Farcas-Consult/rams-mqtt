using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using ZebraIoTConnector.Persistence.Entities;

namespace ZebraIoTConnector.Persistence.Repositories
{
    public class GateRepository : IGateRepository
    {
        private readonly ZebraDbContext zebraDbContext;

        public GateRepository(ZebraDbContext zebraDbContext)
        {
            this.zebraDbContext = zebraDbContext ?? throw new ArgumentNullException(nameof(zebraDbContext));
        }

        public Gate? GetById(int id)
        {
            return zebraDbContext.Gates
                .Include(g => g.Location)
                .Include(g => g.Readers)
                .FirstOrDefault(g => g.Id == id);
        }

        public Gate? GetByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return zebraDbContext.Gates
                .Include(g => g.Location)
                .Include(g => g.Readers)
                .FirstOrDefault(g => g.Name == name);
        }

        public Gate? GetByReaderName(string readerName)
        {
            if (string.IsNullOrEmpty(readerName))
                return null;

            return zebraDbContext.Gates
                .Include(g => g.Location)
                .Include(g => g.Readers)
                .FirstOrDefault(g => g.Readers.Any(r => r.Name == readerName));
        }

        public List<Gate> GetAll()
        {
            return zebraDbContext.Gates
                .Include(g => g.Location)
                .Include(g => g.Readers)
                .ToList();
        }

        public void Create(Gate gate)
        {
            if (gate == null)
                throw new ArgumentNullException(nameof(gate));

            zebraDbContext.Gates.Add(gate);
            zebraDbContext.SaveChanges();
        }

        public void Update(Gate gate)
        {
            if (gate == null)
                throw new ArgumentNullException(nameof(gate));

            zebraDbContext.Gates.Update(gate);
            zebraDbContext.SaveChanges();
        }

        public void Delete(int id)
        {
            var gate = GetById(id);
            if (gate != null)
            {
                zebraDbContext.Gates.Remove(gate);
                zebraDbContext.SaveChanges();
            }
        }
    }
}



