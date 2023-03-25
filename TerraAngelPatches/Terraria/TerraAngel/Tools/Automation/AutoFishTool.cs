using System;
using System.Linq;
using Terraria.DataStructures;

namespace TerraAngel.Tools.Automation;

public class AutoFishTool : Tool
{
    public override string Name => "Auto-Fish";

    public override ToolTabs Tab => ToolTabs.AutomationTools;

    public ref bool AcceptItems => ref ClientConfig.Settings.AutoFishAcceptItems;
    public ref bool AcceptAllItems => ref ClientConfig.Settings.AutoFishAcceptAllItems;
    public ref bool AcceptQuestFish => ref ClientConfig.Settings.AutoFishAcceptQuestFish;
    public ref bool AcceptCrates => ref ClientConfig.Settings.AutoFishAcceptCrates;
    public ref bool AcceptNormal => ref ClientConfig.Settings.AutoFishAcceptNormal;
    public ref bool AcceptCommon => ref ClientConfig.Settings.AutoFishAcceptCommon;
    public ref bool AcceptUncommon => ref ClientConfig.Settings.AutoFishAcceptUncommon;
    public ref bool AcceptRare => ref ClientConfig.Settings.AutoFishAcceptRare;
    public ref bool AcceptVeryRare => ref ClientConfig.Settings.AutoFishAcceptVeryRare;
    public ref bool AcceptLegendary => ref ClientConfig.Settings.AutoFishAcceptLegendary;
    public ref bool AcceptNPCs => ref ClientConfig.Settings.AutoFishAcceptNPCs;
    private ref int frameCountRandomizationMin => ref ClientConfig.Settings.AutoFishFrameCountRandomizationMin;
    private ref int frameCountRandomizationMax => ref ClientConfig.Settings.AutoFishFrameCountRandomizationMax;

    private int FrameCountBeforeActualPullFish = 0;

    private int FrameCountBeforeActualCast = 0;

    private bool WantPullFish = false;

    private bool WantToReCast = false;

    private bool HasSpecialPosition = false;

    private Vector2 SpecialPosition = Vector2.Zero;

    public bool Enabled;

    public override void DrawUI(ImGuiIOPtr io)
    {
        ImGui.Checkbox(Name, ref Enabled);

        if (Enabled)
        {
            if (ImGui.CollapsingHeader("Auto-Fish settings"))
            {
                ImGui.Indent();
                ImGui.Checkbox("Accept Items", ref AcceptItems);
                ImGui.Checkbox("Accept All Items", ref AcceptAllItems);
                if (AcceptItems && !AcceptAllItems)
                {
                    ImGui.Checkbox("Accept Quest Fish", ref AcceptQuestFish);
                    ImGui.Checkbox("Accept Crates", ref AcceptCrates);
                    ImGui.Checkbox("Accept Normal", ref AcceptNormal);
                    ImGui.Checkbox("Accept Common", ref AcceptCommon);
                    ImGui.Checkbox("Accept Uncommon", ref AcceptUncommon);
                    ImGui.Checkbox("Accept Rare", ref AcceptRare);
                    ImGui.Checkbox("Accept Very Rare", ref AcceptVeryRare);
                    ImGui.Checkbox("Accept Legendary", ref AcceptLegendary);
                }
                ImGui.Checkbox("Accept NPCs", ref AcceptNPCs);

                ImGui.SliderInt("Randomization Min", ref frameCountRandomizationMin, 0, 120);
                ImGui.SliderInt("Randomization Max", ref frameCountRandomizationMax, frameCountRandomizationMin, frameCountRandomizationMin + 120);
                ImGui.Checkbox("Use specific mouse position", ref HasSpecialPosition); ImGui.SameLine(); ImGui.TextDisabled("*Press Ctrl+Alt to select specific cast position");
                ImGui.Unindent();
            }
        }
    }

    public override void Update()
    {
        if (frameCountRandomizationMax < frameCountRandomizationMin)
            frameCountRandomizationMax = frameCountRandomizationMin;
        if (Enabled)
        {
            if (HasSpecialPosition)
            {
                ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();
                drawList.AddCircleFilled(Util.WorldToScreenDynamic(SpecialPosition), 10f, Color.Red.PackedValue);

                if (InputSystem.Ctrl && InputSystem.Alt)
                {
                    SpecialPosition = Main.MouseWorld;
                }
            }

            if (WantPullFish)
            {
                FrameCountBeforeActualPullFish--;

                if (FrameCountBeforeActualPullFish <= 0)
                {
                    try
                    {
                        bool canUse = (bool)typeof(Player).GetMethod("ItemCheck_CheckFishingBobbers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(Main.LocalPlayer, new object[] { true })!;
                        WantPullFish = false;
                        WantToReCast = true;
                        FrameCountBeforeActualCast = Main.rand.Next(frameCountRandomizationMin, frameCountRandomizationMax) + 50;
                    }
                    catch (Exception ex)
                    {

                    }

                    ClientLoader.Console.WriteLine("Pulled fish");
                }
            }
            if (WantToReCast)
            {
                if (Main.projectile.Any(x => (x.bobber && x.owner == Main.myPlayer && x.active)))
                    return;

                FrameCountBeforeActualCast--;

                if (FrameCountBeforeActualCast <= 0)
                {
                    Item heldItem = Main.LocalPlayer.HeldItem;

                    if (heldItem.fishingPole > 0)
                    {
                        Main.LocalPlayer.Fishing_GetBait(out int baitPower, out int baitType);

                        if (baitPower > 0 && baitType > 0)
                        {
                            Main.LocalPlayer.controlUseItem = true;
                            int mx = Main.mouseX;
                            int my = Main.mouseY;
                            if (HasSpecialPosition)
                            {
                                Main.mouseX = (int)SpecialPosition.X - (int)Main.screenPosition.X;
                                Main.mouseY = (int)SpecialPosition.Y - (int)Main.screenPosition.Y;
                            }
                            Main.LocalPlayer.ItemCheck();
                            NetMessage.SendData(MessageID.PlayerControls, number: Main.myPlayer);

                            Main.mouseX = mx;
                            Main.mouseY = my;

                        }
                    }
                    WantToReCast = false;
                }

            }

        }
    }

    public void FishingCheck(Projectile bobber, FishingAttempt fish)
    {
        if (bobber.owner == Main.myPlayer && Enabled)
        {
            bool wantToCatch = false;
            if (fish.rolledItemDrop > 0)
            {
                ClientLoader.Console.WriteLine($"Fish: {InternalRepresentation.GetItemIDName(fish.rolledItemDrop)}");
                if (AcceptItems)
                {
                    if (!fish.crate && fish.questFish == -1 && !fish.common && !fish.uncommon && !fish.rare && !fish.veryrare && !fish.legendary && AcceptNormal)
                    {
                        wantToCatch = true;
                    }

                    if (AcceptQuestFish && fish.questFish != -1)
                    {
                        wantToCatch = true;
                    }

                    if (AcceptCrates && fish.crate)
                    {
                        wantToCatch = true;
                    }

                    if (AcceptCommon && fish.common)
                    {
                        wantToCatch = true;
                    }

                    if (AcceptUncommon && fish.uncommon)
                    {
                        wantToCatch = true;
                    }

                    if (AcceptRare && fish.rare)
                    {
                        wantToCatch = true;
                    }

                    if (AcceptVeryRare && fish.veryrare)
                    {
                        wantToCatch = true;
                    }

                    if (AcceptLegendary && fish.legendary)
                    {
                        wantToCatch = true;
                    }

                }
            }

            if (fish.rolledEnemySpawn > 0)
            {
                ClientLoader.Console.WriteLine($"NPC: {InternalRepresentation.GetNPCIDName(fish.rolledEnemySpawn)}");
                if (AcceptNPCs)
                {
                    wantToCatch = true;
                }
            }

            if (wantToCatch)
            {
                WantPullFish = true;
                FrameCountBeforeActualPullFish = Main.rand.Next(frameCountRandomizationMin, frameCountRandomizationMax);
            }
        }
    }
}
