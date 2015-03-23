using System;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Printing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using Color = System.Drawing.Color;
using JustOlaf;

namespace JustOlaf
{
    internal class Program
    {
        public const string ChampName = "Olaf";
        public static HpBarIndicator Hpi = new HpBarIndicator();
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell Smite;
        public static int SpellRangeTick;
        //Credits to Kurisu for Smite Stuff :^)
        public static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        public static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        public static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        public static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };

        private static SpellSlot Ignite;
        private static SpellSlot smiteSlot;
        private static int LastCast;
        private static readonly Obj_AI_Hero player = ObjectManager.Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;

        }

        private static void OnLoad(EventArgs args)
        {
            if (player.ChampionName != ChampName)
                return;

            Notifications.AddNotification("JustOlaf - [V.1.0.0.0]", 8000);

            Killsteal();

            //Ability Information - Range - Variables.
            Q = new Spell(SpellSlot.Q, 1000);
            Q.SetSkillshot(0.25f, 75f, 1500f, false, SkillshotType.SkillshotLine);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R);


            Config = new Menu("JustOlaf", "Olaf", true);
            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("[JO]: Orbwalker", "Orbwalker")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("[JO]: Target Selector", "Target Selector")));

            //COMBOMENU

            var combo = Config.AddSubMenu(new Menu("[JO]: Combo Settings", "Combo Settings"));
            var harass = Config.AddSubMenu(new Menu("JO]: Harass Settings", "Harass Settings"));
            var drawing = Config.AddSubMenu(new Menu("[JO]: Draw Settings", "Draw Settings"));

            combo.SubMenu("[SBTW] ManaManager").AddItem(new MenuItem("qmana", "[Q] Mana %").SetValue(new Slider(10, 100, 0)));
            combo.SubMenu("[SBTW] ManaManager").AddItem(new MenuItem("wmana", "[W] Mana %").SetValue(new Slider(10, 100, 0)));
            //combo.SubMenu("[SBTW] ManaManager").AddItem(new MenuItem("emana", "[E] Mana %").SetValue(new Slider(10, 100, 0)));
            combo.SubMenu("[SBTW] ManaManager").AddItem(new MenuItem("rmana", "[R] Mana %").SetValue(new Slider(15, 100, 0)));

            combo.SubMenu("[Q] Settings").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            combo.SubMenu("[Q] Settings").AddItem(new MenuItem("qr", "Mininum Distance to Q")).SetValue(new Slider(550, 0, (int)Q.Range));
            combo.SubMenu("[Q] Settings").AddItem(new MenuItem("qr2", "Maximum Distance to Q")).SetValue(new Slider((int)Q.Range, 0, (int)Q.Range));
            combo.SubMenu("[W] Settings").AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            combo.SubMenu("[E] Settings").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            combo.SubMenu("[R] Settings").AddItem(new MenuItem("UseR", "Use R (TOGGLE) ").SetValue(new KeyBind('K', KeyBindType.Toggle)));
            combo.SubMenu("Smite Settings").AddItem(new MenuItem("useSmiteCombo", "Use Smite On Combo")).SetValue(true);
            //combo.SubMenu("[R] Settings").AddItem(new MenuItem("manualr", "Cast R Manual").SetValue(new KeyBind('R', KeyBindType.Press)));


            combo.SubMenu("Item Settings")
                .AddItem(new MenuItem("useGhostblade", "Use Youmuu's Ghostblade").SetValue(true));
            combo.SubMenu("Item Settings")
                .AddItem(new MenuItem("UseBOTRK", "Use Blade of the Ruined King").SetValue(true));
            combo.SubMenu("Item Settings")
                .AddItem(new MenuItem("eL", "  Enemy HP Percentage").SetValue(new Slider(80, 100, 0)));
            combo.SubMenu("Item Settings")
                .AddItem(new MenuItem("oL", "  Own HP Percentage").SetValue(new Slider(65, 100, 0)));
            combo.SubMenu("Item Settings").AddItem(new MenuItem("UseBilge", "Use Bilgewater Cutlass").SetValue(true));
            combo.SubMenu("Item Settings")
                .AddItem(new MenuItem("HLe", "  Enemy HP Percentage").SetValue(new Slider(80, 100, 0)));
            combo.SubMenu("Summoner Settings").AddItem(new MenuItem("UseIgnite", "Use Ignite").SetValue(true));

            //LANECLEARMENU
            Config.SubMenu("[JO]: Laneclear Settings")
                .AddItem(new MenuItem("laneQ", "Use Q").SetValue(true));
            Config.SubMenu("[JO]: Laneclear Settings")
                .AddItem(new MenuItem("laneW", "Use w").SetValue(true));
            Config.SubMenu("[JO]: Laneclear Settings")
                .AddItem(new MenuItem("laneE", "Use E").SetValue(true));
            Config.SubMenu("[JO]: Laneclear Settings")
                .AddItem(new MenuItem("laneclearmana", "Mana Percentage").SetValue(new Slider(30, 100, 0)));

            //JUNGLEFARMMENU
            Config.SubMenu("[JO]: Jungle Settings")
                .AddItem(new MenuItem("jungleQ", "Use Q").SetValue(true));
            Config.SubMenu("[JO]: Jungle Settings")
                .AddItem(new MenuItem("jungleW", "Use W").SetValue(true));
            Config.SubMenu("[JO]: Jungle Settings")
                .AddItem(new MenuItem("jungleE", "Use E").SetValue(true));
            Config.SubMenu("[JO]: Jungle Settings")
                .AddItem(new MenuItem("jungleclearmana", "Mana Percentage").SetValue(new Slider(30, 100, 0)));

            //KSMENU
            Config.SubMenu("[JO]: Killsteal Settings").AddItem(new MenuItem("ksQ", "Use Q For KS").SetValue(true));
            Config.SubMenu("[JO]: Killsteal Settings").AddItem(new MenuItem("ksE", "Use E For KS").SetValue(true));

            drawing.AddItem(new MenuItem("Draw_Disabled", "Disable All Spell Drawings").SetValue(false));
            drawing.AddItem(new MenuItem("Qdraw", "Draw Q Range").SetValue(new Circle(true, Color.Orange)));
            drawing.AddItem(new MenuItem("Edraw", "Draw E Range").SetValue(new Circle(true, Color.AntiqueWhite)));

            harass.AddItem(new MenuItem("harassQ", "Use Q").SetValue(true));
            harass.AddItem(new MenuItem("harassW", "Use W").SetValue(true));
            harass.AddItem(new MenuItem("harassE", "Use E").SetValue(true));
            harass.AddItem(new MenuItem("harassmana", "Mana Percentage").SetValue(new Slider(30, 100, 0)));

            Config.SubMenu("[JO]: Misc Settings").AddItem(new MenuItem("DrawD", "Damage Indicator").SetValue(true));
            Config.SubMenu("[JO]: Misc Settings").AddItem(new MenuItem("hitQ", "Q Hitchance")).SetValue(new Slider(3, 1, 4));
            
            Config.AddToMainMenu();
            
            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnEndScene += OnEndScene;


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


        private static void OnEndScene(EventArgs args)
        {
            if (Config.SubMenu("[JO]: Misc Settings").Item("DrawD").GetValue<bool>())
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
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValidTarget()) //if there is no target or target isn't valid it will return; (It won't combo)
                return;

            if (Config.Item("useSmiteCombo").GetValue<bool>())
            {
                Smite.Cast(target);
            }

            var qmana = Config.Item("qmana").GetValue<Slider>().Value;

            if (Q.IsReady() && player.ManaPercentage() >= qmana)
            {
                PredictionOutput Qpredict = Q.GetPrediction(target);
                var hithere = Qpredict.CastPosition.Extend(ObjectManager.Player.Position, -100);
                if (player.Distance(target.ServerPosition) >= 350)
                    if (target.Distance(player.ServerPosition) > Config.Item("qr").GetValue<Slider>().Value)
                        Q.Cast(hithere);
            }
            else
            {
                var pred = Q.GetPrediction(target);
                if (pred.Hitchance >= (HitChance)Config.Item("hitQ").GetValue<Slider>().Value + 1)
                    Q.Cast(pred.CastPosition);
            }
         
            if (E.IsReady() && Config.Item("UseE").GetValue<bool>() && target.IsValidTarget(E.Range))
                E.CastOnUnit(target);

            var wmana = Config.Item("wmana").GetValue<Slider>().Value;

            if (W.IsReady() && target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(player)) && Config.Item("UseW").GetValue<bool>())
                W.Cast();

            //W doesn't have range, because W enhances olaf's attacks you might wanna use it in basicattack range
            //Orbwalking.GetRealAutoAttackRange(player)

            if (R.IsReady() && target.IsValidTarget(R.Range) && Config.Item("UseR").GetValue<bool>())
                R.Cast();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                items();


        }

        private static int CalcDamage(Obj_AI_Base target)
        {
            //The only dmg spells olaf has are E and Q (Added those and removed R/W)
            var aa = player.GetAutoAttackDamage(target, true) * (1 + player.Crit);
            var damage = aa;
            Ignite = player.GetSpellSlot("summonerdot");

            if (Ignite.IsReady())
                damage += player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite); //ignitedmg

            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                damage += player.GetItemDamage(target, Damage.DamageItems.Botrk); //ITEM BOTRK

            if (Items.HasItem(3144) && Items.CanUseItem(3144))
                damage += player.GetItemDamage(target, Damage.DamageItems.Bilgewater); //ITEM BOTRK

            if (Config.Item("UseE").GetValue<bool>()) // edamage
            {
                if (E.IsReady())
                {
                    damage += E.GetDamage(target);
                }
            }

            if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>()) // qdamage
            {

                damage += Q.GetDamage(target);
            }
            return (int)damage;
        }

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (Ignite == SpellSlot.Unknown || player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
                return 0f;
            return (float)player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        private static void Killsteal()
        {
            {
                foreach (var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsValidTarget(Q.Range))
                        .Where(x => !x.IsZombie)
                        .Where(x => !x.IsDead))
                {
                    var qDmg = Q.GetDamage(enemy);
                    var eDmg = E.GetDamage(enemy);

                    if (Config.Item("ksQ").GetValue<bool>() && enemy.Health <= qDmg)
                    if (Q.IsReady())
                        {
                            PredictionOutput Qpredict = Q.GetPrediction(enemy);
                            var hithere = Qpredict.CastPosition.Extend(ObjectManager.Player.Position, -100);
                            if (player.Distance(enemy.ServerPosition) >= 350)
                                if (enemy.Distance(player.ServerPosition) > Config.Item("qr").GetValue<Slider>().Value)
                                    Q.Cast(hithere);
                        }
                        else
                        {
                            var pred = Q.GetPrediction(enemy);
                            if (pred.Hitchance >= (HitChance)Config.Item("hitQ").GetValue<Slider>().Value + 1)
                                Q.Cast(pred.CastPosition);
                        }

                    if (Config.Item("ksE").GetValue<bool>() && enemy.IsValidTarget(E.Range) && enemy.Health <= eDmg)
                    {
                        E.Cast(enemy);
                    }
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
            && target.HealthPercentage() <= Config.Item("eL").GetValue<Slider>().Value
            && Config.Item("UseBOTRK").GetValue<bool>())

                botrk.Cast(target);

            if (botrk.IsReady() && botrk.IsOwned(player) && botrk.IsInRange(target)
                && player.HealthPercentage() <= Config.Item("oL").GetValue<Slider>().Value
                && Config.Item("UseBOTRK").GetValue<bool>())

                botrk.Cast(target);

            if (cutlass.IsReady() && cutlass.IsOwned(player) && cutlass.IsInRange(target) &&
                target.HealthPercentage() <= Config.Item("HLe").GetValue<Slider>().Value
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
        }

        private static void harass()
        {
            var harassmana = Config.Item("harassmana").GetValue<Slider>().Value;
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;

            if (Q.IsReady()
                && Config.Item("hQ").GetValue<bool>() && player.ManaPercentage() >= harassmana)
                {
                    PredictionOutput Qpredict = Q.GetPrediction(target);
                    var hithere = Qpredict.CastPosition.Extend(ObjectManager.Player.Position, -100);
                    if (player.Distance(target.ServerPosition) >= 350)
                        if (target.Distance(player.ServerPosition) > Config.Item("qr").GetValue<Slider>().Value)
                            Q.Cast(hithere);
                }
                else
                {
                    var pred = Q.GetPrediction(target);
                    if (pred.Hitchance >= (HitChance)Config.Item("hitQ").GetValue<Slider>().Value + 1)
                        Q.Cast(pred.CastPosition);
                }
               
            if (W.IsReady()
                && Config.Item("hW").GetValue<bool>()
                && player.ManaPercentage() >= harassmana && target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(player)))

                W.Cast();

            if (E.IsReady()
                && Config.Item("hE").GetValue<bool>()
                && target.IsValidTarget(E.Range)
                && player.ManaPercentage() >= harassmana)

                E.CastOnUnit(target);
        }

        private static void Laneclear()
        {
            var lanemana = Config.Item("laneclearmana").GetValue<Slider>().Value;
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width);

            var Qfarmpos = Q.GetLineFarmLocation(allMinionsQ, Q.Width);


            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                && Qfarmpos.MinionsHit >= 2 && Config.Item("laneQ").GetValue<bool>()
                && player.ManaPercentage() >= lanemana)

                Q.Cast(Qfarmpos.Position);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
               && Config.Item("laneW").GetValue<bool>()
               && player.ManaPercentage() >= lanemana)

                W.Cast();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                && Config.Item("laneE").GetValue<bool>())
                E.CastOnUnit(minion);

        }


        private static void Jungleclear()
        {
            var jlanemana = Config.Item("jungleclearmana").GetValue<Slider>().Value;
            var MinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width,
                MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            var Qfarmpos = Q.GetLineFarmLocation(MinionsQ, Q.Width);


            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                && Config.Item("jungleW").GetValue<bool>()
                && player.ManaPercentage() >= jlanemana)

                W.Cast();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                && Qfarmpos.MinionsHit >= 1
                && Config.Item("jungleQ").GetValue<bool>()
                && player.ManaPercentage() >= jlanemana)

                Q.Cast(Qfarmpos.Position);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                && Config.Item("jungleE").GetValue<bool>())
                E.CastOnUnit(minion);

        }

        private static void OnDraw(EventArgs args)
        {
            {

            }

            //Draw If R is enabled
            var pos = Drawing.WorldToScreen(ObjectManager.Player.Position);

            if (Config.Item("UseR").GetValue<KeyBind>().Active)
                Drawing.DrawText(pos.X - 50, pos.Y + 50, Color.Gold, "[R] is Enabled!");


            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;

            if (Config.Item("Qdraw").GetValue<Circle>().Active)
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Config.Item("Qdraw").GetValue<Circle>().Color : Color.Red);

            if (Config.Item("Edraw").GetValue<Circle>().Active)
                if (E.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range - 1,
                        E.IsReady() ? Config.Item("Edraw").GetValue<Circle>().Color : Color.Red);

            var orbtarget = Orbwalker.GetTarget();
            Render.Circle.DrawCircle(orbtarget.Position, 100, Color.DarkOrange, 10);
        }



        public static Obj_AI_Base minion { get; set; }
    }
}
