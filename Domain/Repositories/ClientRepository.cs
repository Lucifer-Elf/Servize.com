﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Servize.Domain.Model;
using Servize.Utility;
using Servize.Utility.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Servize.Domain.Repositories
{
    public class ClientRepository : BaseRepository<ServizeDBContext>
    {
        private readonly ServizeDBContext _context;
        public ClientRepository(ServizeDBContext dBContext) : base(dBContext)
        {
            _context = dBContext;
        }

        public async Task<Response<IList<Client>>> GetAllClientList()
        {
            try
            {
                List<Client> clientList = await _context.Client.Include(i => i.ApplicationUser)
                                                                            .AsNoTracking().ToListAsync();
                return new Response<IList<Client>>(clientList, StatusCodes.Status200OK);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                return new Response<IList<Client>>($"Failed to get clientList Error", StatusCodes.Status500InternalServerError);
            }

        }

        public async Task<Response<Client>> GetAllServizeUserById(string Id)
        {
            try
            {
                Client client = await _context.Client
                                                    .Include(i => i.ApplicationUser)
                                                    .AsNoTracking().SingleOrDefaultAsync(c => c.UserId == Id);
                if (client == null)
                    return new Response<Client>("Failed to find Id", StatusCodes.Status404NotFound);
                return new Response<Client>(client, StatusCodes.Status200OK);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                return new Response<Client>($"Failed to get ServiceProvide Error:{e.Message}", StatusCodes.Status500InternalServerError);
            }
        }

    }
}
