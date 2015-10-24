using System.Collections.Generic;

namespace NetworkTypes
{
    public class Turn : SerializableType
    {
        public bool IsFirstHero { get; set; }
        public int CreatureIndex { get; set; }
        public LinkedListNode<AbstractCreature> Creature;

        public Turn(bool isFirstHero, LinkedListNode<AbstractCreature> creature)
        {
            IsFirstHero = isFirstHero;
            Creature = creature;
            CreatureIndex = creature.Value.Index;
        }
    }

}
