using MonoSync.Attributes;
using MonoSync.Collections;
using PropertyChanged;

namespace MonoSync.Sample.Tweening
{
    [AddINotifyPropertyChangedInterface]
    public class Map
    {
        public Map() : this(new ObservableDictionary<int, Player>())
        {
        }

        /// <summary>
        ///     Properties can be accessed during construction using parameters.
        ///     The parameter name should be camelCase or Marked with the <see cref="SyncConstructorParameterAttribute" />.
        ///     MonoSync will use the default constructor if no <see cref="SyncConstructorAttribute" /> Marked constructor is
        ///     provided.
        ///     Properties that do not occur in the constructor parameter will be synchronized after the constructor call.
        /// </summary>
        /// <param name="players"></param>
        [SyncConstructor]
        public Map(ObservableDictionary<int, Player> players)
        {
            Players = players;
        }

        [Sync] public ObservableDictionary<int, Player> Players { get; set; }
    }
}