using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp.Common;
using LeagueSharp;
using SharpDX;


namespace JustShyvanaV2
{
    class Program
    {
        internal static Menu Menu;
        internal static Spell Q, W, E, R;
        internal static Orbwalking.Orbwalker Orbwalker;
        internal static Obj_AI_Hero Player => ObjectManager.Player;
        internal static HpBarIndicator BarIndicator = new HpBarIndicator();

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        internal static void OnGameLoad(EventArgs args)
        {
            try
            {
                if (ObjectManager.Player.ChampionName != "Shyvana")
                {
                    return;
                }

                Notifications.AddNotification("Justy's Shyvana - [V.1.0.0.0]", 8000);

                Q = new Spell(SpellSlot.Q);
                W = new Spell(SpellSlot.W, 350f);
                E = new Spell(SpellSlot.E, 925f);
                E.SetSkillshot(0.25f, 60f, 1700, false, SkillshotType.SkillshotLine);
                R = new Spell(SpellSlot.R, 1000f);
                R.SetSkillshot(0.25f, 150f, 1500, false, SkillshotType.SkillshotLine);

                Menu = new Menu("Justy's Shyvana", "shyvana", true);

                var orbmenu = new Menu("[JS] - Orbwalk", "orbwalk");
                Orbwalker = new Orbwalking.Orbwalker(orbmenu);
                Menu.AddSubMenu(orbmenu);

                var kmenu = new Menu("[JS] - Keys", "keymenu");
                kmenu.AddItem(new MenuItem("usecombo", "Combo [active]")).SetValue(new KeyBind(32, KeyBindType.Press));
                kmenu.AddItem(new MenuItem("useharass", "Harass [active]"))
                    .SetValue(new KeyBind('G', KeyBindType.Press));
                kmenu.AddItem(new MenuItem("useclear", "Clear [active]")).SetValue(new KeyBind('A', KeyBindType.Press));
                kmenu.AddItem(new MenuItem("useflee", "Flee [active]")).SetValue(new KeyBind('Z', KeyBindType.Press));
                Menu.AddSubMenu(kmenu);

                var cmenu = new Menu("[JS] - Combo", "cmenu");
                var emenu = new Menu("[JS] - Extra", "emenu");
                var smenu = new Menu("[JS] - Skills", "smenu");

                smenu.AddItem(new MenuItem("useqcombo", "Use Q")).SetValue(true);
                smenu.AddItem(new MenuItem("usewcombo", "Use W")).SetValue(true);
                smenu.AddItem(new MenuItem("useecombo", "Use E")).SetValue(true);
                smenu.AddItem(new MenuItem("usercombo", "Use R")).SetValue(true);

                emenu.AddItem(new MenuItem("rene", "Min Enemies for R").SetValue(new Slider(2, 1, 5)));
                emenu.AddItem(new MenuItem("KsQ", "Killsteal with Q").SetValue(false));
                emenu.AddItem(new MenuItem("KsE", "Killsteal with E").SetValue(false));
                cmenu.AddSubMenu(emenu);
                cmenu.AddSubMenu(smenu);

                Menu.AddSubMenu(cmenu);

                var hmenu = new Menu("[JS] - Harass", "hamenu");
                hmenu.AddItem(new MenuItem("useqharass", "Use Q")).SetValue(true);
                hmenu.AddItem(new MenuItem("usewharass", "Use W")).SetValue(true);
                hmenu.AddItem(new MenuItem("useeharass", "Use E")).SetValue(true);
                Menu.AddSubMenu(hmenu);

                var clmenu = new Menu("[JS] - Clear", "clmenu");
                clmenu.AddItem(new MenuItem("useqclear", "Use Q")).SetValue(true);
                clmenu.AddItem(new MenuItem("usewclear", "Use W")).SetValue(true);
                clmenu.AddItem(new MenuItem("useeclear", "Use E")).SetValue(true);
                Menu.AddSubMenu(clmenu);

                var fmenu = new Menu("[JS] - Flee", "fmenu");
                fmenu.AddItem(new MenuItem("useeflee", "Use W")).SetValue(true);
                Menu.AddSubMenu(fmenu);

                var exmenu = new Menu("[JS] - Interrupt", "exmenu");
                exmenu.AddItem(new MenuItem("interrupt", "Interrupt")).SetValue(false).ValueChanged +=
                    (sender, eventArgs) => eventArgs.Process = false;
                Menu.AddSubMenu(exmenu);

                var skmenu = new Menu("[JS] - Skins", "skmenu");
                var skinitem = new MenuItem("useskin", "Enabled");
                skmenu.AddItem(skinitem).SetValue(false);

                skinitem.ValueChanged += (sender, eventArgs) =>
                {
                    if (!eventArgs.GetNewValue<bool>())
                    {
                        ObjectManager.Player.SetSkin(ObjectManager.Player.CharData.BaseSkinName,
                            ObjectManager.Player.BaseSkinId);
                    }
                };

                skmenu.AddItem(new MenuItem("skinid", "Skin ID")).SetValue(new Slider(1, 0, 6));
                Menu.AddSubMenu(skmenu);

                var drmenu = new Menu("[JS] - Draw", "drmenu");
                drmenu.AddItem(new MenuItem("drawhpbarfill", "Draw HPBarFill")).SetValue(true);
                drmenu.AddItem(new MenuItem("drawe", "Draw E"))
                    .SetValue(new Circle(true, System.Drawing.Color.FromArgb(178, 34, 34)));
                drmenu.AddItem(new MenuItem("draww", "Draw W"))
                    .SetValue(new Circle(true, System.Drawing.Color.FromArgb(178, 34, 34)));
                drmenu.AddItem(new MenuItem("drawr", "Draw R"))
                    .SetValue(new Circle(true, System.Drawing.Color.FromArgb(178, 34, 34)));
                Menu.AddSubMenu(drmenu);

                Menu.AddToMainMenu();

                Game.OnUpdate += Game_OnUpdate;
                Drawing.OnDraw += Drawing_OnDraw;
                Drawing.OnEndScene += Drawing_OnEndScene;
                Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
                Orbwalking.AfterAttack += OnAfterAttack;

            }

            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (R.IsReady() && sender.IsValidTarget(R.Range) && Menu.Item("interrupt").GetValue<bool>())
                R.CastIfHitchanceEquals(sender, HitChance.Medium);
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (Menu.Item("drawhpbarfill").GetValue<bool>())
            {
                foreach (
                    var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(ene => ene.IsValidTarget() && !ene.IsZombie))
                {
                    var color = R.IsReady() && IsLethal(enemy)
                        ? new ColorBGRA(0, 255, 0, 90)
                        : new ColorBGRA(255, 255, 0, 90);

                    BarIndicator.unit = enemy;
                    BarIndicator.drawDmg((float) ComboDamage(enemy), color);
                }
            }
        }

        private static void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe || !unit.IsValid) return;

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (Menu.Item("useqcombo").GetValue<bool>() && target.IsValid<Obj_AI_Hero>())
                    {
                        Q.Cast();
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (Menu.Item("useqharass").GetValue<bool>() && target.IsValid<Obj_AI_Hero>())
                    {
                        Q.Cast();
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (Menu.Item("useqclear").GetValue<bool>() && target.IsValid<Obj_AI_Minion>())
                    {
                        Q.Cast();
                    }
                    break;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var eCircle = Menu.Item("drawe").GetValue<Circle>();
            if (eCircle.Active)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, eCircle.Color);
            }

            var wCircle = Menu.Item("draww").GetValue<Circle>();
            if (wCircle.Active && !Player.HasBuff("ShyvanaTransform"))
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, wCircle.Color);
            }

            if (wCircle.Active && Player.HasBuff("ShyvanaTransform"))
            {
                Render.Circle.DrawCircle(Player.Position, 350, wCircle.Color);
            }

            var rCircle = Menu.Item("drawr").GetValue<Circle>();
            if (rCircle.Active)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, rCircle.Color);
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            Killsteal();

            if (Menu.Item("useflee").GetValue<KeyBind>().Active)
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
                W.Cast();
            }

            if (Menu.Item("usecombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (Menu.Item("useclear").GetValue<KeyBind>().Active)
            {
                {
                    Clear();
                }
            }

            if (Menu.Item("useharass").GetValue<KeyBind>().Active)
            {
                {
                    Harass();
                }
            }
        }

        private static void Killsteal()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var target2 = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            {
                if (Menu.Item("KsQ").GetValue<bool>() && target.IsValidTarget(Q.Range + 1) && target.Health <= Qdmg(target))
                {
                    UseQ(target);
                }


                if (Menu.Item("KsE").GetValue<bool>() && target2.IsValidTarget(E.Range) && target.Health <= Edmg(target2))
                {
                    UseE(target2);
                }
            }
        }

        static void Combo()
        {
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (target.IsValidTarget() && !target.IsZombie)
            {
                if (Menu.Item("useqcombo").GetValue<bool>())
                    UseQ(target);

                if (Menu.Item("usewcombo").GetValue<bool>())
                    UseW(target);

                if (Menu.Item("useecombo").GetValue<bool>())
                    UseE(target);

                if (Menu.Item("usercombo").GetValue<bool>())
                    UseR(target);
            }
        }

        static void Harass()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (target.IsValidTarget() && !target.IsZombie)
            {
                if (Menu.Item("useqharass").GetValue<bool>() && Q.IsReady())
                    UseQ(target);

                if (Menu.Item("usewharass").GetValue<bool>() && W.IsReady())
                    UseW(target);

                if (Menu.Item("useeharass").GetValue<bool>() && E.IsReady())
                    UseE(target);
            }
        }

        private static void Clear()
        {
            var minions = MinionManager.GetMinions(Player.Position, W.Range,
                MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);

            foreach (var unit in minions)
            {
                if (Menu.Item("useqclear").GetValue<bool>() && Q.IsReady())
                {
                    UseQ(unit);
                }

                if (Menu.Item("usewclear").GetValue<bool>() && W.IsReady())
                {
                    UseW(unit);
                }

                if (Menu.Item("useeclear").GetValue<bool>() && E.IsReady())
                {
                    UseE(unit);
                }
            }
        }

        static void UseQ(Obj_AI_Base target)
        {
            if (Q.IsReady() && target.Distance(Player.ServerPosition) <= Q.Range)
            {
                if (Q.Cast())
                    Orbwalking.ResetAutoAttackTimer();
            }
        }

        static void UseW(Obj_AI_Base target)
        {
            if (W.IsReady() && target.Distance(Player.ServerPosition) <= W.Range && !Player.HasBuff("ShyvanaTransform"))
            {
                W.Cast();
            }

            if (W.IsReady() && target.Distance(Player.ServerPosition) <= 350 && Player.HasBuff("ShyvanaTransform"))
            {
                W.Cast();
            }
        }

        static void UseE(Obj_AI_Base target)
        {
            if (E.IsReady() && target.Distance(Player.ServerPosition) <= E.Range)
            {
                E.CastIfHitchanceEquals(target, HitChance.High);
            }
        }

        static void UseR(Obj_AI_Hero target)
        {
            var minr = Menu.Item("rene").GetValue<Slider>().Value;
            var enemys = target.CountEnemiesInRange(R.Range);
            if (minr <= enemys && R.IsReady() && target.Distance(Player.ServerPosition) <= R.Range)
            {
                R.CastIfHitchanceEquals(target, HitChance.Medium);
            }
        }

        private static bool IsLethal(Obj_AI_Base unit)
        {
            return ComboDamage(unit) / 1.65 >= unit.Health;
        }

        private static double ComboDamage(Obj_AI_Base unit)
        {
            if (unit == null)
                return 0d;

            return Qdmg(unit) + Wdmg(unit) +
                   Edmg(unit) + Rdmg(unit);
        }


        private static double Qdmg(Obj_AI_Base target)
        {
            double dmg = 0;

            if (Q.IsReady() && target != null)
            {

                dmg += Player.CalcDamage(target, Damage.DamageType.Physical, Player.GetAutoAttackDamage(target, true) +
                                                                             (new[] {0.2, 0.25, 0.30, 0.35, 0.40}[
                                                                                 Q.Level - 1]));

            }

            return dmg;
        }

        private static double Wdmg(Obj_AI_Base target)
        {
            double dmg = 0;

            if (W.IsReady() && target != null)
            {
                W.GetDamage(target);
            }

            return dmg;
        }

        private static double Edmg(Obj_AI_Base target)
        {
            double dmg = 0;

            if (E.IsReady() && target != null)
            {
                dmg += Player.CalcDamage(target, Damage.DamageType.Magical,
                (new[] {60, 100, 140, 180, 220}[E.Level - 1] +
                 (0.6 * Player.FlatMagicDamageMod)));
            }

            return dmg;
        }

        private static double Rdmg(Obj_AI_Base target)
        {
            double dmg = 0;
            if (R.IsReady() && target != null)
            {
                dmg += Player.CalcDamage(target, Damage.DamageType.Magical, (new[] {175, 300, 425}[R.Level - 1] +
                                                                             (0.8 * Player.FlatMagicDamageMod)));
            }

            return dmg;
        }
    }
}
