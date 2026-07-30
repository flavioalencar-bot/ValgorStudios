using System;
using System.Collections.Generic;
using System.Text;
using Valgor.Dragons.Data;

namespace Valgor.Dragons.Combat
{
    /// <summary>Configuração de habilidades (até 3 slots) e listagem por nível.</summary>
    public sealed class DragonAbilityService
    {
        public const int SlotCount = 3;

        public IReadOnlyList<DragonAbilityDefinition> GetUnlocked(int dragonLevel)
        {
            var list = new List<DragonAbilityDefinition>();
            foreach (var pair in DragonAbilityCatalog.All)
            {
                if (dragonLevel >= pair.Value.UnlockLevel)
                {
                    list.Add(pair.Value);
                }
            }

            list.Sort((a, b) => a.UnlockLevel.CompareTo(b.UnlockLevel));
            return list;
        }

        public void EnsureDefaults(DragonInstance dragon)
        {
            if (dragon.DragonLevel < 1)
            {
                return;
            }

            if (dragon.AbilitySlot0 == DragonAbilityId.None)
            {
                dragon.AbilitySlot0 = DragonAbilityId.EmberBreath;
            }

            ClampSlotsToUnlock(dragon);
        }

        public void ClampSlotsToUnlock(DragonInstance dragon)
        {
            dragon.AbilitySlot0 = ClampOrNone(dragon.AbilitySlot0, dragon.DragonLevel);
            dragon.AbilitySlot1 = ClampOrNone(dragon.AbilitySlot1, dragon.DragonLevel);
            dragon.AbilitySlot2 = ClampOrNone(dragon.AbilitySlot2, dragon.DragonLevel);
        }

        public bool TrySetSlot(
            DragonInstance dragon,
            DragonAbilitySlot slot,
            DragonAbilityId abilityId,
            out string error)
        {
            if (dragon.DragonLevel < 1)
            {
                error = "Dragão ainda não nasceu.";
                return false;
            }

            if (dragon.IsLevelingUp)
            {
                error = "Aguarde a evolução/ritual.";
                return false;
            }

            if (dragon.State is DragonState.Deployed or DragonState.Recovering or DragonState.Injured
                or DragonState.Exhausted)
            {
                error = "Configure habilidades com o dragão no ninho (não em missão/recuperação).";
                return false;
            }

            if (abilityId == DragonAbilityId.None)
            {
                SetSlot(dragon, slot, DragonAbilityId.None);
                error = string.Empty;
                return true;
            }

            if (!DragonAbilityCatalog.TryGet(abilityId, out var def))
            {
                error = "Habilidade inválida.";
                return false;
            }

            if (dragon.DragonLevel < def.UnlockLevel)
            {
                error = $"Desbloqueia no Nv.{def.UnlockLevel} ({def.DisplayName}).";
                return false;
            }

            // Evita duplicar a mesma habilidade em dois slots.
            ClearDuplicates(dragon, abilityId, slot);
            SetSlot(dragon, slot, abilityId);
            error = string.Empty;
            return true;
        }

        public string DescribeLoadout(DragonInstance dragon)
        {
            var sb = new StringBuilder();
            AppendSlot(sb, "1", dragon.AbilitySlot0);
            AppendSlot(sb, "2", dragon.AbilitySlot1);
            AppendSlot(sb, "3", dragon.AbilitySlot2);
            return sb.Length == 0 ? "Nenhuma habilidade equipada." : sb.ToString().TrimEnd(' ', '·');
        }

        public IEnumerable<DragonAbilityId> Equipped(DragonInstance dragon)
        {
            if (dragon.AbilitySlot0 != DragonAbilityId.None)
            {
                yield return dragon.AbilitySlot0;
            }

            if (dragon.AbilitySlot1 != DragonAbilityId.None)
            {
                yield return dragon.AbilitySlot1;
            }

            if (dragon.AbilitySlot2 != DragonAbilityId.None)
            {
                yield return dragon.AbilitySlot2;
            }
        }

        private static DragonAbilityId ClampOrNone(DragonAbilityId id, int level)
        {
            if (id == DragonAbilityId.None || !DragonAbilityCatalog.TryGet(id, out var def))
            {
                return DragonAbilityId.None;
            }

            return level >= def.UnlockLevel ? id : DragonAbilityId.None;
        }

        private static void SetSlot(DragonInstance dragon, DragonAbilitySlot slot, DragonAbilityId id)
        {
            switch (slot)
            {
                case DragonAbilitySlot.Primary:
                    dragon.AbilitySlot0 = id;
                    break;
                case DragonAbilitySlot.Secondary:
                    dragon.AbilitySlot1 = id;
                    break;
                default:
                    dragon.AbilitySlot2 = id;
                    break;
            }
        }

        private static void ClearDuplicates(DragonInstance dragon, DragonAbilityId id, DragonAbilitySlot keep)
        {
            if (keep != DragonAbilitySlot.Primary && dragon.AbilitySlot0 == id)
            {
                dragon.AbilitySlot0 = DragonAbilityId.None;
            }

            if (keep != DragonAbilitySlot.Secondary && dragon.AbilitySlot1 == id)
            {
                dragon.AbilitySlot1 = DragonAbilityId.None;
            }

            if (keep != DragonAbilitySlot.Tertiary && dragon.AbilitySlot2 == id)
            {
                dragon.AbilitySlot2 = DragonAbilityId.None;
            }
        }

        private static void AppendSlot(StringBuilder sb, string label, DragonAbilityId id)
        {
            if (id == DragonAbilityId.None || !DragonAbilityCatalog.TryGet(id, out var def))
            {
                return;
            }

            if (sb.Length > 0)
            {
                sb.Append(" · ");
            }

            sb.Append(label).Append(':').Append(def.DisplayName);
        }
    }
}
