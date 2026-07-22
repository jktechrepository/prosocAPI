using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class UserDeviceService : IUserDeviceRepository
    {
        private readonly ProsocDbContext _context;

        public UserDeviceService(ProsocDbContext context)
        {
            _context = context;
        }

        public async Task<UserDevice?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.UserDevices
                .Include(ud => ud.Utilisateur)
                .FirstOrDefaultAsync(ud => ud.IdUserDevice == id, ct);
        }

        public async Task<List<UserDevice>> GetByUserIdAsync(int userId, CancellationToken ct = default)
        {
            return await _context.UserDevices
                .Where(ud => ud.UtilisateurId == userId && ud.Statut == true)
                .OrderByDescending(ud => ud.DateDerniereUtilisation)
                .ToListAsync(ct);
        }

        public async Task<UserDevice?> GetByFcmTokenAsync(string fcmToken, CancellationToken ct = default)
        {
            return await _context.UserDevices
                .Include(ud => ud.Utilisateur)
                .FirstOrDefaultAsync(ud => ud.FcmToken == fcmToken, ct);
        }

        public async Task<UserDevice> CreateAsync(UserDevice device, CancellationToken ct = default)
        {
            device.DateEnregistrement = DateTime.Now;
            device.DateDerniereUtilisation = DateTime.Now;
            device.Statut = true;

            _context.UserDevices.Add(device);
            await _context.SaveChangesAsync(ct);
            return device;
        }

        public async Task<UserDevice> UpdateAsync(UserDevice device, CancellationToken ct = default)
        {
            device.DateDerniereUtilisation = DateTime.Now;
            _context.UserDevices.Update(device);
            await _context.SaveChangesAsync(ct);
            return device;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var device = await _context.UserDevices.FindAsync(new object[] { id }, ct);
            if (device == null) return false;

            device.Statut = false;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> UpdateLastUsageAsync(int id, CancellationToken ct = default)
        {
            var device = await _context.UserDevices.FindAsync(new object[] { id }, ct);
            if (device == null) return false;

            device.DateDerniereUtilisation = DateTime.Now;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        // Méthode utilitaire pour gérer l'enregistrement/mise à jour du device lors du login
        public async Task<UserDevice> RegisterOrUpdateDeviceAsync(int userId, string fcmToken, 
            string? deviceType = null, string? deviceModel = null, string? osVersion = null, 
            CancellationToken ct = default)
        {
            // Vérifier si le device existe déjà avec ce FCM token
            var existingDevice = await GetByFcmTokenAsync(fcmToken, ct);
            
            if (existingDevice != null)
            {
                // Mettre à jour le device existant
                existingDevice.UtilisateurId = userId;
                existingDevice.DeviceType = deviceType;
                existingDevice.DeviceModel = deviceModel;
                existingDevice.OsVersion = osVersion;
                existingDevice.DateDerniereUtilisation = DateTime.Now;
                existingDevice.Statut = true;
                
                return await UpdateAsync(existingDevice, ct);
            }
            else
            {
                // Créer un nouveau device
                var newDevice = new UserDevice
                {
                    UtilisateurId = userId,
                    FcmToken = fcmToken,
                    DeviceType = deviceType,
                    DeviceModel = deviceModel,
                    OsVersion = osVersion,
                    DefaultDevice = "Oui", // Premier device enregistré devient par défaut
                    DateEnregistrement = DateTime.Now,
                    DateDerniereUtilisation = DateTime.Now,
                    Statut = true
                };

                return await CreateAsync(newDevice, ct);
            }
        }
    }
}
