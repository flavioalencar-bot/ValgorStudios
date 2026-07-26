using FluentValidation;
using MediatR;
using Valgor.Application.Common.Interfaces;
using Valgor.Application.Common.Interfaces.Heroes;
using Valgor.Application.Common.Results;
using Valgor.Contracts.Heroes;
using Valgor.Domain.Heroes.Services;

namespace Valgor.Application.Heroes.ActivateSpecial;

public sealed record ActivateSpecialCommand(
    string BattleId,
    string HeroId,
    Guid PlayerId,
    string IdempotencyKey) : IRequest<Result<ActivateSpecialResponse>>;

public sealed class ActivateSpecialCommandValidator : AbstractValidator<ActivateSpecialCommand>
{
    public ActivateSpecialCommandValidator()
    {
        RuleFor(x => x.BattleId).NotEmpty();
        RuleFor(x => x.HeroId).NotEmpty();
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(128);
    }
}

public sealed class ActivateSpecialCommandHandler(
    IHeroCatalogStore catalogStore,
    IPlayerHeroStore playerHeroStore,
    IBattleSpecialStore battleSpecialStore,
    IDateTimeProvider clock)
    : IRequestHandler<ActivateSpecialCommand, Result<ActivateSpecialResponse>>
{
    private static readonly SpecialPowerStateMachine StateMachine = new();

    public async Task<Result<ActivateSpecialResponse>> Handle(ActivateSpecialCommand request, CancellationToken cancellationToken)
    {
        var hero = await catalogStore.GetByIdAsync(request.HeroId, cancellationToken);
        if (hero is null || hero.SpecialPower is null)
        {
            return Result.Failure<ActivateSpecialResponse>(Error.NotFound($"Herói '{request.HeroId}' inválido."));
        }

        var roster = await playerHeroStore.GetByPlayerAsync(request.PlayerId, cancellationToken);
        if (roster.All(h => !string.Equals(h.HeroId, request.HeroId, StringComparison.Ordinal) || !h.Unlocked))
        {
            return Result.Failure<ActivateSpecialResponse>(
                Error.Unauthorized($"Jogador não possui o herói '{request.HeroId}'."));
        }

        var state = await battleSpecialStore.GetOrCreateAsync(
            request.BattleId,
            request.PlayerId,
            request.HeroId,
            cancellationToken);

        var utcNow = clock.UtcNow;
        var activated = state.TryActivate(
            StateMachine,
            utcNow,
            hero.SpecialPower.ActiveDurationSec,
            hero.SpecialPower.CooldownSec,
            request.IdempotencyKey,
            out var snapshot,
            out var wasDuplicate);

        if (!activated)
        {
            if (!state.IsAlive)
            {
                return Result.Failure<ActivateSpecialResponse>(Error.Validation("Herói morto não pode ativar poder especial."));
            }

            return Result.Failure<ActivateSpecialResponse>(
                Error.Conflict($"Poder especial não está READY (estado atual: {snapshot.State})."));
        }

        await battleSpecialStore.SaveAsync(state, cancellationToken);

        return Result.Success(new ActivateSpecialResponse(
            request.BattleId,
            request.HeroId,
            snapshot.State.ToString().ToUpperInvariant(),
            snapshot.ActiveUntilUtc,
            snapshot.CooldownUntilUtc,
            wasDuplicate));
    }
}
