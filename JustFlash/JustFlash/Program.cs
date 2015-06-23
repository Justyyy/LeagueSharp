using LeagueSharp;
using SharpDX;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace JustFlash
{
    internal class Program
    {
        private static Menu Config;
        private static readonly Obj_AI_Hero player = ObjectManager.Player;
        public const string Menuname = "JustFlash";
        private static Obj_AI_Hero igniteuser;
        private static SpellSlot flash = ObjectManager.Player.GetSpellSlot("SummonerFlash");

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            Notifications.AddNotification("JustFlash Loaded - [V.1.0.0.0]", 8000);

            Config = new Menu(Menuname, Menuname, true);
            //Menu
            Config.AddSubMenu(new Menu("Flash Settings", "Flash Settings"));
            Config.SubMenu("settings").AddItem(new MenuItem("block", "Block Flash").SetValue(true));
            Config.SubMenu("settings").AddItem(new MenuItem("ignite", "For Ignite").SetValue(true));
            Config.SubMenu("settings").AddItem(new MenuItem("poison", "For Poison - SOON™"));
            Spellbook.OnCastSpell += OnCastSpell;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Config.AddToMainMenu();
        }

       private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                Notifications.AddNotification(sender.Name + " " + args.SData.Name, 2000);
            }

            if (args.SData.Name == "summonerdot" && sender.IsEnemy)
            {
                sender = igniteuser;
            }
        }

        private static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (Config.Item("block").GetValue<bool>() && (Config.Item("ignite").GetValue<bool>() && (ObjectManager.Player.Spellbook.CanUseSpell(flash) == SpellState.Ready) 
                && args.Slot == ObjectManager.Player.GetSpellSlot("SummonerFlash")))
            {
                if (player.Health < IgniteDamage())
                {
                    args.Process = false;
                }
            }
        }

        private static float IgniteDamage()
        {
            double ignite = 0;
            double hp = player.HPRegenRate;
            var ignitedamage = (90 + (20*igniteuser.Level) - 20);
            var tick = ignitedamage/5;
            var durationstart = player.Buffs.Find(buff => buff.Name == "summonerdot").StartTime;
            var durationstop = player.Buffs.Find(buff => buff.Name == "summonerdot").EndTime;
            var buffduration = Game.Time - durationstart;
            var exactdmg = tick*buffduration;
            var regen = (hp / 5) * buffduration;
           
            ignite += exactdmg;
            Console.WriteLine("Damage :" + ignitedamage);
            return (float) ignite;
        }
    }
}


    

