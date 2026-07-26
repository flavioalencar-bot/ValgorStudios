using FluentValidation;
using MediatR;
using Valgor.Application.Common.Interfaces.Heroes;
using Valgor.Application.Common.Results;
using Valgor.Contracts.Heroes;
using Valgor.Domain.Heroes.Services;

namespace Valgor.Application.Heroes.ValidateTeam;

public sealed record ValidateTeamCommand(IReadOnlyList<string> HeroIds) : IRequest<Result<ValidateTeamResponse>>;

public sealed class ValidateTeamCommandValidator : AbstractValidator<ValidateTeamCommand>
{
    public ValidateTeamCommandValidator()
    {
        RuleFor(x => x.HeroIds).NotNull().Must(ids => ids.Count is >= 1 and <= 5)
            .WithMessage("A equipe deve ter entre 1 e 5 heróis.");
    }
}

public sealed class ValidateTeamCommandHandler(IHeroCatalogStore catalogStore)
    : IRequestHandler<ValidateTeamCommand, Result<ValidateTeamResponse>>
{
    public async Task<Result<ValidateTeamResponse>> Handle(ValidateTeamCommand request, CancellationToken cancellationToken)
    {
        var catalog = await catalogStore.GetAllAsync(cancellationToken);
        var byId = catalog.ToDictionary(h => h.Id, StringComparer.Ordinal);
        var resolved = new List<string>();
        var factions = new List<string>();

        foreach (var heroId in request.HeroIds)
        {
            if (!byId.TryGetValue(heroId, out var hero))
            {
                return Result.Success(new ValidateTeamResponse(
                    false,
                    $"Herói '{heroId}' não existe no catálogo.",
                    1.0m,
                    0,
                    null,
                    request.HeroIds,
                    []));
            }

            resolved.Add(hero.Id);
            factions.Add(hero.FactionId);
        }

        if (resolved.Distinct(StringComparer.Ordinal).Count() != resolved.Count)
        {
            return Result.Success(new ValidateTeamResponse(
                false,
                "Não é permitido repetir o mesmo herói na equipe.",
                1.0m,
                0,
                null,
                request.HeroIds,
                factions));
        }

        var bonusRules = (await catalogStore.GetTeamBonusesAsync(cancellationToken))
            .Select(b => new TeamBonusRule(b.SameFactionCount, b.OtherFactionCount, b.TotalTroopAttackMultiplier))
            .ToArray();

        var calculator = new FactionBonusCalculator(bonusRules);
        var bonus = calculator.Calculate(factions);

        return Result.Success(new ValidateTeamResponse(
            true,
            null,
            bonus.TotalTroopAttackMultiplier,
            bonus.SameFactionCount,
            bonus.DominantFactionId,
            resolved,
            factions));
    }
}
