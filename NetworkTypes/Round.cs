using System.Collections.Generic;

namespace NetworkTypes
{
    public class Round
    {
        private readonly Dictionary<Team, LinkedListNode<AbstractCreature>> _creaturesDictionary = new Dictionary<Team, LinkedListNode<AbstractCreature>>();
        private readonly LinkedList<AbstractHero> _heroes = new LinkedList<AbstractHero>();
        private LinkedListNode<AbstractHero> _currentHero;
        private readonly LinkedList<AbstractCreature> blueCreatures;
        private readonly LinkedList<AbstractCreature> redCreatures;

        public Round(AbstractHero h1, AbstractHero h2)
        {
            _heroes.AddLast(h1);
            _heroes.AddLast(h2);
            _currentHero = _heroes.First;
            blueCreatures = new LinkedList<AbstractCreature>(h1.Creatures);
            redCreatures = new LinkedList<AbstractCreature>(h2.Creatures);
            var blueCreature = blueCreatures.First;
            var redCreature = redCreatures.First;
            _creaturesDictionary.Add(h1.HeroTeam, blueCreature);
            _creaturesDictionary.Add(h2.HeroTeam, redCreature);
        }

        public AbstractCreature NextCreature()
        {
            _currentHero = _currentHero.NextOrFirst();
            var currentTurn = _creaturesDictionary[_currentHero.Value.HeroTeam].NextOrFirst();
            while (currentTurn.Value.Status == CreatureStatus.Death)
            {
                currentTurn = currentTurn.NextOrFirst();
            }
            _creaturesDictionary[_currentHero.Value.HeroTeam] = currentTurn;
            return currentTurn.Value;
        }

    }

    static class CircularLinkedList
    {
        public static LinkedListNode<T> NextOrFirst<T>(this LinkedListNode<T> current)
        {
            return current.Next ?? current.List.First;
        }
    }

}
