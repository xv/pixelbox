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
}