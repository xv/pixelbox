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
    /// Gets or sets the key binding used to expand the area of the screen being
    /// magnified.
    /// </summary>
    public required KeyBinding ExpandView
    { get; set; }

    /// <summary>
    /// Gets or sets the key binding used to shrink the area of the screen being
    /// magnified.
    /// </summary>
    public required KeyBinding ShrinkView
    { get; set; }

    /// <summary>
    /// Gets or sets the key binding used to zoom in the magnified view.
    /// </summary>
    public required KeyBinding ZoomIn
    { get; set; }

    /// <summary>
    /// Gets or sets the key binding used to zoom out the magnified view.
    /// </summary>
    public required KeyBinding ZoomOut
    { get; set; }

    /// <summary>
    /// Gets or sets the key binding used to increase the size of color sampling
    /// area.
    /// </summary>
    public required KeyBinding IncreaseColorSamplingArea
    { get; set; }

    /// <summary>
    /// Gets or sets the key binding used to decrease the size of color sampling
    /// area.
    /// </summary>
    public required KeyBinding DecreaseColorSamplingArea
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
        ExpandView = Bind(PixelMagnifierUICommands.ExpandView, Key.OemPlus, ModifierKeys.Shift),
        ShrinkView = Bind(PixelMagnifierUICommands.ShrinkView, Key.OemMinus, ModifierKeys.Shift),
        ZoomIn = Bind(PixelMagnifierUICommands.ZoomIn, Key.OemPlus, ModifierKeys.Control),
        ZoomOut = Bind(PixelMagnifierUICommands.ZoomOut, Key.OemMinus, ModifierKeys.Control),
        IncreaseColorSamplingArea = Bind(PixelMagnifierUICommands.IncreaseColorSamplingArea, Key.OemPlus),
        DecreaseColorSamplingArea = Bind(PixelMagnifierUICommands.IncreaseColorSamplingArea, Key.OemMinus)
    };
}