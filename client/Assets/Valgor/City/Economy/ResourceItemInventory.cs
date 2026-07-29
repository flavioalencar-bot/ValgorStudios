using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using Valgor.City.Data;
using Valgor.Core;

namespace Valgor.City.Economy
{
    /// <summary>Acesso compartilhado ao inventário da sessão City.</summary>
    public static class CityResourceItems
    {
        private static ResourceItemInventory? _shared;
        public static ResourceItemInventory Shared => _shared ??= new ResourceItemInventory();
        public static void ResetSharedForTests() => _shared = new ResourceItemInventory();
    }

    /// <summary>
    /// Inventário de itens de recurso (PlayerPrefs). Operações atômicas com anti-duplicata.
    /// </summary>
    public sealed class ResourceItemInventory
    {
        public const string PersistenceKeyNormal = "valgor.city.resource-items.v1";
        public const string PersistenceKeyQa = "valgor.city.resource-items.v1.city-progression-qa";

        private readonly Dictionary<string, int> _quantities = new(StringComparer.Ordinal);
        private readonly HashSet<string> _consumedTokens = new(StringComparer.Ordinal);
        private bool _busy;
        private bool _loaded;

        public event Action? Changed;

        public static string PersistenceKey =>
            CityProgressionQa.IsActive ? PersistenceKeyQa : PersistenceKeyNormal;

        public bool IsBusy => _busy;

        public void EnsureLoaded()
        {
            if (_loaded)
            {
                return;
            }

            Load();
            if (_quantities.Count == 0)
            {
                SeedStarter();
                Save();
            }

            _loaded = true;
        }

        public int GetQuantity(string itemId)
        {
            EnsureLoaded();
            return _quantities.TryGetValue(itemId, out var q) ? q : 0;
        }

        public IReadOnlyList<ResourceItemStack> GetStacksFor(ResourceType resource)
        {
            EnsureLoaded();
            var list = new List<ResourceItemStack>();
            foreach (var def in ResourceItemCatalog.ForResource(resource))
            {
                var qty = GetQuantity(def.ItemId);
                if (qty > 0)
                {
                    list.Add(new ResourceItemStack(def, qty));
                }
            }

            list.Sort((a, b) => a.UsagePriority.CompareTo(b.UsagePriority));
            return list;
        }

        public IReadOnlyList<ResourceItemStack> GetAllOwned()
        {
            EnsureLoaded();
            var list = new List<ResourceItemStack>();
            foreach (var def in ResourceItemCatalog.All)
            {
                var qty = GetQuantity(def.ItemId);
                if (qty > 0)
                {
                    list.Add(new ResourceItemStack(def, qty));
                }
            }

            return list;
        }

        public bool CanAutoRefill(ResourceType resource, long missing)
        {
            if (missing <= 0)
            {
                return false;
            }

            var plan = AutoRefillPlanner.Plan(this, resource, missing);
            return plan.CompletesRequirement;
        }

        /// <summary>Usa um item (1 unidade). Retorna valor creditado ou 0 se falhar.</summary>
        public bool TryUse(
            string itemId,
            ResourceWallet wallet,
            out long credited,
            out string error,
            int quantity = 1)
        {
            credited = 0;
            error = string.Empty;
            if (_busy)
            {
                error = "Operação em andamento.";
                return false;
            }

            if (quantity <= 0)
            {
                error = "Quantidade inválida.";
                return false;
            }

            if (!ResourceItemCatalog.TryGet(itemId, out var def))
            {
                error = "Item desconhecido.";
                return false;
            }

            EnsureLoaded();
            if (GetQuantity(itemId) < quantity)
            {
                error = "Quantidade insuficiente no inventário.";
                return false;
            }

            _busy = true;
            try
            {
                var token = $"{itemId}:{quantity}:{DateTime.UtcNow.Ticks}";
                if (!_consumedTokens.Add(token))
                {
                    error = "Recompensa já processada.";
                    return false;
                }

                _quantities[itemId] = GetQuantity(itemId) - quantity;
                credited = checked(def.Value * quantity);
                wallet.Add(def.ResourceId, credited);
                Save();
                Changed?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Load();
                return false;
            }
            finally
            {
                _busy = false;
            }
        }

        public bool TryApplyAutoRefill(
            AutoRefillPlan plan,
            ResourceWallet wallet,
            out string error)
        {
            error = string.Empty;
            if (plan == null || plan.Lines.Length == 0)
            {
                error = "Nada a consumir.";
                return false;
            }

            if (_busy)
            {
                error = "Operação em andamento.";
                return false;
            }

            _busy = true;
            try
            {
                var token = $"auto:{plan.ResourceId}:{plan.RequiredAmount}:{DateTime.UtcNow.Ticks}";
                if (!_consumedTokens.Add(token))
                {
                    error = "Recompensa já processada.";
                    return false;
                }

                EnsureLoaded();
                foreach (var line in plan.Lines)
                {
                    if (GetQuantity(line.ItemId) < line.Quantity)
                    {
                        error = $"Inventário insuficiente: {line.DisplayName}";
                        Load();
                        return false;
                    }
                }

                long total = 0;
                foreach (var line in plan.Lines)
                {
                    _quantities[line.ItemId] = GetQuantity(line.ItemId) - line.Quantity;
                    total = checked(total + line.TotalValue);
                }

                wallet.Add(plan.ResourceId, total);
                Save();
                Changed?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Load();
                return false;
            }
            finally
            {
                _busy = false;
            }
        }

        public void SeedStarter()
        {
            SetQty("pack-food-small", 8);
            SetQty("pack-wood-small", 8);
            SetQty("pack-stone-small", 6);
            SetQty("pack-iron-small", 6);
            SetQty("pack-gold-small", 6);
            SetQty("box-food-basic", 4);
            SetQty("box-wood-basic", 4);
            SetQty("box-stone-basic", 3);
            SetQty("box-iron-basic", 3);
            SetQty("box-gold-basic", 3);
            SetQty("chest-blue-food", 2);
            SetQty("chest-blue-wood", 2);
            SetQty("chest-blue-stone", 2);
            SetQty("chest-blue-iron", 1);
            SetQty("chest-blue-gold", 1);
            SetQty("chest-purple-food", 1);
            SetQty("chest-purple-wood", 1);
            SetQty("crate-select-food", 1);
            SetQty("crate-select-wood", 1);
            SetQty("pack-essence", 3);
        }

        public void SeedQaControlled()
        {
            _quantities.Clear();
            SeedStarter();
            SetQty("pack-wood-small", 40);
            SetQty("box-wood-basic", 10);
            SetQty("chest-blue-wood", 3);
            Save();
            Changed?.Invoke();
        }

        public void ClearAll()
        {
            _quantities.Clear();
            Save();
            Changed?.Invoke();
        }

        private void SetQty(string itemId, int qty) => _quantities[itemId] = Math.Max(0, qty);

        private void Load()
        {
            _quantities.Clear();
            var raw = PlayerPrefs.GetString(PersistenceKey, string.Empty);
            if (string.IsNullOrEmpty(raw))
            {
                return;
            }

            var parts = raw.Split(';');
            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }

                var kv = part.Split('=');
                if (kv.Length != 2)
                {
                    continue;
                }

                if (int.TryParse(kv[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var qty) &&
                    qty > 0 &&
                    ResourceItemCatalog.TryGet(kv[0], out _))
                {
                    _quantities[kv[0]] = qty;
                }
            }
        }

        private void Save()
        {
            var sb = new StringBuilder();
            foreach (var pair in _quantities)
            {
                if (pair.Value <= 0)
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append(';');
                }

                sb.Append(pair.Key).Append('=').Append(pair.Value.ToString(CultureInfo.InvariantCulture));
            }

            PlayerPrefs.SetString(PersistenceKey, sb.ToString());
            PlayerPrefs.Save();
        }
    }

    /// <summary>Planeja consumo mínimo para completar o faltante (prioriza pacotes menores).</summary>
    public static class AutoRefillPlanner
    {
        public static AutoRefillPlan Plan(ResourceItemInventory inventory, ResourceType resource, long missing)
        {
            var before = 0L; // caller preenche Before/After com wallet
            var plan = new AutoRefillPlan
            {
                ResourceId = resource,
                RequiredAmount = missing,
                BeforeAmount = before
            };

            if (missing <= 0)
            {
                plan.CompletesRequirement = true;
                return plan;
            }

            var stacks = new List<ResourceItemStack>(inventory.GetStacksFor(resource));
            stacks.Sort((a, b) =>
            {
                var p = a.UsagePriority.CompareTo(b.UsagePriority);
                return p != 0 ? p : a.Value.CompareTo(b.Value);
            });

            var remaining = missing;
            var lines = new List<AutoRefillPlanLine>();
            long obtained = 0;

            foreach (var stack in stacks)
            {
                if (remaining <= 0 || stack.Quantity <= 0)
                {
                    continue;
                }

                // Quantidade mínima que completa (ou esgota o stack).
                var need = (int)Math.Min(
                    stack.Quantity,
                    Math.Ceiling(remaining / (double)stack.Value));
                if (need <= 0)
                {
                    continue;
                }

                var total = checked(stack.Value * need);
                lines.Add(new AutoRefillPlanLine(
                    stack.ItemId,
                    stack.Definition.DisplayName,
                    need,
                    stack.Value,
                    total));
                obtained = checked(obtained + total);
                remaining -= total;
            }

            plan.Lines = lines.ToArray();
            plan.TotalObtained = obtained;
            plan.CompletesRequirement = remaining <= 0;
            return plan;
        }
    }
}
