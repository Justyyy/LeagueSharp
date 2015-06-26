using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;
using ItemData = LeagueSharp.Common.Data.ItemData;
using Color = System.Drawing.Color;

namespace JustEzreal
{
    internal class Program
    {
        public const string ChampName = "Ezreal";
        public const string Menun = "JustEzreal";
        public static Menu Config;
        public static Items.Item TearoftheGoddess = new Items.Item(3070, 0);
        public static Items.Item TearoftheGoddessCrystalScar = new Items.Item(3073, 0);
        public static Items.Item ArchangelsStaff = new Items.Item(3003, 0);
        public static Items.Item ArchangelsStaffCrystalScar = new Items.Item(3007, 0);
        public static Items.Item Manamune = new Items.Item(3004, 0);
        public static Items.Item ManamuneCrystalScar = new Items.Item(3008, 0);
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R;

        private static Obj_AI_Hero currentTarget
        {
            get
            {
                if (Hud.SelectedUnit != null && Hud.SelectedUnit is Obj_AI_Hero && Hud.SelectedUnit.Team != player.Team)
                    return (Obj_AI_Hero) Hud.SelectedUnit;
                if (TargetSelector.GetSelectedTarget() != null)
                    return TargetSelector.GetSelectedTarget();
                return TargetSelector.GetTarget(Q.Range + 175, TargetSelector.DamageType.Physical);
            }
        }

        private static SpellSlot Ignite;
        private static readonly Obj_AI_Hero player = ObjectManager.Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            if (player.ChampionName != ChampName)
                return;

            Notifications.AddNotification("JustEzreal Loaded | Please give feedback on forum", 8000).SetTextColor(Color.NavajoWhite); ;
            Notifications.AddNotification("Don't forget upvote in AssemblyDB", 12000).SetTextColor(Color.Red); ;

            //Ability Information - Range - Variables.
            Q = new Spell(SpellSlot.Q, 1200);
            Q.SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine);
            W = new Spell(SpellSlot.W, 800);
            W.SetSkillshot(0.25f, 80f, 1600f, false, SkillshotType.SkillshotLine);
            E = new Spell(SpellSlot.E, 475);
            R = new Spell(SpellSlot.R, 2500);
            R.SetSkillshot(1f, 160f, 2000f, false, SkillshotType.SkillshotLine);

            Config = new Menu(Menun, Menun, true);
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Combo
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("qhit", "Q Hitchance 1-Low, 4-Very High")).SetValue(new Slider(3, 1, 4));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("emode", "E Usage Modes").SetValue(new StringList(new[] { "To Target", "To Mouse Cursor" })));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Rene", "Min Enemies for R").SetValue(new Slider(2, 1, 5)));

            //Harass
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("hQ", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("hW", "Use W").SetValue(true));
            Config.SubMenu("Harass")
                .AddItem(new MenuItem("harassmana", "Mana Percentage").SetValue(new Slider(30, 0, 100)));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("AutoHarass", "Auto Harass", true).SetValue(new KeyBind("J".ToCharArray()[0],
                        KeyBindType.Toggle)));
            Config.SubMenu("Harass").AddItem(new MenuItem("aQ", "Use Q for Auto Harass").SetValue(true));
            Config.SubMenu("Harass")
                .AddItem(new MenuItem("qmana", "Q Mana Percentage").SetValue(new Slider(30, 0, 100)));
            Config.SubMenu("Harass").AddItem(new MenuItem("aW", "Use W for Auto Harass").SetValue(true));
            Config.SubMenu("Harass")
                .AddItem(new MenuItem("wmana", "W Mana Percentage").SetValue(new Slider(30, 0, 100)));


            //Farm
            Config.AddSubMenu(new Menu("Farm", "Farm"));
            Config.SubMenu("Farm").AddItem(new MenuItem("fq", "Use Q for Auto Farm").SetValue(true));
            Config.SubMenu("Farm")
                .AddItem(new MenuItem("qfmana", "Q Mana Percentage").SetValue(new Slider(30, 0, 100)));


            //Item
            Config.AddSubMenu(new Menu("Item", "Item"));
            Config.SubMenu("Item").AddItem(new MenuItem("useGhostblade", "Use Youmuu's Ghostblade").SetValue(true));
            Config.SubMenu("Item").AddItem(new MenuItem("UseBOTRK", "Use Blade of the Ruined King").SetValue(true));
            Config.SubMenu("Item").AddItem(new MenuItem("eL", "  Enemy HP Percentage").SetValue(new Slider(80, 0, 100)));
            Config.SubMenu("Item").AddItem(new MenuItem("oL", "  Own HP Percentage").SetValue(new Slider(65, 0, 100)));
            Config.SubMenu("Item").AddItem(new MenuItem("UseBilge", "Use Bilgewater Cutlass").SetValue(true));
            Config.SubMenu("Item")
                .AddItem(new MenuItem("HLe", "  Enemy HP Percentage").SetValue(new Slider(80, 0, 100)));
            Config.SubMenu("Item").AddItem(new MenuItem("UseIgnite", "Use Ignite").SetValue(true));

            //Laneclear
            Config.AddSubMenu(new Menu("Clear", "Clear"));
            Config.SubMenu("Clear").AddItem(new MenuItem("lQ", "Use Q").SetValue(true));
            Config.SubMenu("Clear")
                .AddItem(new MenuItem("qlmana", "Q Mana Percentage").SetValue(new Slider(30, 0, 100)));

            //Draw
            Config.AddSubMenu(new Menu("Draw", "Draw"));
            Config.SubMenu("Draw").AddItem(new MenuItem("Draw_Disabled", "Disable All Spell Drawings").SetValue(false));
            Config.SubMenu("Draw").AddItem(new MenuItem("Qdraw", "Draw Q Range").SetValue(true));
            Config.SubMenu("Draw").AddItem(new MenuItem("Wdraw", "Draw W Range").SetValue(true));
            Config.SubMenu("Draw").AddItem(new MenuItem("Edraw", "Draw E Range").SetValue(true));
            Config.SubMenu("Draw").AddItem(new MenuItem("Rdraw", "Draw R Range").SetValue(true));
            Config.SubMenu("Draw").AddItem(new MenuItem("qpred", "Draw Prediction")).SetValue(true);

            //Misc
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("stacktear", "Stack Tear in Fountain").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("ksQ", "Killsteal with Q").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("ksW", "Killsteal with W").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("ksR", "Killsteal with R").SetValue(false));
            Config.SubMenu("Misc")
                .AddItem(new MenuItem("ksRR", "KS R Range").SetValue(new Slider(1000, 0, (int) R.Range)));
            var dmg = new MenuItem("combodamage", "Damage Indicator").SetValue(true);
            var drawFill = new MenuItem("color", "Fill colour", true).SetValue(new Circle(true, Color.Orange));
            Config.SubMenu("Draw").AddItem(drawFill);
            Config.SubMenu("Draw").AddItem(dmg);

            DrawDamage.DamageToUnit = GetComboDamage;
            DrawDamage.Enabled = dmg.GetValue<bool>();
            DrawDamage.Fill = drawFill.GetValue<Circle>().Active;
            DrawDamage.FillColor = drawFill.GetValue<Circle>().Color;

            dmg.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DrawDamage.Enabled = eventArgs.GetNewValue<bool>();
            };

            drawFill.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DrawDamage.Fill = eventArgs.GetNewValue<Circle>().Active;
                DrawDamage.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };

            Config.AddToMainMenu();
            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
        }

        private static float GetComboDamage(Obj_AI_Hero enemy)
        {
            double damage = 0d;

            if (Q.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.Q);

            if (W.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
                damage += RDamage(enemy);

            if (Ignite.IsReady())
                damage += IgniteDamage(enemy);

            return (float) damage;
        }

        private static void combo()
        {
            int mode = Config.Item("emode").GetValue<StringList>().SelectedIndex;
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValidTarget())
                return;

            if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>() && target.IsValidTarget(Q.Range))
            {
                Q.CastIfHitchanceEquals(target, HitChance.VeryHigh);
            }

            if (W.IsReady() && Config.Item("UseW").GetValue<bool>() && target.IsValidTarget(W.Range))
            {
                {
                    W.CastIfHitchanceEquals(target, HitChance.High);
                }
            }

            if (E.IsReady() && Config.Item("UseE").GetValue<bool>() && target.IsValidTarget(E.Range))
            {
                {
                    if (mode == 0)
                    {
                        E.Cast(target);
                    }
                    else if (mode == 1)
                    {
                        E.Cast(Game.CursorPos);
                    }
                }
            }

            var hit = (Config.Item("Rene").GetValue<Slider>().Value);
            if (R.IsReady() && Config.Item("UseR").GetValue<bool>() && target.IsValidTarget(R.Range))
            {
                {
                    R.CastIfWillHit(target, hit);
                }
            }
            
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                items();
        }

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (Ignite == SpellSlot.Unknown || player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
                return 0f;
            return (float) player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        private static void Killsteal()
        {
            if (Config.Item("ksQ").GetValue<bool>() && Q.IsReady())
            {
                var target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            enemy =>
                                enemy.IsValidTarget(Q.Range) && enemy.Health < player.GetSpellDamage(enemy, SpellSlot.Q));
                if (target != null && target.IsValidTarget(Q.Range))
                {
                    Q.CastIfHitchanceEquals(target, HitChance.High);
                }
            }

            if (Config.Item("ksW").GetValue<bool>() && W.IsReady())
            {
                var target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            enemy =>
                                enemy.IsValidTarget(W.Range) && enemy.Health < player.GetSpellDamage(enemy, SpellSlot.W));
                if (target != null && target.IsValidTarget(W.Range))
                {
                    W.CastIfHitchanceEquals(target, HitChance.High);
                }
            }

            if (Config.Item("ksR").GetValue<bool>() && R.IsReady())
            {
                var target2 =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            enemy =>
                                enemy.IsValidTarget(Q.Range) && enemy.Health < player.GetSpellDamage(enemy, SpellSlot.Q));
               
                if (target2 != null && target2.IsValidTarget(Q.Range) && target2.Health < Q.GetDamage(target2))
                    return;

                var rrange = Config.Item("ksRR").GetValue<Slider>().Value;
                var target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            enemy =>
                                enemy.IsValidTarget(R.Range) 
                                );
                if (target != null && ObjectManager.Player.Distance(target) >= rrange && target.Health < RDamage(target))
                {
                    R.CastIfHitchanceEquals(target, HitChance.High);
                }
            }
        }

        private static void items()
        {
            Ignite = player.GetSpellSlot("summonerdot");
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;

            var botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            var Ghost = ItemData.Youmuus_Ghostblade.GetItem();
            var cutlass = ItemData.Bilgewater_Cutlass.GetItem();

            if (botrk.IsReady() && botrk.IsOwned(player) && botrk.IsInRange(target)
                && target.HealthPercent <= Config.Item("eL").GetValue<Slider>().Value
                && Config.Item("UseBOTRK").GetValue<bool>())

                botrk.Cast(target);

            if (botrk.IsReady() && botrk.IsOwned(player) && botrk.IsInRange(target)
                && target.HealthPercent <= Config.Item("oL").GetValue<Slider>().Value
                && Config.Item("UseBOTRK").GetValue<bool>())

                botrk.Cast(target);

            if (cutlass.IsReady() && cutlass.IsOwned(player) && cutlass.IsInRange(target) &&
                target.HealthPercent <= Config.Item("HLe").GetValue<Slider>().Value
                && Config.Item("UseBilge").GetValue<bool>())

                cutlass.Cast(target);

            if (Ghost.IsReady() && Ghost.IsOwned(player) && target.IsValidTarget(E.Range)
                && Config.Item("useGhostblade").GetValue<bool>())

                Ghost.Cast();

            if (player.Distance(target.Position) <= 600 && IgniteDamage(target) >= target.Health &&
                Config.Item("UseIgnite").GetValue<bool>())
                player.Spellbook.CastSpell(Ignite, target);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (player.IsDead || MenuGUI.IsChatOpen || player.IsRecalling())
            {
                return;
            }

            if (Config.Item("stacktear").GetValue<bool>() && Q.IsReady() && ObjectManager.Player.InFountain() &&
                (TearoftheGoddess.IsOwned(player) || TearoftheGoddessCrystalScar.IsOwned(player) || ArchangelsStaff.IsOwned(player) || ArchangelsStaffCrystalScar.IsOwned(player) || Manamune.IsOwned(player) || ManamuneCrystalScar.IsOwned(player)))
                Q.Cast(ObjectManager.Player, true, true);

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    harass();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Farm();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Clear();
                    break;
            }


            Killsteal();
            var autoHarass = Config.Item("AutoHarass", true).GetValue<KeyBind>().Active;
            if (autoHarass)
                AutoHarass();
        }

        private static void AutoHarass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var qmana = Config.Item("qmana").GetValue<Slider>().Value;
            var wmana = Config.Item("wmana").GetValue<Slider>().Value;
            if (target == null || !target.IsValidTarget())
                return;

            if (Q.IsReady() && Config.Item("aQ").GetValue<bool>() && target.IsValidTarget(Q.Range) &&
                player.ManaPercent >= qmana)
            {
                Q.CastIfHitchanceEquals(target, HitChance.Dashing, true);
                Q.CastIfHitchanceEquals(target, HitChance.Immobile, true);
                var qpred = Q.GetPrediction(target);
                if (qpred.Hitchance >= (HitChance)Config.Item("qhit").GetValue<Slider>().Value + 1 &&
                    qpred.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 2)
                {
                    Q.Cast(qpred.CastPosition);
                }
            }

            if (W.IsReady() && Config.Item("aW").GetValue<bool>() && target.IsValidTarget(W.Range) &&
                player.ManaPercent >= wmana)
            {
                W.CastIfHitchanceEquals(target, HitChance.VeryHigh);
            }
        }

       private static void Farm()
        {
            var minions = MinionManager.GetMinions(player.ServerPosition, Q.Range);
            if (minions.Count <= 0)
                return;

            var lastmana = Config.Item("qfmana").GetValue<Slider>().Value;

            if (Q.IsReady() && Config.Item("fq").GetValue<bool>() && player.ManaPercent >= lastmana)
            {
                var qtarget =
                    minions.Where(
                        x =>
                            x.Distance(player) < Q.Range && Q.GetPrediction(x).Hitchance >= HitChance.Medium &&
                            (x.Health < player.GetSpellDamage(x, SpellSlot.Q) &&
                             !(x.Health < player.GetAutoAttackDamage(x))))
                        .OrderByDescending(x => x.Health)
                        .FirstOrDefault();
                if (HealthPrediction.GetHealthPrediction(qtarget, (int) 0.25) <=
                    player.GetSpellDamage(qtarget, SpellSlot.Q))
                    Q.Cast(qtarget);
            }

        }

        private static void harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var mana = Config.Item("harassmana").GetValue<Slider>().Value;
            if (target == null || !target.IsValidTarget())
                return;

            if (Q.IsReady() && Config.Item("hQ").GetValue<bool>() && target.IsValidTarget(Q.Range) &&
                player.ManaPercent >= mana)
            {
                Q.CastIfHitchanceEquals(target, HitChance.VeryHigh);
            }
            if (W.IsReady() && Config.Item("hW").GetValue<bool>() && target.IsValidTarget(W.Range) &&
                player.ManaPercent >= mana)
            {
                W.CastIfHitchanceEquals(target, HitChance.High);
            }
        }

        private static void Clear()
        {
            var qmana = Config.Item("qlmana").GetValue<Slider>().Value;
            var minionCount = MinionManager.GetMinions(player.Position, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            {
                foreach (var minion in minionCount)
                {
                    if (Config.Item("lQ").GetValue<bool>()
                        && Q.IsReady()
                        && minion.IsValidTarget(Q.Range)
                        && player.ManaPercent >= qmana)
                    {
                        Q.Cast(minion);
                    }
                }
            }
        }

        private static float RDamage(Obj_AI_Hero hero)
        {
            if (!R.IsReady()) return 0;

            float reduction = 1f -
                              ((R.GetCollision(player.Position.To2D(), new List<SharpDX.Vector2> {hero.Position.To2D()})
                                  .Count)/10f);
            reduction = (reduction < 0.3f ? 0.3f : reduction);

            return
                (float)
                    Damage.CalcDamage(player, hero, Damage.DamageType.Magical,
                        Damage.GetSpellDamage(player, hero, SpellSlot.R)*reduction);
        }

        private static void OnDraw(EventArgs args)
        {
            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;

            if (Config.Item("Qdraw").GetValue<bool>())
                Render.Circle.DrawCircle(player.Position, Q.Range, System.Drawing.Color.White, 3);
            if (Config.Item("Wdraw").GetValue<bool>())
                Render.Circle.DrawCircle(player.Position, W.Range, System.Drawing.Color.White, 3);
            if (Config.Item("Edraw").GetValue<bool>())
                Render.Circle.DrawCircle(player.Position, E.Range, System.Drawing.Color.White, 3);
            if (Config.Item("Rdraw").GetValue<bool>())
                Render.Circle.DrawCircle(player.Position, R.Range, System.Drawing.Color.White, 3);
            if (Config.SubMenu("Draw").Item("qpred").GetValue<bool>() && !player.IsDead)
            {
                if (currentTarget != null && player.Distance(currentTarget) < Q.Range + 200)
                {
                    var playerPos = Drawing.WorldToScreen(player.Position);
                    var targetPos = Drawing.WorldToScreen(currentTarget.Position);
                    Drawing.DrawLine(playerPos, targetPos, 4,
                        Q.GetPrediction(currentTarget, overrideRange: Q.Range).Hitchance < HitChance.High
                            ? Color.Gray
                            : Color.SpringGreen);
                }
            }
        }

    }
}
    
