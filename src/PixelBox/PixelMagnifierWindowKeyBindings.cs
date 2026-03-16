// Copyright 2026 Jad Altahan <xv.git@aol.com>
// SPDX-License-Identifier: MIT

using System.Windows.Input;

namespace PixelBox;

/// <summary>
/// Represents key bindings for <see cref="PixelMagnifierWindow"/>.
/// </summary>
public class PixelMagnifierWindowKeyBindings
{
    /// <summary>
    /// Gets or sets the key binding used to toggle the visibility of the grid.
    /// </summary>
    public required KeyBinding ToggleGrid
    { get; set; }

    /// <summary>
    /// Gets or sets the key binding used increase the size of the pixel grid.
    /// </summary>
    public required KeyBinding IncreaseGridSize
    { get; set; }

    /// <summary>
    /// Gets or sets the key binding used decrease the size of the pixel grid.
    /// </summary>
    public required KeyBinding DecreaseGridSize
    { get; set; }

    /// <summary>
    /// Gets or sets the key binding used to increase the size of pixel cells in
    /// the magnified view.
    /// </summary>
    public required KeyBinding IncreasePixelSize
    { get; set; }

    /// <summary>
    /// Gets or sets the key binding used to decrease the size of pixel cells in
    /// the magnified view.
    /// </summary>
    public required KeyBinding DecreasePixelSize
    { get; set; }

    /// <summary>
    /// Gets or sets the key binding used to increase the size of color sampler.
    /// </summary>
    public required KeyBinding IncreaseColorSamplerSize
    { get; set; }

    /// <summary>
    /// Gets or sets the key binding used to decrease the size of color sampler.
    /// </summary>
    public required KeyBinding DecreaseColorSamplerSize
    { get; set; }

    /// <summary>
    /// Gets or sets the key binding used to confirm the selection of the pixel
    /// and close the magnifier window.
    /// </summary>
    public required KeyBinding Close
    { get; set; }

    /// <summary>
    /// Creates a new key binding to associate with the specified command.
    /// </summary>
    /// 
    /// <param name="command">
    /// The command to associate the key binding with.
    /// </param>
    /// 
    /// <param name="key">
    /// The key to associate with the command.
    /// </param>
    /// 
    /// <param name="mod">
    /// The modifier keys to associate with the command.
    /// </param>
    /// 
    /// <returns>
    /// A new <see cref="KeyBinding"/> instance associated with
    /// <paramref name="command"/>.
    /// </returns>
    private static KeyBinding Bind(ICommand command, Key key, ModifierKeys mod = ModifierKeys.None) => new()
    {
        Command = command,
        Key = key,
        Modifiers = mod
    };

    public static PixelMagnifierWindowKeyBindings CreateDefault() => new()
    {
        ToggleGrid = Bind(PixelMagnifierUICommands.ToggleGrid, Key.G),
        IncreaseGridSize = Bind(PixelMagnifierUICommands.IncreaseGridSize, Key.OemPlus, ModifierKeys.Shift),
        DecreaseGridSize = Bind(PixelMagnifierUICommands.DecreaseGridSize, Key.OemMinus, ModifierKeys.Shift),
        IncreasePixelSize = Bind(PixelMagnifierUICommands.IncreasePixelSize, Key.OemPlus, ModifierKeys.Control),
        DecreasePixelSize = Bind(PixelMagnifierUICommands.DecreasePixelSize, Key.OemMinus, ModifierKeys.Control),
        IncreaseColorSamplerSize = Bind(PixelMagnifierUICommands.IncreaseColorSamplerSize, Key.OemPlus),
        DecreaseColorSamplerSize = Bind(PixelMagnifierUICommands.DecreaseColorSamplerSize, Key.OemMinus),
        Close = Bind(PixelMagnifierUICommands.Close, Key.Enter)
    };
}