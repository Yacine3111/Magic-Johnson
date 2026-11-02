using DTOs.Requests;
using Magik_Johnson.CustomException;
using Magik_Johnson.Data;
using Magik_Johnson.DTOs.Responses;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Magik_Johnson.Services;

public class AddressService : IAddressService
{
    private readonly ApplicationDbContext _applicationDbContext;

    public AddressService(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public Country? GetCountryByName(string name)
    {
        return _applicationDbContext
            .Countries.Include(p => p.Provinces)
            .FirstOrDefault(c => c.Name.ToLower() == name);
    }

    public async Task AddAsync(string userId, AddressRegistrationRequest request)
    {
        var newAddress = new Address
        {
            DoorNb = request.DoorNb,
            Unit = request.Unit,
            Street = request.Street,
            City = request.City,
            PostalCode = request.PostalCode,
            CountryId = request.CountryId,
            ProvinceId = request.ProvinceId,
            ApplicationUserId = userId,
        };

        _applicationDbContext.Address.Add(newAddress);
        await _applicationDbContext.SaveChangesAsync();
    }

    public async Task<List<AddressResponse>> GetAddressByUserIdAsync(string userId)
    {
        return await _applicationDbContext
            .Address.Where(ad => ad.ApplicationUserId == userId)
            .Include(ad => ad.Province)
            .Include(ad => ad.Country)
            .Select(ad => new AddressResponse
            {
                Id = ad.Id,
                UserId = ad.ApplicationUserId,
                Street = ad.Street,
                DoorNb = ad.DoorNb,
                Unit = ad.Unit,
                City = ad.City,
                PostalCode = ad.PostalCode,
                Province = ad.Province != null ? ad.Province.Name : null,
                Country = ad.Country != null ? ad.Country.Name : null,
                CountryId = ad.CountryId,
                ProvinceId = ad.ProvinceId,
            })
            .ToListAsync();
    }

    public async Task UpdateAsync(Guid addressId, string userId, AddressRegistrationRequest request)
    {
        var address = await _applicationDbContext.Address.FirstOrDefaultAsync(ad =>
            ad.Id == addressId && ad.ApplicationUserId == userId
        );

        if (address == null)
            throw new Exception("Adresse non trouvée");

        address.DoorNb = request.DoorNb;
        address.Unit = request.Unit;
        address.Street = request.Street;
        address.City = request.City;
        address.PostalCode = request.PostalCode;
        address.CountryId = request.CountryId;
        address.ProvinceId = request.ProvinceId;

        await _applicationDbContext.SaveChangesAsync();
    }

    public async Task UpdateForClientAsync(Guid addressId, AddressRegistrationRequest request)
    {
        var address = await _applicationDbContext.Address.FirstOrDefaultAsync(ad =>
            ad.Id == addressId
        );

        if (address == null)
            throw new RecordNotFoundException("Adresse non trouvée");

        address.DoorNb = request.DoorNb;
        address.Unit = request.Unit;
        address.Street = request.Street;
        address.City = request.City;
        address.PostalCode = request.PostalCode;
        address.CountryId = request.CountryId;
        address.ProvinceId = request.ProvinceId;

        await _applicationDbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid addressId, string userId)
    {
        var address = await _applicationDbContext.Address.FirstOrDefaultAsync(ad =>
            ad.Id == addressId && ad.ApplicationUserId == userId
        );

        if (address == null)
            throw new RecordNotFoundException("Adresse non trouvée");

        _applicationDbContext.Address.Remove(address);
        await _applicationDbContext.SaveChangesAsync();
    }
}
