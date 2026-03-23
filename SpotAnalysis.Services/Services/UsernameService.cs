namespace SpotAnalysis.Services.Services;

public class UsernameService : IUsernameService
{
    private static readonly Random random = new();


    private static readonly string[] Adjectives =
    {
        "Happy", "Sad", "Funny", "Serious", "Silly", "Crazy", "Lazy", "Smart",  
        "Brave", "Flaming", "Frosty", "Grumpy", "Jolly", "Lucky", "Mighty", "Proud",
        "Dusty", "Hyper", "Tiny", "Massive", "Weird", "Scary", "Friendly", "Dark",
        "Periodic", "Covalent", "Ionic", "Resonant", "Unstable", "Metastable",
        "Quantum", "Relativistic", "Subatomic", "Cosmic", "Galactic", "Nebular",
        "Red", "Blue", "Green", "Yellow", "Purple", "Orange", "Black", "Silver",
        "Violet", "Indigo", "Magenta", "Cyan", "Sweet", "Sour", "Bitter", "Salty",
        "Golden", "Crimson", "Azure", "Emerald", "Sapphire", "Ruby", "Onyx", "Ivory",
        "Flying", "Sunny", "Stormy", "Windy", "Rainy", "Snowy", "Cloudy", "Foggy", "Icy"

    };

    private static readonly string[] Nouns =
    {
        "Bit", "Dog", "Cat", "Bird", "Fish", "Elephant", "Lion", "Tiger", "Bear", "Monkey", "Snake",
        "Eagle", "Shark", "Dolphin", "Whale", "Wolf", "Fox", "Rabbit", "Frog", "Turtle",
        "Hertz", "Proton", "Quark", "Polar", "Electron", "Titan", "Neon", "Ion", "Photon", "Plasma", 
        "Nebula", "Comet", "Asteroid", "Meteor", "Galaxy", "Star", "Koala", "Giraffe", 
        "Zebra", "Cheetah", "Leopard", "Panda", "Sloth", "Otter", "Penguin", "Raccoon", "Squirrel", 
        "Hamster", "Llama", "Alpaca", "Cobra", "Viper", "Python", "Anaconda", "Cheetah", "Panther", 
        "Jaguar", "Lynx", "Bobcat", "Parrot", "Toucan", "Flamingo", "Owl", "Hawk", "Falcon", 
        "Vulture", "Crow", "Raven", "Sparrow", "Bluejay", "Woodpecker", "Cardinal", "Swan", "Duck", 
        "Goose", "Turkey"
    };
  

    public string Generate()
    {
        var adjective = Adjectives[random.Next(Adjectives.Length)];
        var noun = Nouns[random.Next(Nouns.Length)];

        return $"{adjective} {noun}";
    }
}