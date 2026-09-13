using System.Windows.Input;

namespace Frosty.Core;

public static class TypeExtensions
{
    /// <summary>
    /// Registers key bindings to a <see cref="CommandBindingCollection"/> via a dictionary of <see cref="KeyGesture"/>/<see cref="ExecutedRoutedEventHandler"/> pairings.
    /// </summary>
    /// <param name="gestureHandlerPairings">The dictionary of key/handler pairings to be used.</param>
    public static void RegisterKeyBindings(this CommandBindingCollection commandBindingCollection, Dictionary<KeyGesture, ExecutedRoutedEventHandler> gestureHandlerPairings)
    {
        // create a RoutedCommand for storing the created command instances of the iteration
        RoutedCommand currentCommand;

        for (int i = 0; i < gestureHandlerPairings.Count; i++)
        {
            currentCommand = new RoutedCommand();

            // register the iteration's selected key to the new command
            currentCommand.InputGestures.Add(gestureHandlerPairings.Keys.ElementAt(i));

            // register a CommandBinding with the use of the newly-created RoutedCommand
            commandBindingCollection.Add(new CommandBinding(currentCommand, gestureHandlerPairings.Values.ElementAt(i)));
        }
    }
}