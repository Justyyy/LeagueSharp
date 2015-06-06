using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Printing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using Color = System.Drawing.Color;
using SharpDX;

namespace JustRyze
{
    internal class Program
    {
        public static Items.Item HealthPotion = new Items.Item(2003, 0);
        public static Items.Item ManaPotion = new Items.Item(2004, 0);
        public static Items.Item CrystallineFlask = new Items.Item(2041, 0);
        public static Items.Item BiscuitofRejuvenation = new Items.Item(2010, 0);
        public static Items.Item TearoftheGoddess = new Items.Item(3070, 0);
        public static Items.Item TearoftheGoddessCrystalScar = new Items.Item(3073, 0);
        public static Items.Item ArchangelsStaff = new Items.Item(3003, 0);
        public static Items.Item ArchangelsStaffCrystalScar = new Items.Item(3007, 0);
        public static Items.Item Manamune = new Items.Item(3004, 0);
        public static Items.Item ManamuneCrystalScar = new Items.Item(3008, 0);
        private static Obj_AI_Hero currentTarget
        {
            get
            {
                if (Hud.SelectedUnit != null && Hud.SelectedUnit is Obj_AI_Hero && Hud.SelectedUnit.Team != player.Team)
                    return (Obj_AI_Hero)Hud.SelectedUnit;
                if (TargetSelector.GetSelectedTarget() != null)
                    return TargetSelector.GetSelectedTarget();
                return TargetSelector.GetTarget(Q.Range + 175, TargetSelector.DamageType.Magical);
            }
        }
        public const string ChampName = "Ryze";
        public const string Menuname = "JustRyze";
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R;
        private static SpellSlot Ignite;
        public static int[] abilitySequence;
        public static int qOff = 0, wOff = 0, eOff = 0, rOff = 0;
        private static readonly Obj_AI_Hero player = ObjectManager.Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;

        }

        private static void OnLoad(EventArgs args)
        {
            if (player.ChampionName != ChampName)
                return;

            Notifications.AddNotification("JustRyze Loaded - [V.1.0.1.0]", 8000);

            //Ability Information - Range - Variables.
            Q = new Spell(SpellSlot.Q, 900f);
            W = new Spell(SpellSlot.W, 600f);
            E = new Spell(SpellSlot.E, 600f);
            R = new Spell(SpellSlot.R);
            Q.SetSkillshot(0.46f, 50f, 1399f, true, SkillshotType.SkillshotLine);


            abilitySequence = new int[] { 3, 1, 2, 3, 3, 4, 3, 1, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2 };


            Config = new Menu(Menuname, Menuname, true);
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Combo
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseAA", "Use AA Options").SetValue(new StringList(new[] { "Use AA", "No AA" })));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Rene", "Min Enemies for R").SetValue(new Slider(2, 1, 5)));

            //Harass
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("hQ", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("hW", "Use W").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("hE", "Use E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("hQA", "Use Auto Q Harass").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("hWA", "Use Auto W Harass").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("hEA", "Use Auto E Harass").SetValue(false));
            Config.SubMenu("Harass")
                .AddItem(new MenuItem("harassmana", "Mana Percentage").SetValue(new Slider(30, 0, 100)));

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
            Config.SubMenu("Clear")
                .AddItem(
                    new MenuItem("laneQ", "Use Q").SetValue(
                        new StringList(new[] { "Don't Cast ", "Always", "Only If Tears" }, 1)));
            Config.SubMenu("Clear").AddItem(new MenuItem("laneW", "Use W").SetValue(true));
            Config.SubMenu("Clear").AddItem(new MenuItem("laneE", "Use E").SetValue(true));
            Config.SubMenu("Clear").AddItem(new MenuItem("emin", "Min Minion for E").SetValue(new Slider(3, 1, 5)));
            Config.SubMenu("Clear")
                .AddItem(new MenuItem("lanemana", "Mana Percentage").SetValue(new Slider(30, 0, 100)));

            //Draw
            Config.AddSubMenu(new Menu("Draw", "Draw"));
            Config.SubMenu("Draw").AddItem(new MenuItem("Draw_Disabled", "Disable All Spell Drawings").SetValue(false));
            Config.SubMenu("Draw").AddItem(new MenuItem("Qdraw", "Draw Q Range").SetValue(true));
            Config.SubMenu("Draw").AddItem(new MenuItem("Wdraw", "Draw W Range").SetValue(true));
            Config.SubMenu("Draw").AddItem(new MenuItem("Edraw", "Draw E Range").SetValue(true));
            Config.SubMenu("Draw").AddItem(new MenuItem("combodamage", "Damage on HPBar")).SetValue(true);
            Config.SubMenu("Draw").AddItem(new MenuItem("qpred", "Draw Prediction")).SetValue(true);

            //Draw
            Config.AddSubMenu(new Menu("LastHit", "LastHit"));
            Config.SubMenu("LastHit").AddItem(new MenuItem("fQ", "Last Hit Q").SetValue(false));
            Config.SubMenu("LastHit").AddItem(new MenuItem("fQA", "Auto Hit Q").SetValue(false));
            Config.SubMenu("LastHit").AddItem(new MenuItem("fW", "Last Hit W").SetValue(false));
            Config.SubMenu("LastHit").AddItem(new MenuItem("fE", "Last Hit E").SetValue(false));
            Config.SubMenu("LastHit")
                .AddItem(new MenuItem("lastmana", "Mana Percentage").SetValue(new Slider(30, 0, 100)));

            //Misc
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("ksQ", "Killsteal with Q").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("ksW", "Killsteal with W").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("ksE", "Killsteal with E").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("stacktear", "Stack Tear in Fountain").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("antigap", "AntiGapCloser with W").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("autolevel", "Auto Level Spells").SetValue(false));

            Config.AddToMainMenu();
            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (W.IsReady() && gapcloser.Sender.IsValidTarget(275) && Config.Item("antigap").GetValue<bool>())
                W.CastOnUnit(gapcloser.Sender);
        }

        private static void combo()
        {
            var t = TargetSelector.GetTarget(ObjectManager.Player.AttackRange, TargetSelector.DamageType.Magical);
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var enemys = target.CountEnemiesInRange(200);
            if (target == null || !target.IsValidTarget())
                return;

            switch (Config.Item("UseAA").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    {
                        if (t.IsValidTarget() && (!E.IsReady() && (!Q.IsReady() && !W.IsReady() && !R.IsReady())))
                            Orbwalking.Attack = true;
                        else
                            Orbwalking.Attack = false;
                        break;
                    }
                case 1:
                    {
                        if (t.IsValidTarget() && ObjectManager.Player.GetAutoAttackDamage(t) > t.Health)
                            Orbwalking.Attack = true;
                        else
                            Orbwalking.Attack = false;
                        break;
                    }
            }

            if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>() && target.IsValidTarget(Q.Range))
            {
                Q.CastIfHitchanceEquals(target, HitChance.Dashing, true);
                Q.CastIfHitchanceEquals(target, HitChance.Immobile, true);
                var qpred = Q.GetPrediction(target);
                if (qpred.Hitchance >= HitChance.VeryHigh &&
                    qpred.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 2)
                {
                    Q.Cast(qpred.CastPosition);
                }
            }

            if (R.IsReady() && Config.Item("UseR").GetValue<bool>() && target.IsValidTarget(R.Range))
                if (Config.Item("Rene").GetValue<Slider>().Value <= enemys)
                    R.Cast();

            if (W.IsReady() && target.IsValidTarget(W.Range - 10) && Config.Item("UseW").GetValue<bool>())
                W.CastOnUnit(target);

            if (E.IsReady() && target.IsValidTarget(E.Range) && Config.Item("UseE").GetValue<bool>())
                E.CastOnUnit(target);


            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                items();
        }

        private static void Combo2()
        {

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var enemys = target.CountEnemiesInRange(200);
            var t = TargetSelector.GetTarget(ObjectManager.Player.AttackRange, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;

            switch (Config.Item("UseAA").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    {
                        if (t.IsValidTarget() && (!E.IsReady() && (!Q.IsReady() && !W.IsReady())))
                            Orbwalking.Attack = true;
                        else
                            Orbwalking.Attack = false;
                        break;
                    }
                case 1:
                    {
                        if (t.IsValidTarget() && ObjectManager.Player.GetAutoAttackDamage(t) > t.Health)
                            Orbwalking.Attack = true;
                        else
                            Orbwalking.Attack = false;
                        break;
                    }
            }

            {
                if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>() && target.IsValidTarget(Q.Range))
                {
                    Q.CastIfHitchanceEquals(target, HitChance.Dashing, true);
                    Q.CastIfHitchanceEquals(target, HitChance.Immobile, true);
                    var qpred = Q.GetPrediction(target);
                    if (qpred.Hitchance >= HitChance.VeryHigh &&
                        qpred.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 2)
                    {
                        Q.Cast(qpred.CastPosition);
                    }
                }

                if (R.IsReady() && Config.Item("UseR").GetValue<bool>() && target.IsValidTarget(R.Range))
                    if (Config.Item("Rene").GetValue<Slider>().Value <= enemys)
                        R.Cast();

                if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>() && target.IsValidTarget(Q.Range))
                {
                    Q.CastIfHitchanceEquals(target, HitChance.Dashing, true);
                    Q.CastIfHitchanceEquals(target, HitChance.Immobile, true);
                    var qpred = Q.GetPrediction(target);
                    if (qpred.Hitchance >= HitChance.VeryHigh &&
                        qpred.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 2)
                    {
                        Q.Cast(qpred.CastPosition);
                    }
                }

                if (W.IsReady() && target.IsValidTarget(W.Range) && Config.Item("UseW").GetValue<bool>())
                    W.Cast();

                if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>() && target.IsValidTarget(Q.Range))
                {
                    Q.CastIfHitchanceEquals(target, HitChance.Dashing, true);
                    Q.CastIfHitchanceEquals(target, HitChance.Immobile, true);
                    var qpred = Q.GetPrediction(target);
                    if (qpred.Hitchance >= HitChance.VeryHigh &&
                        qpred.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 2)
                    {
                        Q.Cast(qpred.CastPosition);
                    }
                }

                if (E.IsReady() && target.IsValidTarget(2000) && Config.Item("UseE").GetValue<bool>())
                    E.Cast();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    items();
            }
        }

        private static float GetComboDamage(Obj_AI_Hero Target)
        {
            if (Target != null)
            {
                float ComboDamage = new float();

                ComboDamage += Q.IsReady() ? Q.GetDamage(Target) : 0;
                ComboDamage += W.IsReady() ? W.GetDamage(Target) : 0;
                ComboDamage += E.IsReady() ? E.GetDamage(Target) : 0;
                ComboDamage += Ignite.IsReady() ? IgniteDamage(Target) : 0;
                ComboDamage += player.TotalAttackDamage;
                return ComboDamage;
            }
            return 0;
        }

        private static float[] GetLength()
        {
            var Target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (Target != null)
            {
                float[] Length =
                {
                    GetComboDamage(Target) > Target.Health
                        ? 0
                        : (Target.Health - GetComboDamage(Target))/Target.MaxHealth,
                    Target.Health/Target.MaxHealth
                };
                return Length;
            }
            return new float[] { 0, 0 };
        }

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (Ignite == SpellSlot.Unknown || player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
                return 0f;
            return (float)player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
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
                if (target.IsValidTarget(Q.Range))
                {
                    {
                        Q.CastIfHitchanceEquals(target, HitChance.Dashing, true);
                        Q.CastIfHitchanceEquals(target, HitChance.Immobile, true);
                        var qpred = Q.GetPrediction(target);
                        if (qpred.Hitchance >= HitChance.VeryHigh &&
                            qpred.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 2)
                        {
                            Q.Cast(qpred.CastPosition);
                        }
                    }
                }
            }

            if (Config.Item("ksW").GetValue<bool>() && W.IsReady())
            {
                var target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            enemy =>
                                enemy.IsValidTarget(W.Range) && enemy.Health < player.GetSpellDamage(enemy, SpellSlot.W));
                if (target.IsValidTarget(W.Range))
                {
                    W.CastOnUnit(target);
                }
            }

            if (Config.Item("ksE").GetValue<bool>() && E.IsReady())
            {
                var target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            enemy =>
                                enemy.IsValidTarget(E.Range) && enemy.Health < player.GetSpellDamage(enemy, SpellSlot.E));
                if (target.IsValidTarget(E.Range))
                {
                    E.CastOnUnit(target);
                }
            }
        }

        private static void items()
        {
            Ignite = player.GetSpellSlot("summonerdot");
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
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

            if (Ghost.IsReady() && Ghost.IsOwned(player) && target.IsValidTarget(Q.Range)
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

            Orbwalking.Attack = true;

            if (Config.Item("stacktear").GetValue<bool>() && Q.IsReady() && ObjectManager.Player.InFountain() &&
                (TearoftheGoddess.IsOwned(player) || TearoftheGoddessCrystalScar.IsOwned(player) || ArchangelsStaff.IsOwned(player) || ArchangelsStaffCrystalScar.IsOwned(player) || Manamune.IsOwned(player) || ManamuneCrystalScar.IsOwned(player)))
                Q.Cast(ObjectManager.Player, true, true);

            if (Config.Item("autolevel").GetValue<bool>()) LevelUpSpells();

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.LastHit:
                    Lasthit();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Clear();
                    break;
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo &&
                player.Buffs.Count(buf => buf.Name == "RyzePassiveStack") == 4)
                Combo2();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo &&
                player.Buffs.Count(buf => buf.Name == "RyzePassiveStack") < 4)
                combo();

            if (Config.Item("fQA").GetValue<bool>())
                Lasthit2();

            Killsteal();
            AutoHarass();
        }

        private static void Lasthit()
        {
            var minions = MinionManager.GetMinions(player.ServerPosition, Q.Range);
            if (minions.Count <= 0)
                return;

            var lastmana = Config.Item("lastmana").GetValue<Slider>().Value;

            if (Q.IsReady() && Config.Item("fQ").GetValue<bool>() && player.ManaPercent >= lastmana)
            {
                var qtarget = minions.Where(x => x.Distance(player) < Q.Range && Q.GetPrediction(x).Hitchance >= HitChance.High && (x.Health < player.GetSpellDamage(x, SpellSlot.Q) && !(x.Health < player.GetAutoAttackDamage(x)))).OrderByDescending(x => x.Health).FirstOrDefault();
                if (HealthPrediction.GetHealthPrediction(qtarget, (int)0.25) <= player.GetSpellDamage(qtarget, SpellSlot.Q))
                    Q.Cast(qtarget);
            }

            if (W.IsReady() && Config.Item("fW").GetValue<bool>() && player.ManaPercent >= lastmana)
            {
                var wtarget = minions.Where(x => x.Distance(player) < W.Range && (x.Health < player.GetSpellDamage(x, SpellSlot.W) && !(x.Health < player.GetAutoAttackDamage(x)))).OrderByDescending(x => x.Health).FirstOrDefault();
                if (HealthPrediction.GetHealthPrediction(wtarget, (int)0.25) <= player.GetSpellDamage(wtarget, SpellSlot.W))
                    W.CastOnUnit(wtarget);
            }

            if (E.IsReady() && Config.Item("fE").GetValue<bool>() && player.ManaPercent >= lastmana)
            {
                var etarget = minions.Where(x => x.Distance(player) < E.Range && (x.Health < player.GetSpellDamage(x, SpellSlot.E) && !(x.Health < player.GetAutoAttackDamage(x)))).OrderByDescending(x => x.Health).FirstOrDefault();
                if (HealthPrediction.GetHealthPrediction(etarget, (int)0.25) <= player.GetSpellDamage(etarget, SpellSlot.E))
                    E.CastOnUnit(etarget);
            }

        }

        private static void Lasthit2()
        {
            var min = MinionManager.GetMinions(player.ServerPosition, Q.Range);
            if (min.Count <= 0)
                return;

            var lastmana = Config.Item("lastmana").GetValue<Slider>().Value;

            if (Q.IsReady() && player.ManaPercent >= lastmana)
            {
                var qtarget =
                    min.Where(
                        x =>
                            x.Distance(player) < Q.Range && Q.GetPrediction(x).Hitchance >= HitChance.High &&
                            (x.Health < player.GetSpellDamage(x, SpellSlot.Q) &&
                             !(x.Health < player.GetAutoAttackDamage(x))))
                        .OrderByDescending(x => x.Health)
                        .FirstOrDefault();
                if (HealthPrediction.GetHealthPrediction(qtarget, (int) 0.25) <=
                    player.GetSpellDamage(qtarget, SpellSlot.Q))
                    Q.Cast(qtarget);
            }
        }

        private static void AutoHarass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (!Q.IsReady() || player.IsRecalling() || target == null || !target.IsValidTarget())
                return;
            var harassmana = Config.Item("harassmana").GetValue<Slider>().Value;

            if (Q.IsReady() && Config.Item("hQA").GetValue<bool>() && target.IsValidTarget(Q.Range) && player.ManaPercent >= harassmana)
            {
                Q.CastIfHitchanceEquals(target, HitChance.Dashing, true);
                Q.CastIfHitchanceEquals(target, HitChance.Immobile, true);
                var qpred = Q.GetPrediction(target);
                if (qpred.Hitchance >= HitChance.VeryHigh &&
                    qpred.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 2)
                {
                    Q.Cast(qpred.CastPosition);
                }
            }

            if (W.IsReady() && Config.Item("hWA").GetValue<bool>() && target.IsValidTarget(W.Range) && player.ManaPercent >= harassmana)
            {
                W.CastOnUnit(target);
            }

            if (E.IsReady() && Config.Item("hEA").GetValue<bool>() && target.IsValidTarget(E.Range) && player.ManaPercent >= harassmana)
            {
                E.CastOnUnit(target);
            }
        }

        //Thanks to LuNi
        private static void LevelUpSpells()
        {
            int qL = player.Spellbook.GetSpell(SpellSlot.Q).Level + qOff;
            int wL = player.Spellbook.GetSpell(SpellSlot.W).Level + wOff;
            int eL = player.Spellbook.GetSpell(SpellSlot.E).Level + eOff;
            int rL = player.Spellbook.GetSpell(SpellSlot.R).Level + rOff;
            if (qL + wL + eL + rL < ObjectManager.Player.Level)
            {
                int[] level = new int[] { 0, 0, 0, 0 };
                for (int i = 0; i < ObjectManager.Player.Level; i++)
                {
                    level[abilitySequence[i] - 1] = level[abilitySequence[i] - 1] + 1;
                }
                if (qL < level[0]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                if (wL < level[1]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                if (eL < level[2]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                if (rL < level[3]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);

            }
        }

        private static void harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var harassmana = Config.Item("harassmana").GetValue<Slider>().Value;
            if (target == null || !target.IsValidTarget())
                return;

            if (Config.Item("hQ").GetValue<bool>() && target.IsValidTarget(Q.Range) &&
                player.ManaPercent >= harassmana)
            {
                Q.CastIfHitchanceEquals(target, HitChance.Dashing, true);
                Q.CastIfHitchanceEquals(target, HitChance.Immobile, true);
                var qpred = Q.GetPrediction(target);
                if (qpred.Hitchance >= HitChance.VeryHigh &&
                    qpred.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 2)
                {
                    Q.Cast(qpred.CastPosition);
                }
            }

            if (W.IsReady() && target.IsValidTarget(W.Range) && player.ManaPercent >= harassmana &&
               Config.Item("hW").GetValue<bool>())
                W.CastOnUnit(target);

            if (E.IsReady() && player.ManaPercent >= harassmana &&
               Config.Item("hE").GetValue<bool>())
                E.CastOnUnit(target);
        }

        private static void Clear()
        {
            var minionObj = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly,
                MinionOrderTypes.MaxHealth);
            var lanemana = Config.Item("lanemana").GetValue<Slider>().Value;
            var minions = MinionManager.GetMinions(player.Position, E.Range, MinionTypes.All, MinionTeam.Enemy,
                   MinionOrderTypes.MaxHealth);

            if (!minionObj.Any())
            {
                return;
            }

            if (player.ManaPercent >= lanemana && Q.IsReady() && minionObj.Count >= 1)
            {
                if (TearoftheGoddess.IsOwned(player) || TearoftheGoddessCrystalScar.IsOwned(player) || ArchangelsStaff.IsOwned(player) || ArchangelsStaffCrystalScar.IsOwned(player) || Manamune.IsOwned(player) || ManamuneCrystalScar.IsOwned(player) && Config.Item("laneQ").GetValue<StringList>().SelectedIndex != 0)
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                    {
                        var minionsss = minionObj[2];
                        Q.Cast(minionsss);
                    }
            }

            if (player.ManaPercent >= lanemana && Q.IsReady() && minionObj.Count >= 1)
            {
                if (Config.Item("laneQ").GetValue<StringList>().SelectedIndex == 1)
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                    {
                        var minionsss = minionObj[2];
                        Q.Cast(minionsss);
                    }
            }

            if (E.IsReady() && Config.Item("laneE").GetValue<bool>() && player.ManaPercent >= lanemana && minionObj.Count >= 3)
            {
                var emino = Config.Item("emin").GetValue<Slider>().Value;
                var allMinionsE = MinionManager.GetMinions(player.Position, E.Range, MinionTypes.All, MinionTeam.Enemy);

                if (minions.Any())
                {
                    var farmAll = E.GetCircularFarmLocation(minions, E.Range);
                    if (farmAll.MinionsHit >= emino)
                    {
                        var inions = minionObj.OrderBy(minion => minion.Distance(farmAll.Position)).FirstOrDefault();
                        E.CastOnUnit(inions, true);
                        return;
                    }
                }

                if (player.ManaPercent >= lanemana)
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear &&
                        Config.Item("laneW").GetValue<bool>() && minionObj.Count >= 2)
                    {
                        var minionss = minionObj[1];
                        {
                            W.CastOnUnit(minionss);
                        }
                    }
            }

        }

        private static void OnDraw(EventArgs args)
        {
            var Target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;

            if (Config.Item("Qdraw").GetValue<bool>())
                Render.Circle.DrawCircle(player.Position, Q.Range, System.Drawing.Color.White, 3);
            if (Config.Item("Wdraw").GetValue<bool>())
                Render.Circle.DrawCircle(player.Position, W.Range, System.Drawing.Color.White, 3);
            if (Config.Item("Edraw").GetValue<bool>())
                Render.Circle.DrawCircle(player.Position, E.Range, System.Drawing.Color.White, 3);

            if (Config.Item("combodamage").GetValue<bool>() && Target != null && Q.IsInRange(Target))
            {
                float[] Positions = GetLength();
                Drawing.DrawLine
                    (
                        new Vector2(Target.HPBarPosition.X + 10 + Positions[0] * 104, Target.HPBarPosition.Y + 20),
                        new Vector2(Target.HPBarPosition.X + 10 + Positions[1] * 104, Target.HPBarPosition.Y + 20),
                        9,
                        Color.Orange
                    );
            }
            if (Config.SubMenu("Draw").Item("qpred").GetValue<bool>() && !player.IsDead)
            {
                if (currentTarget != null && player.Distance(currentTarget) < Q.Range + 200)
                {
                    var playerPos = Drawing.WorldToScreen(player.Position);
                    var targetPos = Drawing.WorldToScreen(currentTarget.Position);
                    Drawing.DrawLine(playerPos, targetPos, 4,
                        Q.GetPrediction(currentTarget, overrideRange: Q.Range).Hitchance < HitChance.High
                            ? Color.LightSlateGray
                            : Color.SpringGreen);
                }
            }
        }
    }
}
