using EsportsApp.Application.DTOs.Teams;

namespace EsportsApp.Application.Interfaces;

public interface ITeamService
{
    Task<TeamDetailDto> GetByIdAsync(Guid teamId);
    Task<TeamDetailDto> CreateAsync(Guid captainPlayerId, CreateTeamRequestDto request);
    Task<TeamDetailDto> UpdateAsync(Guid captainPlayerId, Guid teamId, UpdateTeamRequestDto request);
    Task InvitePlayerAsync(Guid captainPlayerId, Guid teamId, InvitePlayerRequestDto request);
    Task RespondToInvitationAsync(Guid playerId, Guid invitationId, bool accept);
    Task RemoveMemberAsync(Guid captainPlayerId, Guid teamId, Guid memberId);
    Task TransferCaptaincyAsync(Guid currentCaptainId, Guid teamId, Guid newCaptainId);
    Task<IEnumerable<InvitationDto>> GetMyInvitationsAsync(Guid playerId);
    Task<TeamDetailDto?> GetMyTeamAsync(Guid playerId);
}
