using AutoMapper;
using BrandService.DTOs;
using BrandService.Entities;
using BrandService.Infrastructure.Repositories;

namespace BrandService.Infrastructure.Services
{
    public class DealerService
    {
        private readonly DealerRepository _repo;
        private readonly IMapper _mapper;

        public DealerService(DealerRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<DealerDTO.DealerResponse>> GetAllAsync()
        {
            try
            {
                var dealers = await _repo.GetAllAsync();
                return _mapper.Map<List<DealerDTO.DealerResponse>>(dealers);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get all dealers: {ex.Message}", ex);
            }
        }

        public async Task<DealerDTO.DealerResponse?> GetByIdAsync(Guid id)
        {
            try
            {
                var dealer = await _repo.GetByIdAsync(id);
                return dealer == null ? null : _mapper.Map<DealerDTO.DealerResponse>(dealer);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get dealer by Id: {ex.Message}", ex);
            }
        }

        public async Task<DealerDTO.DealerResponse> CreateAsync(DealerDTO.DealerRequest dealerRequest)
        {
            var dealer = _mapper.Map<Dealer>(dealerRequest);
            dealer.DealerId = Guid.NewGuid();
            try
            {
                var created = await _repo.AddAsync(dealer);
                return _mapper.Map<DealerDTO.DealerResponse>(created);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create dealer: {ex.Message}", ex);
            }
        }

        public async Task<DealerDTO.DealerResponse?> UpdateAsync(Guid id, DealerDTO.DealerRequest dealerRequest)
        {
            try
            {
                var dealer = await _repo.GetByIdAsync(id);
                if (dealer == null) return null;
                dealer = _mapper.Map(dealerRequest, dealer);
                var updated = await _repo.UpdateAsync(dealer);
                return _mapper.Map<DealerDTO.DealerResponse>(updated);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update dealer: {ex.Message}", ex);
            }
        }

        //public async Task<List<DealerTargetDto>> GetTargetsAsync(Guid dealerId)
        //{
        //    var targets = await _repo.GetTargetsAsync(dealerId);
        //    return _mapper.Map<List<DealerTargetDto>>(targets);
        //}
    }
}
