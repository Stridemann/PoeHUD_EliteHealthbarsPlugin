using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpDX;
using SharpDX.Direct3D9;
using PoeHUD.Hud.Menu;
using PoeHUD.Hud.Settings;
using PoeHUD.Models.Enums;
using PoeHUD.Plugins;
using PoeHUD.Poe;
using PoeHUD.Models;
using PoeHUD.Poe.Components;
using PoeHUD.Poe.Elements;
using PoeHUD.Poe.EntityComponents;
using PoeHUD.Poe.RemoteMemoryObjects;


namespace EliteHealthbarPlugin
{
    public class EliteHealthbarsPlugin : BaseSettingsPlugin<Settings>
    {
        public EliteHealthbarsPlugin()
        {
            PluginName = "Elite Healthbars";
        }


        private List<EntityWrapper> MonstersCached = new List<EntityWrapper>();
        private List<MonsterPack> MonsterPacks = new List<MonsterPack>();

        public override void Initialise()
        {
            GameController.Area.OnAreaChange += area =>
            {
                MonstersCached.Clear();
                MonsterPacks.Clear();
            };
        }


        public override void EntityAdded(EntityWrapper entity)
        {
            if (GameController.Area.CurrentArea.IsTown || GameController.Area.CurrentArea.IsHideout) return;

            if (!Settings.Enable || entity == null || !entity.IsAlive) return;

            if (MonstersCached.Contains(entity) || !entity.HasComponent<Monster>() || !entity.IsHostile) return;
            var rarity = entity.GetComponent<ObjectMagicProperties>();
            if (rarity.Rarity == MonsterRarity.White) return;

            var entityGridPos = entity.GetComponent<Positioned>().GridPos;

            if (MonsterPacks.Count > 0)
            {
                var rarityFiltered = MonsterPacks.Where(x => x.Rarity == rarity.Rarity).ToList();

                if (rarityFiltered.Count > 0)
                {
                    var foundFroups = rarityFiltered.OrderBy(x => Vector2.Distance(x.PackCenter, entityGridPos)).ToArray();
                    var nearestGroup = foundFroups[0];

                    var dist = Vector2.Distance(nearestGroup.PackCenter, entityGridPos);
                    if (dist < Settings.GroupingDistance)
                    {
                        MonstersCached.Add(entity);
                        nearestGroup.Monsters.Add(entity);
                        nearestGroup.UpdatePackCenter();
                        return;
                    }
                }
            }

            string path = entity.Path;
            if (path.Contains('@'))
                path = path.Split('@')[0];

            var vCfg = GameController.Files.MonsterVarieties.TranslateFromMetadata(path);


            if (vCfg == null && path.Contains("/"))
                path = path.Substring(path.LastIndexOf("/") + 1);
            else
                path = vCfg.MonsterName;

            var newPack = new MonsterPack();
            newPack.Rarity = rarity.Rarity;
            newPack.PackName = path;
            newPack.PackCenter = entityGridPos;
            MonstersCached.Add(entity);
            newPack.Monsters.Add(entity);

            MonsterPacks.Add(newPack);
        }


        public override void EntityRemoved(EntityWrapper entity)
        {
            RemoveMonster(entity);
        }

        private void RemoveMonster(EntityWrapper entity)
        {
            MonstersCached.Remove(entity);

            foreach (var pack in MonsterPacks.ToArray())
            {
                pack.Monsters.Remove(entity);

                if (pack.Monsters.Count == 0)
                    MonsterPacks.Remove(pack);
            }
        }

        public override void Render()
        {
            if (GameController.Area.CurrentArea.IsTown || GameController.Area.CurrentArea.IsHideout) return;
            if (!Settings.Enable) return;

            var drawPosY = Settings.PosY.Value;

            foreach (var pack in MonsterPacks.ToArray())
            {
                var rarity = pack.Rarity;

                if (rarity == MonsterRarity.Magic && !Settings.ShowMagic.Value)
                    continue;

                var barColor = Settings.RareHpColor;

                if (rarity == MonsterRarity.Magic)
                    barColor = Settings.MagicHpColor;
                else if (rarity == MonsterRarity.Unique)
                    barColor = Settings.UniqueHpColor;
                

                Graphics.DrawText(pack.PackName, 15, new Vector2(Settings.PosX.Value, drawPosY));
                drawPosY += 20;

                var partialWidth = Settings.Width.Value / pack.Monsters.Count;
                var posX = Settings.PosX.Value;

                foreach (var minion in pack.Monsters.ToArray())
                {
                    var life = minion.GetComponent<Life>();

                    int totalHp = life.MaxES + life.MaxHP;
                    int curHp = life.CurES + life.CurHP;

                    if (curHp <= 0)
                        RemoveMonster(minion);

                    float perc = (float)curHp / totalHp;

                    if (rarity == MonsterRarity.Magic && Settings.GroupMagic.Value)
                    {
                        DrawProgressBar(drawPosY, barColor.Value, perc, partialWidth, posX);
                        posX += partialWidth;
                    }
                    else
                    {
                        DrawProgressBar(drawPosY, barColor.Value, perc, Settings.Width.Value, Settings.PosX.Value);

                        Graphics.DrawText(perc.ToString("p0"), 12, 
                            new Vector2(Settings.PosX.Value + Settings.Width.Value / 2, drawPosY + Settings.Height.Value / 2), 
                            FontDrawFlags.VerticalCenter | FontDrawFlags.Center);

                        drawPosY += Settings.Height.Value + Settings.Spacing.Value;
                    } 
                }
                drawPosY += 5;
            }
        }

        private void DrawProgressBar(int drawPosY, Color barColor, float perc, int width, int posX)
        {
            var mainRect = new RectangleF(posX, drawPosY, width, Settings.Height.Value);
            Graphics.DrawBox(mainRect, Settings.BGColor.Value);
            var frameRect = mainRect;
            mainRect.Width *= perc;
            Graphics.DrawBox(mainRect, barColor);
            Graphics.DrawFrame(frameRect, 1, Settings.BorderColor.Value);
        }

        private class MonsterPack
        {
            public string PackName;
            public List<EntityWrapper> Monsters = new List<EntityWrapper>();
            public Vector2 PackCenter;
            public MonsterRarity Rarity;

            public void UpdatePackCenter()
            {
                Vector2 PackCenter = Vector2.Zero;
                foreach (var monster in Monsters)
                    PackCenter += monster.GetComponent<Positioned>().GridPos;
                PackCenter /= Monsters.Count;
            }
        }
    }
}
