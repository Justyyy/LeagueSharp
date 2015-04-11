using System;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Printing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using Color = System.Drawing.Color;
using JustMalphite;

namespace JustMalphite
{
    internal class Program
    {
        public const string ChampName = "Malphite";
        public static HpBarIndicator Hpi = new HpBarIndicator();
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell Smite;

        //Credits to Kurisu for Smite Stuff :^)
        public static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        public static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        public static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        public static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };
        private static SpellSlot Ignite;
        private static SpellSlot smiteSlot;
        private static readonly Obj_AI_Hero player = ObjectManager.Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;

        }

        private static void OnLoad(EventArgs args)
        {
            if (player.ChampionName != ChampName)
                return;

            Notifications.AddNotification("JustMalphite - [V.1.0.2.0]", 8000);

            //Ability Information - Range - Variables.
            Q = new Spell(SpellSlot.Q, 625);
            W = new Spell(SpellSlot.W, 125);
            E = new Spell(SpellSlot.E, 375);
            R = new Spell(SpellSlot.R, 1000);
            R.SetSkillshot(0.00f, 270, 700, false, SkillshotType.SkillshotCircle);


            Config = new Menu(player.ChampionName, player.ChampionName, true);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Combo menu:
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("manualr", "Cast R Manual").SetValue(new KeyBind('R', KeyBindType.Press)));
            Config.SubMenu("Combo").AddItem(new MenuItem("RHit", "Cast R If Hit").SetValue(new Slider(2, 1, 5)));

            //Mana Manager
            Config.AddSubMenu(new Menu("Mana", "Mana"));
            Config.SubMenu("Mana").AddItem(new MenuItem("qmana", "[Q] Mana %").SetValue(new Slider(10, 100, 0)));
            Config.SubMenu("Mana").AddItem(new MenuItem("wmana", "[W] Mana %").SetValue(new Slider(10, 100, 0)));
            Config.SubMenu("Mana").AddItem(new MenuItem("emana", "[E] Mana %").SetValue(new Slider(10, 100, 0)));
            Config.SubMenu("Mana").AddItem(new MenuItem("rmana", "[R] Mana %").SetValue(new Slider(10, 100, 0)));

            //Harass menu:
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("hQ", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("hW", "Use W").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("hE", "Use E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("harassmana", "Mana Percentage").SetValue(new Slider(30, 100, 0)));
            Config.SubMenu("Harass").AddItem(new MenuItem("AutoHarass", "Auto Harass", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
            Config.SubMenu("Harass").AddItem(new MenuItem("AutoHarass.Q", "Use Q for Auto").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("AutoHarass.E", "Use E for Auto").SetValue(true));

            //Farming menu:
            Config.AddSubMenu(new Menu("Clear", "Clear"));
            Config.SubMenu("Clear").AddItem(new MenuItem("cQ", "Use Q").SetValue(false));
            Config.SubMenu("Clear").AddItem(new MenuItem("cW", "Use W").SetValue(false));
            Config.SubMenu("Clear").AddItem(new MenuItem("cE", "Use E").SetValue(false));
            Config.SubMenu("Clear").AddItem(new MenuItem("clearmana", "Mana Percentage").SetValue(new Slider(30, 100, 0)));

            //KS menu:
            Config.AddSubMenu(new Menu("KS", "KS"));
            Config.SubMenu("KS").AddItem(new MenuItem("ksQ", "Use Q For KS").SetValue(true));
            Config.SubMenu("KS").AddItem(new MenuItem("ksR", "Use R For KS").SetValue(false));

            //Misc Menu:
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("interrupt", "Use R To Interrupt").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("tower", "Auto R Under Tower").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("antigap", "Anti Gapcloser with Q")).SetValue(true);
            Config.SubMenu("Misc").AddItem(new MenuItem("DrawD", "Damage Indicator").SetValue(true));

            //Drawings menu:
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Draw_Disabled", "Disable All Spell Drawings").SetValue(false));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Qdraw", "Draw Q Range").SetValue(new Circle(true, Color.Orange)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Wdraw", "Draw W Range").SetValue(new Circle(true, Color.Red)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Edraw", "Draw E Range").SetValue(new Circle(true, Color.Blue)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Rdraw", "Draw R Range").SetValue(new Circle(true, Color.AntiqueWhite)));

            Config.AddToMainMenu();
            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnEndScene += OnEndScene;
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (R.IsReady() && sender.IsValidTarget(R.Range) && Config.Item("interrupt").GetValue<bool>())
                R.CastIfHitchanceEquals(sender, HitChance.High);
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Q.IsReady() && gapcloser.Sender.IsValidTarget(Q.Range) && Config.Item("antigap").GetValue<bool>())
                Q.CastOnUnit(gapcloser.Sender);
        }


        public static string GetSmiteType()
        {
            if (SmiteBlue.Any(id => Items.HasItem(id)))
            {
                return "s5_summonersmiteplayerganker";
            }
            if (SmiteRed.Any(id => Items.HasItem(id)))
            {
                return "s5_summonersmiteduel";
            }
            if (SmiteGrey.Any(id => Items.HasItem(id)))
            {
                return "s5_summonersmitequick";
            }
            if (SmitePurple.Any(id => Items.HasItem(id)))
            {
                return "itemsmiteaoe";
            }
            return "summonersmite";
        }

        private static void OnEndScene(EventArgs args)
        {
            if (Config.SubMenu("Misc").Item("DrawD").GetValue<bool>())
            {
                foreach (var enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
                {
                    Hpi.unit = enemy;
                    Hpi.drawDmg(CalcDamage(enemy), Color.Green);
                }
            }
        }

        private static void combo()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;

            var qmana = Config.Item("qmana").GetValue<Slider>().Value;
            if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>() && target.IsValidTarget(Q.Range) && player.ManaPercent >= qmana)
                Q.Cast(target);

            var wmana = Config.Item("wmana").GetValue<Slider>().Value;
            if (W.IsReady() && player.ManaPercent >= wmana && target.IsValidTarget(W.Range) && Config.Item("UseW").GetValue<bool>())
                W.Cast();

            var emana = Config.Item("emana").GetValue<Slider>().Value;
            if (E.IsReady() && target.IsValidTarget(E.Range) && player.ManaPercent >= emana && Config.Item("UseE").GetValue<bool>())
                E.Cast();

            var countEnemies = Config.Item("Rhit").GetValue<Slider>().Value;
            var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            var rmana = Config.Item("rmana").GetValue<Slider>().Value;
            if (R.IsReady() && Config.Item("UseR").GetValue<bool>() && target.IsValidTarget(R.Range) && rTarget != null && player.ManaPercent >= rmana)
            {
                var pred = R.GetPrediction(target).Hitchance;
                if (pred >= HitChance.High)
                R.CastIfWillHit(rTarget, countEnemies);
            }

            if (Config.Item("manualr").GetValue<KeyBind>().Active && rTarget != null && R.IsReady())
                R.Cast(rTarget);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                items();
        }

        private static int CalcDamage(Obj_AI_Base target)
        {
            var aa = player.GetAutoAttackDamage(target, true) * (1 + player.Crit);
            var damage = aa;
            Ignite = player.GetSpellSlot("summonerdot");

            if (Ignite.IsReady())
                damage += player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);

            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                damage += player.GetItemDamage(target, Damage.DamageItems.Botrk); //ITEM BOTRK

            if (Items.HasItem(3144) && Items.CanUseItem(3144))
                damage += player.GetItemDamage(target, Damage.DamageItems.Bilgewater); //ITEM BOTRK

            if (R.IsReady() && Config.Item("UseR").GetValue<bool>()) // rdamage
            {
                if (R.IsReady())
                {
                    damage += R.GetDamage(target);
                }
            }

            if (Q.IsReady() && Config.Item("UseQ").GetValue<KeyBind>().Active) // qdamage
            {

                damage += Q.GetDamage(target);
            }

            if (W.IsReady() && Config.Item("UseW").GetValue<KeyBind>().Active) // wdamage
            {

                damage += E.GetDamage(target);
            }
            return (int)damage;
        }

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (Ignite == SpellSlot.Unknown || player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
                return 0f;
            return (float)player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        private static void UnderTower()
        {
            var Target = TargetSelector.GetTarget(R.Range + R.Width, TargetSelector.DamageType.Magical);

            if (Utility.UnderTurret(Target, false) && R.IsReady() && Config.Item("tower").GetValue<bool>())
            {
                var pred = R.GetPrediction(Target).Hitchance;
                if (pred >= HitChance.High)
                    R.Cast(Target);
            }
        }

        private static void Killsteal()
        {
            foreach (Obj_AI_Hero target in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(hero => hero.IsValidTarget(Q.Range) && !hero.HasBuffOfType(BuffType.Invulnerability) && hero.IsEnemy)
                )
            {
                var qDmg = player.GetSpellDamage(target, SpellSlot.Q);
                if (Config.Item("ksQ").GetValue<bool>() && target.IsValidTarget(Q.Range) && target.Health <= qDmg)
                {
                    Q.CastOnUnit(target);
                }
                var rDmg = player.GetSpellDamage(target, SpellSlot.R);
                if (Config.Item("ksR").GetValue<bool>() && target.IsValidTarget(R.Range) && target.Health <= rDmg)
                {
                    var pred = R.GetPrediction(target).Hitchance;
                    if (pred >= HitChance.High)
                        R.Cast(target);
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

            if (Ghost.IsReady() && Ghost.IsOwned(player) && target.IsValidTarget(E.Range)
                && Config.Item("useGhostblade").GetValue<bool>())

                Ghost.Cast();

            if (player.Distance(target.Position) <= 600 && IgniteDamage(target) >= target.Health &&
                Config.Item("UseIgnite").GetValue<bool>())
                player.Spellbook.CastSpell(Ignite, target);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

            Killsteal();

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Laneclear();
                    Jungleclear();
                    break;
            }

            var autoHarass = Config.Item("AutoHarass", true).GetValue<KeyBind>().Active;
            if (autoHarass)
                AutoHarass();

        }

        private static void AutoHarass()
        {
            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (qTarget == null || !qTarget.IsValid)
                return;

            var useQ = Config.Item("AutoHarass.Q").GetValue<bool>();
            var useE = Config.Item("AutoHarass.E").GetValue<bool>();
            var playerMana = Config.Item("harassmana").GetValue<Slider>().Value;

            if (player.Mana < playerMana)
                return;

            if (useQ && Q.IsReady() && Q.IsInRange(qTarget))
            {
                Q.Cast(qTarget);
            }

            if (useE && E.IsReady() && E.IsInRange(eTarget) && eTarget != null)
            {
                E.Cast();
            }
        }

        private static void harass()
        {
            var harassmana = Config.Item("harassmana").GetValue<Slider>().Value;
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;

            if (Q.IsReady()
                && Config.Item("hQ").GetValue<bool>()
                && target.IsValidTarget(Q.Range)
                && player.ManaPercent >= harassmana)

                Q.CastOnUnit(target);

            if (W.IsReady()
                && Config.Item("hW").GetValue<bool>()
                && target.IsValidTarget(W.Range)
                && player.ManaPercent >= harassmana)

                W.Cast();

            if (E.IsReady()
                && Config.Item("hE").GetValue<bool>()
                && target.IsValidTarget(E.Range)
                && player.ManaPercent >= harassmana)

                E.Cast(target);
        }

        private static void Laneclear()
        {
            var lanemana = Config.Item("laneclearmana").GetValue<Slider>().Value;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                && Config.Item("cQ").GetValue<bool>()
                && player.ManaPercent >= lanemana)

                Q.CastOnUnit(minion);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
               && Config.Item("cW").GetValue<bool>()
               && player.ManaPercent >= lanemana)

                W.Cast(player);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                && Config.Item("cE").GetValue<bool>()
                && player.ManaPercent >= lanemana)

                E.Cast();
        }


        private static void Jungleclear()
        {
            var jlanemana = Config.Item("clearmana").GetValue<Slider>().Value;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                && Config.Item("cW").GetValue<bool>()
                && player.ManaPercent >= jlanemana)

                W.Cast(player);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                && Config.Item("cE").GetValue<bool>()
                && player.ManaPercent >= jlanemana)

                E.Cast();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                && Config.Item("cQ").GetValue<bool>()
                && player.ManaPercent >= jlanemana)

                Q.CastOnUnit(minion);
        }

        private static void OnDraw(EventArgs args)
        {
            {

            }

            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;

            if (Config.Item("Qdraw").GetValue<Circle>().Active)
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Config.Item("Qdraw").GetValue<Circle>().Color : Color.Red);

            if (Config.Item("Edraw").GetValue<Circle>().Active)
                if (E.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range - 1,
                        E.IsReady() ? Config.Item("Edraw").GetValue<Circle>().Color : Color.Red);

            if (Config.Item("Wdraw").GetValue<Circle>().Active)
                if (E.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range - 1,
                        E.IsReady() ? Config.Item("Wdraw").GetValue<Circle>().Color : Color.Red);

            if (Config.Item("Rdraw").GetValue<Circle>().Active)
                if (R.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range,
                        R.IsReady() ? Config.Item("Rdraw").GetValue<Circle>().Color : Color.Red);

            var orbtarget = Orbwalker.GetTarget();
            Render.Circle.DrawCircle(orbtarget.Position, 100, Color.DarkOrange, 10);
        }



        public static void UseSmiteOnChamp(Obj_AI_Hero target)
        {
            if (target.IsValidTarget(E.Range) && smiteSlot != SpellSlot.Unknown &&
                ObjectManager.Player.Spellbook.CanUseSpell((smiteSlot)) == SpellState.Ready &&
                (GetSmiteType() == "s5_summonersmiteplayerganker" ||
                 GetSmiteType() == "s5_summonersmiteduel"))
            {
                ObjectManager.Player.Spellbook.CastSpell(smiteSlot, target);
            }
        }

        public static void GetSmiteSlot()
        {
            foreach (
                var spell in
                    ObjectManager.Player.Spellbook.Spells.Where(
                        spell => String.Equals(spell.Name, GetSmiteType(), StringComparison.CurrentCultureIgnoreCase)))
            {
                smiteSlot = spell.Slot;
                Smite = new Spell(smiteSlot, 700);
                return;
            }
        }

        public static Obj_AI_Base minion { get; set; }
    }
}
