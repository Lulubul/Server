using System.Collections.Generic;

namespace NetworkTypes
{
    public class Round
    {
        private readonly Dictionary<bool, LinkedList<AbstractCreature>> _creaturesDictionary = new Dictionary<bool, LinkedList<AbstractCreature>>();
        private readonly Turn _currentTurn;
        private readonly Dictionary<bool, Turn> _turns = new Dictionary<bool, Turn>();
        private bool _isFirstHero = true;

        public Round(AbstractHero h1, AbstractHero h2)
        {
            _creaturesDictionary.Add(true, new LinkedList<AbstractCreature>(h1.Creatures));
            _creaturesDictionary.Add(false, new LinkedList<AbstractCreature>(h2.Creatures));
            _turns.Add(true, new Turn(true, _creaturesDictionary[true].First));
            _turns.Add(false, new Turn(false, _creaturesDictionary[false].First));
        }

        public AbstractCreature NextCreature()
        {
            _isFirstHero = !_isFirstHero;
            while (_turns[_isFirstHero].Creature.NextOrFirst().Value.Status == CreatureStatus.Death)
            {
                _turns[_isFirstHero].Creature = _turns[_isFirstHero].Creature.NextOrFirst();
            }
            return _turns[_isFirstHero].Creature.Value;
        }
    }

    static class CircularLinkedList
    {
        public static LinkedListNode<AbstractCreature> NextOrFirst(this LinkedListNode<AbstractCreature> current)
        {
            return current.Next ?? current.List.First;
        }

        public static LinkedListNode<AbstractCreature> PreviousOrLast(this LinkedListNode<AbstractCreature> current)
        {
            return current.Previous ?? current.List.Last;
        }
    }
}
