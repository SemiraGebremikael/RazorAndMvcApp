using System;
namespace Models
{
    public enum enAnimalKind { Dog, Cat, Rabbit, Fish, Bird };
    public enum enAnimalMood { Happy, Hungry, Lazy, Sulky, Buzy, Sleepy };
    public interface IPet
	{
        public Guid PetId { get; set; }

        public enAnimalKind Kind { get; set; }
        public enAnimalMood Mood { get; set; }
        public string Name { get; set; }

        public IFriend Friend { get; set; }
    }
}

