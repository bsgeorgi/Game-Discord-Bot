using System.Collections.Generic;

namespace KopDiscordBot.Models
{
    public class MonsterInfo
    {
        public short ID { get; set; }
        public string Name { get; set; }
        public short Level { get; set; }
        public long HP { get; set; }
        public long SP { get; set; }
        public int MinAttack { get; set; }
        public int MaxAttack { get; set; }
        public int Defence { get; set; }
        public int HitRate { get; set; }
        public int Dodge { get; set; }
        public short PhysicalResistance { get; set; }
        public int AttackSpeed { get; set; }
        public int MovementSpeed { get; set; }
        public long Experience { get; set; }
        public Dictionary<short, double> DropDictionary { get; set; }
        public bool CorrectFind { get; set; }
    }
}