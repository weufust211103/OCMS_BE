﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OCMS_BOs;
using OCMS_BOs.Entities;
using OCMS_Repositories.IRepository;
namespace OCMS_Repositories.Repository
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly OCMSDbContext _context;

        public UserRepository(OCMSDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                                 .Include(u => u.Role) // Ensure Role is included in the query
                                 .Where(u => u.Username == username)
                                 .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
        {
            return await _context.Users
                .Where(u => u.Role.RoleName == roleName) 
                .ToListAsync();
        }
        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Role) 
                .ToListAsync();
        }

        public async Task<User> GetByIdAsync(string id)
        {
            return await _context.Users
                                 .Include(u => u.Role)
                                 .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<IEnumerable<User>> GetUsersByIdsAsync(IEnumerable<string> userIds)
        {
            if (userIds == null || !userIds.Any())
            {
                return Enumerable.Empty<User>();
            }

            return await _context.Users
                                 .Include(u => u.Role)
                                 .Where(u => userIds.Contains(u.UserId))
                                 .ToListAsync();
        }

    }
}
