// Copyright 2025 Jad Altahan <xv.git@aol.com>
// SPDX-License-Identifier: MIT

namespace PixelBox.Resources;

internal class ResourceUris
{
    private const string BaseUri =
        "pack://application:,,,/PixelBox;component/Resources/";

    public sealed class Cursors
    {
        public static readonly Uri
            Cross = new($"{BaseUri}Cursors/Cross.cur"),
            SimpleInvertedCross = new($"{BaseUri}Cursors/simple_inverted_cross.cur");
    }

    public sealed class Sounds
    {
        public static readonly Uri
            Pop = new($"{BaseUri}Sounds/ui_pop.wav"),
            Click = new($"{BaseUri}Sounds/ui_click.wav"),
            Notify = new($"{BaseUri}Sounds/ui_notify.wav");
    }
}