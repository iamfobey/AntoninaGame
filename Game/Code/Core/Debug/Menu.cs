#if IMGUI
using ImGuiNET;
#endif
using System;
using System.Collections.Generic;
using System.Text;
using Game.Core.AI;
using Game.Core.Logger;
using Game.Core.Logger.Sinks;
using Game.Logic.Components;
using Game.Logic.Player.FSM;
using Game.Utils;
using Godot;
using ZLinq;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace Game.Core.Debug
{
    public partial class Menu : Node
    {
        #region GAME METHODS
        [GameMethod]
        public override void _ReadyGame()
        {
            base._ReadyGame();

            var io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.NoKeyboard
                | ImGuiConfigFlags.DpiEnableScaleFonts;

            var style = ImGui.GetStyle();

            ImGui.StyleColorsDark(ImGui.GetStyle());

            style.WindowPadding = new Vector2(4, 4);
            style.FramePadding = new Vector2(4, 2);
            style.CellPadding = new Vector2(2, 2);
            style.ItemSpacing = new Vector2(4, 3);
            style.ItemInnerSpacing = new Vector2(3, 3);
            style.TouchExtraPadding = new Vector2(0, 0);

            style.IndentSpacing = 15.0f;
            style.ScrollbarSize = 10.0f;
            style.GrabMinSize = 8.0f;

            style.WindowRounding = 2.0f;
            style.ChildRounding = 2.0f;
            style.FrameRounding = 2.0f;
            style.PopupRounding = 2.0f;
            style.ScrollbarRounding = 2.0f;
            style.GrabRounding = 2.0f;
            style.TabRounding = 2.0f;

            style.WindowTitleAlign = new Vector2(0.5f, 0.5f);
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);

            style.DisplaySafeAreaPadding = new Vector2(3, 3);

            style.WindowBorderSize = 1.0f;
            style.FrameBorderSize = 0.0f;
            style.PopupBorderSize = 1.0f;

            _imguiSink = new ImGuiSink();
            Logger.Server.Instance.Sinks.Add(_imguiSink);
        }


        [GameMethod]
        public override void _ProcessGame(double delta)
        {
            base._ProcessGame(delta);

            if (Enabled)
            {
                UpdateLogsFromSink();

                DrawImGui();
            }
        }
        #endregion

        #region STATIC METHODS
        public static string FormatLogEntry(Entry entry)
        {
            var sb = new StringBuilder();

            sb.Append($"[{entry.Time}] ");
            sb.Append($"[{entry.Level.ToString().ToUpper()}] ");
            sb.Append($"[{entry.Category}] ");
            sb.Append(
                $"[{entry.CallerFilePath}::{entry.CallerNamespace}::{entry.CallerClassName}::{entry.CallerMemberName}::{entry.CallerLineNumber}] ");
            sb.Append($"[TID:{entry.ThreadId}] ");
            sb.Append(entry.Message);
            return sb.ToString();
        }
        #endregion

        #region PRIVATE METHODS
        private void UpdateLogsFromSink()
        {
            while (_imguiSink.LogQueue.TryDequeue(out var entry))
            {
                _logEntries.Add(entry);
            }

            if (_logEntries.Count > MaxLogEntries)
            {
                _logEntries.RemoveRange(0, _logEntries.Count - MaxLogEntries);
            }
        }

        private void DrawLogWindow()
        {
            ImGui.SetNextWindowSize(new Vector2(800, 400), ImGuiCond.FirstUseEver);
            if (!ImGui.Begin("Logger"))
            {
                ImGui.End();
                return;
            }

            if (ImGui.Button("Clear"))
            {
                _logEntries.Clear();
            }
            ImGui.SameLine();
            ImGui.Checkbox("Auto-scroll", ref _autoScroll);
            ImGui.NewLine();
            ImGui.Text("Filter by Level:");
            ImGui.SameLine();
            foreach (var key in _levelFilters.Keys.AsValueEnumerable().ToList())
            {
                bool value = _levelFilters[key];
                if (ImGui.Checkbox(key.ToString(), ref value))
                    _levelFilters[key] = value;
                ImGui.SameLine();
            }
            ImGui.NewLine();
            ImGui.Text("Filter by Category:");
            ImGui.SameLine();
            string categoryLabel = _selectedCategoryFilter?.ToString() ?? "<All>";
            if (ImGui.BeginCombo("##LogCategoryCombo", categoryLabel))
            {
                if (ImGui.Selectable("<All>", _selectedCategoryFilter == null))
                    _selectedCategoryFilter = null;

                foreach (ELogCategory category in Enum.GetValues(typeof(ELogCategory)))
                {
                    bool isSelected = _selectedCategoryFilter == category;
                    if (ImGui.Selectable(category.ToString(), isSelected))
                        _selectedCategoryFilter = category;
                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }

            ImGui.Separator();

            ImGui.BeginChild("LogScrollingRegion", new Vector2(0, 0), ImGuiChildFlags.None);

            foreach (var entry in _logEntries)
            {
                if (!_levelFilters.TryGetValue(entry.Level, out bool enabled) || !enabled)
                    continue;

                if (_selectedCategoryFilter != null && entry.Category != _selectedCategoryFilter.Value)
                    continue;

                uint color = GetColorForLogLevel(entry.Level);
                ImGui.PushStyleColor(ImGuiCol.Text, color);
                ImGui.TextWrapped(FormatLogEntry(entry));
                ImGui.PopStyleColor();
            }

            if (_autoScroll && ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
            {
                ImGui.SetScrollHereY(1.0f);
            }

            ImGui.EndChild();
            ImGui.End();
        }

        private uint GetColorForLogLevel(ELogLevel level)
        {
            var colorVec = level switch
            {
                ELogLevel.Debug => new Vector4(0.7f, 0.7f, 0.7f, 1.0f),
                ELogLevel.Info => new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                ELogLevel.Warning => new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                ELogLevel.Error => new Vector4(1.0f, 0.4f, 0.4f, 1.0f),
                ELogLevel.Critical => new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                _ => new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
            };
            return ImGui.ColorConvertFloat4ToU32(colorVec);
        }

        private void DrawImGui()
        {
            #if IMGUI
            ImGui.Begin("Debug Menu");

            DrawPlayerDebug();

            DrawAiDebug();

            ImGui.End();

            DrawLogWindow();
            #endif
        }

        private void DrawPlayerDebug()
        {
            var player = Logic.Player.Character.Instance;

            if (ImGuiExtensions.BeginCollapsingHeader("Player", 5.0f))
            {
                if (ImGuiExtensions.BeginCollapsingHeader("General", 5.0f))
                {
                    ImGui.Checkbox("Can Update Input", ref player.CanUpdateInput);

                    var state = player.FSM.GetCurrentState();

                    ImGui.Text(state != null ? $"Current FSM State: {state.GetType().Name}" : "Current FSM State: None");
                    ImGui.Text($"Combo step: {player.FSM.GetState<MopAttackState>().ComboStep}");

                    if (player.Teleports is { Count: > 0 })
                    {
                        if (ImGui.TreeNode("Teleports"))
                        {
                            foreach (var teleportEntry in player.Teleports)
                            {
                                if (ImGui.Button(teleportEntry.Key))
                                {
                                    var targetNode = player.GetNodeOrNull(teleportEntry.Value);
                                    if (targetNode is Node2D target2D)
                                        player.GlobalPosition = target2D.GlobalPosition;
                                }
                            }
                            ImGui.TreePop();
                        }
                    }
                }

                if (ImGuiExtensions.BeginCollapsingHeader("Parameters", 5.0f))
                {
                    ImGui.SeparatorText("CleanState");
                    ImGui.DragFloat("Clean Heal Value", ref player.CleanHealValue, 0.1f, 0f, 100f);

                    ImGui.SeparatorText("DashState");
                    ImGui.DragFloat("Dash Speed", ref player.DashSpeed, 10f, 0f, 5000f);
                    ImGui.DragFloat("Dash Time Scale", ref player.DashTimeScale, 0.01f, 0f, 5f);

                    ImGui.SeparatorText("BlockState");
                    ImGui.DragFloat("Block Time Scale", ref player.BlockTimeScale, 0.01f, 0f, 5f);

                    ImGui.SeparatorText("WalkState");
                    ImGui.DragFloat("Walk Speed", ref player.WalkSpeed, 10f, 0f, 2000f);

                    ImGui.SeparatorText("RunState");
                    ImGui.DragFloat("Run Speed", ref player.RunSpeed, 10f, 0f, 2000f);

                    ImGuiExtensions.EndCollapsingHeader();
                }


                if (ImGuiExtensions.BeginCollapsingHeader("Components", 5.0f))
                {
                    if (ImGui.TreeNode("Health Component"))
                    {
                        ImGui.Text($"Current Health: {player.HealthComponent.CurrentHealth:F1} / {player.HealthComponent.MaxHealth:F1}");
                        ImGui.ProgressBar(player.HealthComponent.CurrentHealth / player.HealthComponent.MaxHealth,
                            new Vector2(-1, 0),
                            $"{player.HealthComponent.CurrentHealth:F1}/{player.HealthComponent.MaxHealth:F1}");

                        ImGui.DragFloat("Max Health", ref player.HealthComponent.MaxHealth, 1.0f, 1f, 1000f);
                        ImGui.DragFloat("Health Regen Rate", ref player.HealthComponent.HealthRegenerationRate, 0.1f, 0f, 50f);
                        ImGui.Checkbox("Can Receive Damage", ref player.HealthComponent.CanReceiveDamage);
                        ImGui.Checkbox("Can Receive Heal", ref player.HealthComponent.CanReceiveHeal);
                        ImGui.Checkbox("Can Regenerate Health", ref player.HealthComponent.CanRegenerateHealth);

                        ImGui.Separator();
                        ImGui.DragFloat("Damage to Apply", ref _tempDamage, 1f, 0f, 100f);
                        if (ImGui.Button("Apply Damage"))
                        {
                            player.HealthComponent.ApplyDamage(player, _tempDamage);
                        }
                        ImGui.SameLine();
                        ImGui.DragFloat("Heal to Apply", ref _tempHeal, 1f, 0f, 100f);
                        if (ImGui.Button("Apply Heal"))
                        {
                            player.HealthComponent.ApplyHeal(player, _tempHeal);
                        }
                        if (ImGui.Button("Full Heal"))
                        {
                            player.HealthComponent.ApplyHeal(player, player.HealthComponent.MaxHealth);
                        }
                        ImGui.TreePop();
                    }

                    if (ImGui.TreeNode("Stamina Component"))
                    {
                        ImGui.Text(
                            $"Current Stamina: {player.StaminaComponent.CurrentStamina:F1} / {player.StaminaComponent.MaxStamina:F1}");
                        ImGui.ProgressBar(player.StaminaComponent.CurrentStamina / player.StaminaComponent.MaxStamina,
                            new Vector2(-1, 0),
                            $"{player.StaminaComponent.CurrentStamina:F1}/{player.StaminaComponent.MaxStamina:F1}");

                        ImGui.DragFloat("Max Stamina", ref player.StaminaComponent.MaxStamina, 1.0f, 1f, 1000f);
                        ImGui.DragFloat("Stamina Regen Rate", ref player.StaminaComponent.StaminaRegenerationRate, 0.1f, 0f, 50f);
                        ImGui.DragFloat("Stamina Regen Multiplier", ref player.StaminaComponent.StaminaRegenerationRateMultiplayer, 0.001f,
                            1f, 2f);
                        ImGui.Checkbox("Can Regenerate Stamina", ref player.StaminaComponent.CanRegenerateStamina);

                        if (ImGui.Button("Full Stamina"))
                        {
                            player.StaminaComponent.RegenerateStamina(player, player.StaminaComponent.MaxStamina);
                        }

                        if (ImGui.Button("Use 30 Stamina"))
                        {
                            player.StaminaComponent.UseStamina(player, 30f);
                        }
                        ImGui.TreePop();
                    }

                    if (player.MoveComponent != null && ImGui.TreeNode("Move Component (Read-Only)"))
                    {
                        var moveDir =
                            new Vector2(player.MoveComponent.Direction.X, player.MoveComponent.Direction.Y);
                        var moveVel =
                            new Vector2(player.MoveComponent.Velocity.X, player.MoveComponent.Velocity.Y);

                        ImGui.InputFloat2("Direction", ref moveDir, "%.2f", ImGuiInputTextFlags.ReadOnly);
                        ImGui.InputFloat2("Velocity", ref moveVel, "%.2f", ImGuiInputTextFlags.ReadOnly);
                        ImGui.TreePop();
                    }

                    if (player.PrevMoveComponent != null && ImGui.TreeNode("Prev Move Component (Read-Only)"))
                    {
                        var moveDir =
                            new Vector2(player.PrevMoveComponent.Direction.X, player.PrevMoveComponent.Direction.Y);
                        var moveVel =
                            new Vector2(player.PrevMoveComponent.Velocity.X, player.PrevMoveComponent.Velocity.Y);

                        ImGui.InputFloat2("Direction", ref moveDir, "%.2f", ImGuiInputTextFlags.ReadOnly);
                        ImGui.InputFloat2("Velocity", ref moveVel, "%.2f", ImGuiInputTextFlags.ReadOnly);
                        ImGui.TreePop();
                    }

                    ImGuiExtensions.EndCollapsingHeader();
                }

                ImGuiExtensions.EndCollapsingHeader();
            }
        }

        private void DrawAiDebug()
        {
            if (ImGuiExtensions.BeginCollapsingHeader("AI", 5.0f))
            {
                var aiList = Navigation.Server.Instance.AIList;
                if (aiList == null || aiList.Count == 0)
                {
                    ImGui.Text("No AI Characters found.");
                    return;
                }

                foreach (var ai in aiList)
                {
                    if (!IsInstanceValid(ai)) continue;

                    ImGui.PushID(ai.GetInstanceId().ToString());

                    if (ImGui.TreeNode($"{ai.Name} ({ai.GetType().Name})"))
                    {
                        DrawAiCharacterDebug(ai);
                        ImGui.TreePop();
                    }

                    ImGui.PopID();
                }

                ImGuiExtensions.EndCollapsingHeader();
            }
        }

        private void DrawAiCharacterDebug(Character ai)
        {
            if (ImGui.CollapsingHeader("General"))
            {
                ImGui.Text($"Current FSM State: {ai.FSM.GetCurrentState()?.GetType().Name ?? "None"}");
                ImGui.Checkbox("Can Move And Slide", ref ai.CanMoveAndSlide);
                if (ImGui.Checkbox("NavAgent Debug Enabled", ref ai.IsNavAgentDebugEnabled))
                    ai.NavigationAgent.DebugEnabled = ai.IsNavAgentDebugEnabled;

                ImGui.Text($"NavAgent Target Position: {ai.NavigationAgent.TargetPosition}");
                ImGui.Text($"NavAgent Velocity: {ai.NavigationAgent.Velocity}");
                ImGui.Text($"Safe Velocity: {ai.SafeVelocity}");
            }

            if (ImGui.CollapsingHeader("Components"))
            {
                if (ImGui.TreeNode("Health Component"))
                {
                    ImGui.ProgressBar(ai.HealthComponent.CurrentHealth / ai.HealthComponent.MaxHealth, new Vector2(-1, 0),
                        $"{ai.HealthComponent.CurrentHealth:F1}/{ai.HealthComponent.MaxHealth:F1}");
                    ImGui.DragFloat("Max Health", ref ai.HealthComponent.MaxHealth, 1.0f, 1f, 1000f);
                    ImGui.Checkbox("Can Receive Damage", ref ai.HealthComponent.CanReceiveDamage);
                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Stamina Component"))
                {
                    ImGui.ProgressBar(ai.StaminaComponent.CurrentStamina / ai.StaminaComponent.MaxStamina, new Vector2(-1, 0),
                        $"{ai.StaminaComponent.CurrentStamina:F1}/{ai.StaminaComponent.MaxStamina:F1}");
                    ImGui.DragFloat("Max Stamina", ref ai.StaminaComponent.MaxStamina, 1.0f, 1f, 1000f);
                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Move Component (Read-Only)"))
                {
                    var moveDir =
                        new Vector2(ai.MoveComponent.Direction.X, ai.MoveComponent.Direction.Y);
                    var moveVel =
                        new Vector2(ai.MoveComponent.Velocity.X, ai.MoveComponent.Velocity.Y);

                    ImGui.InputFloat2("Direction", ref moveDir, "%.2f", ImGuiInputTextFlags.ReadOnly);
                    ImGui.InputFloat2("Velocity", ref moveVel, "%.2f", ImGuiInputTextFlags.ReadOnly);
                    ImGui.TreePop();
                }

                if (ai is AI.Attacker.Character attackerCharacter) DrawPoisonedComponentDebug(attackerCharacter.PoisonedComponent);
            }

            if (ImGui.CollapsingHeader("AI Specifics & FSM"))
            {
                if (ai is Logic.AI.Attacker.Aimer.Character aimer) DrawAttackerDebug(aimer);
                else if (ai is Logic.AI.Attacker.Arc.Character arc) DrawAttackerDebug(arc);
                else if (ai is Logic.AI.Attacker.Simple.Character simple) DrawAttackerDebug(simple);
                else if (ai is Logic.AI.Attacker.Sniper.Character sniper) DrawAttackerDebug(sniper);
                else if (ai is Logic.AI.Attacker.Straight.Character straight) DrawAttackerDebug(straight);
                else if (ai is Logic.AI.Attacker.Tank.Character tank) DrawAttackerDebug(tank);
            }
        }

        private void DrawPoisonedComponentDebug(PoisonedComponent component)
        {
            if (ImGui.TreeNode("Poisoned Component"))
            {
                ImGui.Checkbox("Is Poisoned", ref component.IsPoisoned);
                ImGui.Text($"Hit Count: {component.HitCount}");
                ImGui.TreePop();
            }
        }

        private void DrawAttackerDebug(Logic.AI.Attacker.Aimer.Character ai)
        {
            ImGui.Checkbox("Can Attack", ref ai.CanAttack);
            ImGui.DragInt("Hit Count To Remove Poison", ref ai.HitCountToPoison, 1, 1, 10);
            ImGui.SeparatorText("FSM Parameters");
            ImGui.DragFloat("Run Speed", ref ai.RunSpeed, 10f, 0f, 2000f);
            ImGui.DragFloat("Walk Speed", ref ai.WalkSpeed, 10f, 0f, 1000f);
            ImGui.DragFloat("Attack Damage", ref ai.AttackDamageValue, 0.1f, 0f, 100f);
            ImGui.DragFloat("Aim Speed", ref ai.AimSpeed, 1f, 1f, 500f);
        }

        private void DrawAttackerDebug(Logic.AI.Attacker.Arc.Character ai)
        {
            ImGui.Checkbox("Can Change Nav Position", ref ai.CanChangeNavigationPosition);

            ImGui.SeparatorText("Attack Logic");
            ImGui.DragInt("Max Attacks to Player", ref ai.MaxAttackToPlayerCount, 1, 0, 10);
            ImGui.InputInt("Current Attacks to Player", ref ai.CurrentAttackToPlayerCount, 0, 0, ImGuiInputTextFlags.ReadOnly);
            ImGui.DragInt("Max Position Changes", ref ai.MaxChangePositionCount, 1, 0, 10);
            ImGui.InputInt("Current Position Changes", ref ai.CurrentChangePositionCount, 0, 0, ImGuiInputTextFlags.ReadOnly);
            ImGui.SeparatorText("FSM Parameters");
            ImGui.DragFloat("Run Speed", ref ai.RunSpeed, 10f, 0f, 1000f);
            ImGui.DragFloat("Walk Speed", ref ai.WalkSpeed, 10f, 0f, 1000f);
            ImGui.DragFloat("Attack Damage", ref ai.AttackDamageValue, 0.1f, 0f, 100f);
        }

        private void DrawAttackerDebug(Logic.AI.Attacker.Simple.Character ai)
        {
            ImGui.SeparatorText("FSM Parameters");
            ImGui.DragFloat("Run Speed", ref ai.RunSpeed, 10f, 0f, 1000f);
            ImGui.DragFloat("Walk Speed", ref ai.WalkSpeed, 10f, 0f, 1000f);
            ImGui.DragFloat("Attack Damage", ref ai.AttackDamageValue, 0.1f, 0f, 100f);
            ImGui.Checkbox("Can Spawn Dirt On Run", ref ai.CanSpawnDirtOnRun);
        }

        private void DrawAttackerDebug(Logic.AI.Attacker.Sniper.Character ai)
        {
            ImGui.Checkbox("Can Change Nav Position", ref ai.CanChangeNavigationPosition);

            ImGui.SeparatorText("FSM Parameters");
            ImGui.DragFloat("Run Speed", ref ai.RunSpeed, 10f, 0f, 1000f);
            ImGui.DragFloat("Walk Speed", ref ai.WalkSpeed, 10f, 0f, 1000f);
            ImGui.DragFloat("Attack Damage", ref ai.AttackDamageValue, 0.1f, 0f, 100f);
            ImGui.Checkbox("Projectile Destroy on AI", ref ai.ShouldProjectileDestroyOnAI);
        }

        private void DrawAttackerDebug(Logic.AI.Attacker.Straight.Character ai)
        {
            ImGui.Checkbox("Can Change Nav Position", ref ai.CanChangeNavigationPosition);

            ImGui.SeparatorText("FSM Parameters");
            ImGui.DragFloat("Run Speed", ref ai.RunSpeed, 10f, 0f, 1000f);
            ImGui.DragFloat("Walk Speed", ref ai.WalkSpeed, 10f, 0f, 1000f);
            ImGui.DragFloat("Attack Damage", ref ai.AttackDamageValue, 0.1f, 0f, 100f);
            ImGui.Checkbox("Projectile Destroy on AI", ref ai.ShouldProjectileDestroyOnAI);
        }

        private void DrawAttackerDebug(Logic.AI.Attacker.Tank.Character ai)
        {
            ImGui.SeparatorText("Attack Logic");
            ImGui.DragInt("Max Attack Count", ref ai.MaxAttackCount, 1, 1, 10);
            ImGui.DragInt("Max Idle Count", ref ai.MaxIdleCount, 1, 0, 10);

            ImGui.SeparatorText("FSM Parameters");
            ImGui.DragFloat("Run Speed", ref ai.RunSpeed, 10f, 0f, 2000f);
            ImGui.DragFloat("Walk Speed", ref ai.WalkSpeed, 10f, 0f, 2000f);
            ImGui.DragFloat("Attack Walk Speed", ref ai.AttackWalkSpeed, 10f, 0f, 2000f);
            ImGui.DragFloat("Avoid Walk Speed", ref ai.AvoidWalkSpeed, 10f, 0f, 2000f);
            ImGui.DragFloat("Avoid Walk Hit Speed", ref ai.AvoidWalkHitSpeed, 10f, 0f, 2000f);
            ImGui.DragFloat("Attack Damage", ref ai.AttackDamageValue, 0.1f, 0f, 100f);
        }
        #endregion

        #region PUBLIC FIELDS
        public bool Enabled = false;
        #endregion

        #region PRIVATE FIELDS
        private readonly List<Entry> _logEntries = new();

        private readonly Dictionary<ELogLevel, bool> _levelFilters = new()
        {
            { ELogLevel.Debug, true },
            { ELogLevel.Info, true },
            { ELogLevel.Warning, true },
            { ELogLevel.Error, true },
            { ELogLevel.Critical, true }
        };
        private float _tempDamage = 10f;
        private float _tempHeal = 10f;
        private ImGuiSink _imguiSink;
        private bool _autoScroll = true;

        private ELogCategory? _selectedCategoryFilter = null;
        #endregion

        private const int MaxLogEntries = 1000;
    }
}