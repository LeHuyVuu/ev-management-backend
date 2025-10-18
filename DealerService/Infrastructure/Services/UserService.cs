using AutoMapper;
using DealerService.DTOs.Responses.DealerDTOs;
using DealerService.Infrastructure.Repositories;

namespace DealerService.Infrastructure.Services;

public class UserService
{
    private readonly UserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(UserRepository userRepository,  IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DealerStaffsResponse>> GetUserByRoleDealerStaff()
    {
        var dealerStaffs = await _userRepository.GetUserWithRolDealerStaff();
        return _mapper.Map<IEnumerable<DealerStaffsResponse>>(dealerStaffs);
    }
}