using LeagueSharp;
using SharpDX;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;
using System.Runtime.Remoting.Messaging;

namespace JustFlash
{
    internal class Program
    {
        private static Menu Config;
        private static readonly Obj_AI_Hero player = ObjectManager.Player;
        public const string Menuname = "JustFlash";
        public static readonly List<Obj_AI_Base> Attackers = new List<Obj_AI_Base>();
        private static SpellSlot flash = ObjectManager.Player.GetSpellSlot("SummonerFlash");

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            Notifications.AddNotification("JustFlash Loaded - [V.1.0.1.0]", 8000).SetTextColor(Color.GreenYellow);

            Config = new Menu(Menuname, Menuname, true);
            //Menu
            Config.AddSubMenu(new Menu("Flash Settings", "Flash Settings"));
            Config.SubMenu("Flash Settings").AddItem(new MenuItem("ignite", "For Ignite").SetValue(new KeyBind("J".ToCharArray()[0],KeyBindType.Toggle)));
            Config.SubMenu("Flash Settings").AddItem(new MenuItem("poison", "For Poison - Soon™"));
            Config.SubMenu("Flash Settings").AddItem(new MenuItem("author", "by Justy, LeagueSharp | © 2015"));
            Spellbook.OnCastSpell += OnCastSpell;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Config.AddToMainMenu();
        }

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            {
                if (sender.IsValid && args.Target.IsMe && args.SData.Name == "summonerdot")
                {
                    Attackers.Add(sender);
                    Utility.DelayAction.Add(5000, () => Attackers.Remove(Attackers.FirstOrDefault(a => a.NetworkId == sender.NetworkId)));
                }
            }
        }

        private static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if ((Config.Item("ignite").GetValue<KeyBind>().Active

                 &&
                 ObjectManager.Player.Spellbook.CanUseSpell(flash) == SpellState.Ready 
                 &&
                 args.Slot == flash)
                )

            {
                {
                    if (player.Health < IgniteDamage())
                        args.Process = false;
                }
            }
        }

        private static float IgniteDamage()
        {

            var igniteBuff =
                player.Buffs.Where(buff => buff.Name == "summonerdot")
                    .OrderBy(buff => buff.StartTime)
                    .FirstOrDefault();
            if (igniteBuff == null)
            {
                return 0;
            }
            else
            {
                var igniteDamage = Math.Floor(igniteBuff.EndTime - Game.ClockTime)*
                                   player.GetSummonerSpellDamage(Attackers[0], Damage.SummonerSpell.Ignite)/5;
                return (float) igniteDamage;
            }
        }
    }
}




