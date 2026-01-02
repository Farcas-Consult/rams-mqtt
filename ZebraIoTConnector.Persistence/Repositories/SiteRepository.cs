using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using ZebraIoTConnector.Persistence.Entities;

namespace ZebraIoTConnector.Persistence.Repositories
{
    public interface ISiteRepository
    {
        Site GetById(int id);
        Site GetByName(string name);
        IEnumerable<Site> GetAll();
        void Create(Site site);
        void Update(Site site);
        void Delete(int id);
    }

    public class SiteRepository : ISiteRepository
    {
        private readonly ZebraDbContext context;

        public SiteRepository(ZebraDbContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Site GetByName(string name)
        {
            return context.Sites.FirstOrDefault(s => s.Name == name);
        }

        public Site GetById(int id)
        {
            return context.Sites
                .Include(s => s.Gates)
                .FirstOrDefault(s => s.Id == id);
        }

        public IEnumerable<Site> GetAll()
        {
            return context.Sites
                .Include(s => s.Gates)
                .ToList();
        }

        public void Create(Site site)
        {
            if (site == null) throw new ArgumentNullException(nameof(site));
            context.Sites.Add(site);
            context.SaveChanges();
        }

        public void Update(Site site)
        {
            if (site == null) throw new ArgumentNullException(nameof(site));
            context.Sites.Update(site);
            context.SaveChanges();
        }

        public void Delete(int id)
        {
            var site = GetById(id);
            if (site != null)
            {
                context.Sites.Remove(site);
                context.SaveChanges();
            }
        }
    }
}
