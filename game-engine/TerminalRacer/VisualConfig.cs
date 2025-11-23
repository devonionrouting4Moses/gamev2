using System;
using System.Collections.Generic;

namespace TerminalRacer
{
    /// <summary>
    /// Visual Configuration for Terminal Racer
    /// Defines all visual assets, colors, and rendering styles
    /// Inspired by modern mobile racing games
    /// </summary>
    public static class VisualConfig
    {
        // ===== CAR DESIGNS =====
        public static class CarDesigns
        {
            public const string BLUE_SPORTS = "┌─┐\n│●│\n└─┘";
            public const string RED_POLICE = "┌─┐\n│🚨│\n└─┘";
            public const string YELLOW_RACER = "┌─┐\n│⚡│\n└─┘";
            public const string GREEN_TRUCK = "┌───┐\n│ G │\n└───┘";
            public const string ORANGE_TAXI = "┌─┐\n│T│\n└─┘";
            public const string GRAY_VAN = "┌───┐\n│ V │\n└───┘";
            public const string MAGENTA_MUSCLE = "┌─┐\n│M│\n└─┘";
            public const string CYAN_CONVERTIBLE = "┌─┐\n│C│\n└─┘";
            public const string WHITE_LIMO = "┌─────┐\n│ LIM │\n└─────┘";
            public const string BOSS_CAR = "╔═══╗\n║ B ║\n╚═══╝";
        }

        // ===== POWERUP VISUALS =====
        public static class PowerupVisuals
        {
            public const string CONE = "🚧";
            public const string OIL = "💧";
            public const string BOOST = "⚡";
            public const string STAR = "⭐";
            public const string MAGNET = "🧲";
            public const string CLOCK = "🕐";
        }

        // ===== ENVIRONMENT ASSETS =====
        public static class EnvironmentAssets
        {
            public const string TREE = "🌲";
            public const string CACTUS = "🌵";
            public const string MOUNTAIN = "⛰";
            public const string GLASS_BUILDING = "🏢";
            public const string CONCRETE_BUILDING = "🏭";
            public const string BRICK_BUILDING = "🏠";
        }

        // ===== ROAD MARKINGS =====
        public static class RoadMarkings
        {
            public const string SOLID_LINE = "▓";
            public const string DASHED_LINE = "┆";
            public const string LANE_DIVIDER = "║";
            public const string LANE_DIVIDER_CURVED = "┃";
            public const string LANE_DIVIDER_SPARSE = "┆";
        }

        // ===== HUD ELEMENTS =====
        public static class HUDElements
        {
            public const string SCORE_ICON = "⭐";
            public const string HEALTH_ICON = "❤";
            public const string SPEED_ICON = "🏎";
            public const string COMBO_ICON = "🔥";
            public const string BOOST_ICON = "⚡";
            public const string SHIELD_ICON = "🛡";
            public const string STAR_ICON = "⭐";
            public const string MAGNET_ICON = "🧲";
        }

        // ===== LANE CONFIGURATIONS =====
        public static class LaneConfigs
        {
            public class LaneConfig
            {
                public int LaneCount { get; set; }
                public int LaneWidth { get; set; }
                public string MarkerStyle { get; set; }
            }

            public static readonly LaneConfig HIGHWAY = new LaneConfig
            {
                LaneCount = 3,
                LaneWidth = 8,
                MarkerStyle = "┆"
            };

            public static readonly LaneConfig CITY = new LaneConfig
            {
                LaneCount = 4,
                LaneWidth = 6,
                MarkerStyle = "║"
            };

            public static readonly LaneConfig MOUNTAIN = new LaneConfig
            {
                LaneCount = 3,
                LaneWidth = 8,
                MarkerStyle = "┃"
            };

            public static readonly LaneConfig DESERT = new LaneConfig
            {
                LaneCount = 2,
                LaneWidth = 12,
                MarkerStyle = "┆"
            };

            public static readonly LaneConfig TUNNEL = new LaneConfig
            {
                LaneCount = 3,
                LaneWidth = 8,
                MarkerStyle = "┃"
            };
        }

        // ===== COLOR CODES =====
        public static class ColorCodes
        {
            // ANSI color codes for terminal
            public const string RESET = "\u001b[0m";
            public const string BOLD = "\u001b[1m";
            public const string DIM = "\u001b[2m";
            public const string ITALIC = "\u001b[3m";
            public const string UNDERLINE = "\u001b[4m";

            // Foreground colors
            public const string BLACK = "\u001b[30m";
            public const string RED = "\u001b[31m";
            public const string GREEN = "\u001b[32m";
            public const string YELLOW = "\u001b[33m";
            public const string BLUE = "\u001b[34m";
            public const string MAGENTA = "\u001b[35m";
            public const string CYAN = "\u001b[36m";
            public const string WHITE = "\u001b[37m";

            // Bright colors
            public const string BRIGHT_RED = "\u001b[91m";
            public const string BRIGHT_GREEN = "\u001b[92m";
            public const string BRIGHT_YELLOW = "\u001b[93m";
            public const string BRIGHT_BLUE = "\u001b[94m";
            public const string BRIGHT_MAGENTA = "\u001b[95m";
            public const string BRIGHT_CYAN = "\u001b[96m";
            public const string BRIGHT_WHITE = "\u001b[97m";

            // Background colors
            public const string BG_BLACK = "\u001b[40m";
            public const string BG_RED = "\u001b[41m";
            public const string BG_GREEN = "\u001b[42m";
            public const string BG_YELLOW = "\u001b[43m";
            public const string BG_BLUE = "\u001b[44m";
            public const string BG_MAGENTA = "\u001b[45m";
            public const string BG_CYAN = "\u001b[46m";
            public const string BG_WHITE = "\u001b[47m";
        }

        // ===== TRACK PALETTES =====
        public static class TrackPalettes
        {
            public class Palette
            {
                public string Primary { get; set; }
                public string Secondary { get; set; }
                public string Accent { get; set; }
                public string Background { get; set; }
                public string Text { get; set; }
            }

            public static readonly Palette HIGHWAY = new Palette
            {
                Primary = ColorCodes.CYAN,
                Secondary = ColorCodes.BRIGHT_CYAN,
                Accent = ColorCodes.YELLOW,
                Background = ColorCodes.BLACK,
                Text = ColorCodes.WHITE
            };

            public static readonly Palette CITY = new Palette
            {
                Primary = ColorCodes.BLUE,
                Secondary = ColorCodes.BRIGHT_BLUE,
                Accent = ColorCodes.YELLOW,
                Background = ColorCodes.BLACK,
                Text = ColorCodes.WHITE
            };

            public static readonly Palette MOUNTAIN = new Palette
            {
                Primary = ColorCodes.GREEN,
                Secondary = ColorCodes.BRIGHT_GREEN,
                Accent = ColorCodes.WHITE,
                Background = ColorCodes.BLACK,
                Text = ColorCodes.WHITE
            };

            public static readonly Palette DESERT = new Palette
            {
                Primary = ColorCodes.YELLOW,
                Secondary = ColorCodes.BRIGHT_YELLOW,
                Accent = ColorCodes.RED,
                Background = ColorCodes.BLACK,
                Text = ColorCodes.WHITE
            };

            public static readonly Palette TUNNEL = new Palette
            {
                Primary = ColorCodes.CYAN,
                Secondary = ColorCodes.BRIGHT_CYAN,
                Accent = ColorCodes.WHITE,
                Background = ColorCodes.BLACK,
                Text = ColorCodes.WHITE
            };
        }

        // ===== ANIMATION FRAMES =====
        public static class AnimationFrames
        {
            public static readonly string[] WHEEL_ANIMATION = { "◐", "◓", "◑", "◒" };
            public static readonly string[] BOOST_ANIMATION = { "🔥", "💥", "⚡" };
            public static readonly string[] SHIELD_ANIMATION = { "◯", "◉" };
            public static readonly string[] RAIN_ANIMATION = { "·", "·", "·" };
        }

        // ===== PARTICLE EFFECTS =====
        public static class ParticleEffects
        {
            public const string BOOST_PARTICLE = "✦";
            public const string CRASH_PARTICLE = "✕";
            public const string DUST_PARTICLE = "·";
            public const string SPARK_PARTICLE = "✧";
        }

        // ===== WEATHER EFFECTS =====
        public static class WeatherEffects
        {
            public const string RAIN_DROP = "·";
            public const string FOG_PARTICLE = "░";
            public const string SNOW_FLAKE = "❄";
            public const string LIGHTNING = "⚡";
        }

        // ===== UTILITY FUNCTIONS =====
        public static string GetCarColor(int carType)
        {
            return carType switch
            {
                0 => ColorCodes.BLUE,           // Blue sports
                1 => ColorCodes.RED,            // Red police
                2 => ColorCodes.YELLOW,         // Yellow racer
                3 => ColorCodes.GREEN,          // Green truck
                4 => ColorCodes.BRIGHT_YELLOW,  // Orange taxi
                5 => ColorCodes.BRIGHT_WHITE,   // Gray van
                6 => ColorCodes.MAGENTA,        // Magenta muscle
                7 => ColorCodes.CYAN,           // Cyan convertible
                8 => ColorCodes.WHITE,          // White limo
                _ => ColorCodes.BRIGHT_WHITE
            };
        }

        public static string GetPowerupColor(int powerupType)
        {
            return powerupType switch
            {
                0 => ColorCodes.YELLOW,         // Cone
                1 => ColorCodes.BLUE,           // Oil
                2 => ColorCodes.MAGENTA,        // Boost
                3 => ColorCodes.YELLOW,         // Star
                4 => ColorCodes.RED,            // Magnet
                5 => ColorCodes.CYAN,           // Clock
                _ => ColorCodes.WHITE
            };
        }

        public static string GetTrackColor(int trackType)
        {
            return trackType switch
            {
                0 => ColorCodes.CYAN,           // Highway
                1 => ColorCodes.BLUE,           // City
                2 => ColorCodes.GREEN,          // Mountain
                3 => ColorCodes.YELLOW,         // Desert
                4 => ColorCodes.BRIGHT_CYAN,    // Tunnel
                _ => ColorCodes.WHITE
            };
        }

        public static string GetWeatherColor(int weatherType)
        {
            return weatherType switch
            {
                0 => ColorCodes.CYAN,           // Clear
                1 => ColorCodes.BLUE,           // Rain
                2 => ColorCodes.BRIGHT_WHITE,   // Fog
                3 => ColorCodes.BRIGHT_CYAN,    // Night
                _ => ColorCodes.WHITE
            };
        }

        public static string ColorizeText(string text, string color)
        {
            return $"{color}{text}{ColorCodes.RESET}";
        }

        public static string BoldText(string text)
        {
            return $"{ColorCodes.BOLD}{text}{ColorCodes.RESET}";
        }

        public static string DimText(string text)
        {
            return $"{ColorCodes.DIM}{text}{ColorCodes.RESET}";
        }
    }
}
