using System;
using System.Collections.Generic;
using System.Globalization;

namespace TwistSort3D.Core
{
    /// <summary>
    /// Localización ES/EN. Todas las cadenas visibles del juego pasan por aquí.
    /// Añadir un idioma = añadir una columna al diccionario.
    /// </summary>
    public static class Loc
    {
        public static string Language { get; private set; } = "es";

        private static readonly Dictionary<string, (string es, string en)> Strings = new()
        {
            // ---------------------------------------------------------- general
            ["APP_TITLE"] = ("Twist & Sort 3D", "Twist & Sort 3D"),
            ["TAGLINE"] = ("Gira. Ordena. Relájate.", "Twist. Sort. Relax."),
            ["COINS"] = ("Monedas: {0}", "Coins: {0}"),
            ["BACK"] = ("VOLVER", "BACK"),
            ["CONTINUE"] = ("CONTINUAR", "CONTINUE"),
            ["ON"] = ("SÍ", "ON"),
            ["OFF"] = ("NO", "OFF"),
            ["OK"] = ("OK", "OK"),

            // ---------------------------------------------------------- menú
            ["MENU_PLAY"] = ("JUGAR", "PLAY"),
            ["MENU_LEVEL"] = ("Nivel {0}", "Level {0}"),
            ["MENU_DAILY"] = ("RETO DIARIO", "DAILY CHALLENGE"),
            ["MENU_MARATHON"] = ("MARATÓN", "MARATHON"),
            ["MENU_SETTINGS"] = ("AJUSTES", "SETTINGS"),
            ["MENU_SKINS"] = ("PERSONALIZAR", "CUSTOMIZE"),
            ["MENU_HELP"] = ("CÓMO JUGAR", "HOW TO PLAY"),
            ["MENU_BEST"] = ("Mejor nivel: {0}", "Best level: {0}"),

            // ---------------------------------------------------------- HUD
            ["HUD_LEVEL"] = ("NIVEL {0}", "LEVEL {0}"),
            ["HUD_MOVES"] = ("Movimientos: {0}", "Moves: {0}"),
            ["HUD_PAUSE"] = ("PAUSA", "PAUSED"),
            ["HUD_RESUME"] = ("REANUDAR", "RESUME"),
            ["HUD_RESTART"] = ("REINICIAR NIVEL", "RESTART LEVEL"),
            ["HUD_MENU"] = ("MENÚ PRINCIPAL", "MAIN MENU"),
            ["HUD_TRAY"] = ("BANDEJA", "TRAY"),
            ["HUD_HINT"] = ("PISTA", "HINT"),

            // ---------------------------------------------------------- victoria
            ["WIN_TITLE"] = ("¡NIVEL {0} COMPLETADO!", "LEVEL {0} COMPLETE!"),
            ["WIN_MOVES"] = ("Movimientos: {0}  ·  Óptimo: {1}", "Moves: {0}  ·  Optimal: {1}"),
            ["WIN_POINTS"] = ("Puntos: {0}", "Points: {0}"),
            ["WIN_REWARD"] = ("Recompensa: +{0} monedas", "Reward: +{0} coins"),
            ["WIN_NEXT"] = ("SIGUIENTE", "NEXT"),
            ["WIN_MENU"] = ("MENÚ", "MENU"),

            // ---------------------------------------------------------- derrota
            ["LOSE_TITLE"] = ("¡SE LLENÓ LA BANDEJA!", "THE TRAY IS FULL!"),
            ["LOSE_DESC"] = ("No quedan combinaciones posibles.", "No matches available."),
            ["LOSE_BOMB"] = ("¡La bomba explotó!", "The bomb exploded!"),
            ["LOSE_RETRY"] = ("REINTENTAR", "RETRY"),
            ["LOSE_TIP"] = ("Consejo: {0}", "Tip: {0}"),

            // ---------------------------------------------------------- herramientas
            ["TOOL_HAMMER"] = ("Martillo", "Hammer"),
            ["TOOL_SWAP"] = ("Intercambio", "Swap"),
            ["TOOL_EXPAND"] = ("Ampliar", "Expand"),
            ["TOOL_HINT"] = ("Pista", "Hint"),
            ["TOOL_FREEZE"] = ("Congelar", "Freeze"),
            ["TOOL_EMPTY"] = ("Sin usos", "No uses"),
            ["TOOL_USED"] = ("{0} usado", "{0} used"),
            ["TOOL_HAMMER_D"] = ("Quita un tornillo de la bandeja.", "Removes one bolt from the tray."),
            ["TOOL_SWAP_D"] = ("Intercambia dos tornillos de la bandeja.", "Swaps two tray bolts."),
            ["TOOL_EXPAND_D"] = ("Añade 2 ranuras a la bandeja.", "Adds 2 tray slots."),
            ["TOOL_HINT_D"] = ("Señala un tornillo seguro.", "Highlights a safe bolt."),
            ["TOOL_FREEZE_D"] = ("Libera espacio quitando un tornillo repetido.", "Frees space by removing a repeated bolt."),

            // ---------------------------------------------------------- obstáculos
            ["OB_DOUBLE"] = ("Tornillo doble", "Double bolt"),
            ["OB_DOUBLE_D"] = ("Ocupa dos ranuras de la bandeja.", "Takes two tray slots."),
            ["OB_WOOD"] = ("Tornillo atascado", "Stuck bolt"),
            ["OB_WOOD_D"] = ("Necesita dos toques para salir.", "Needs two taps to come out."),
            ["OB_ICE"] = ("Tornillo helado", "Iced bolt"),
            ["OB_ICE_D"] = ("Se libera tras quitar otros tornillos.", "Unlocks after removing other bolts."),
            ["OB_LINKED"] = ("Tornillo enlazado", "Linked bolt"),
            ["OB_LINKED_D"] = ("Al quitarlo, su pareja también cae.", "Removing it also drops its pair."),
            ["OB_LOCKED"] = ("Tornillo candado", "Locked bolt"),
            ["OB_LOCKED_D"] = ("Necesita que se quite antes la llave.", "Requires the key bolt first."),
            ["OB_SWIVEL"] = ("Placa giratoria", "Swivel plate"),
            ["OB_SWIVEL_D"] = ("Sus tornillos cambian de sitio.", "Its bolts change places."),
            ["OB_PAINT"] = ("Tornillo pintura", "Paint bolt"),
            ["OB_PAINT_D"] = ("Tiñe otro tornillo de la bandeja.", "Recolors another tray bolt."),
            ["OB_BOMB"] = ("Tornillo bomba", "Bomb bolt"),
            ["OB_BOMB_D"] = ("Explota si no lo quitas a tiempo.", "Explodes if not removed in time."),
            ["OB_NONE"] = ("Sin obstáculos", "No obstacles"),

            // ---------------------------------------------------------- ajustes
            ["SET_TITLE"] = ("AJUSTES", "SETTINGS"),
            ["SET_LANGUAGE"] = ("Idioma", "Language"),
            ["SET_MUSIC"] = ("Música", "Music"),
            ["SET_SFX"] = ("Efectos", "Sound effects"),
            ["SET_VIBRATION"] = ("Vibración", "Vibration"),
            ["SET_SYMBOLS"] = ("Símbolos de color", "Color symbols"),
            ["SET_MOTION"] = ("Reducir movimiento", "Reduce motion"),
            ["SET_QUALITY"] = ("Calidad gráfica", "Graphics quality"),
            ["SET_Q_LOW"] = ("Baja", "Low"),
            ["SET_Q_MED"] = ("Media", "Medium"),
            ["SET_Q_HIGH"] = ("Alta", "High"),
            ["SET_RESET"] = ("Borrar progreso", "Reset progress"),
            ["SET_RESET_Q"] = ("¿Seguro? Pulsa otra vez para borrar todo.", "Sure? Tap again to erase everything."),
            ["SET_RESET_DONE"] = ("Progreso borrado.", "Progress erased."),
            ["SET_STATS"] = ("Niveles ganados: {0}  ·  perdidos: {1}  ·  tornillos: {2}",
                              "Levels won: {0}  ·  lost: {1}  ·  bolts: {2}"),
            ["SET_CREDITS"] = ("Twist & Sort 3D — PixelBolt Games. Fuentes: Lilita One y Nunito (OFL).",
                               "Twist & Sort 3D — PixelBolt Games. Fonts: Lilita One and Nunito (OFL)."),

            // ---------------------------------------------------------- skins
            ["SK_TITLE"] = ("PERSONALIZAR", "CUSTOMIZE"),
            ["SK_SCREWS"] = ("TORNILLOS", "BOLTS"),
            ["SK_TRAYS"] = ("BANDEJAS", "TRAYS"),
            ["SK_EQUIP"] = ("EQUIPAR", "EQUIP"),
            ["SK_EQUIPPED"] = ("EQUIPADO", "EQUIPPED"),
            ["SK_LOCKED"] = ("Nivel {0}", "Level {0}"),
            ["SK_BUY"] = ("{0} monedas", "{0} coins"),
            ["SK_STANDARD"] = ("Clásico", "Classic"),
            ["SK_HEX"] = ("Hexagonal", "Hex"),
            ["SK_ROUND"] = ("Redondo", "Round"),
            ["SK_GOLD"] = ("Dorado", "Gold"),
            ["SK_WOOD"] = ("Madera", "Wood"),
            ["SK_METAL"] = ("Metal", "Metal"),
            ["SK_CANDY"] = ("Caramelo", "Candy"),

            // ---------------------------------------------------------- daily / marathon
            ["DAILY_TITLE"] = ("RETO DIARIO", "DAILY CHALLENGE"),
            ["DAILY_DESC"] = ("Un nivel único cada día. Todos juegan el mismo.", "A unique level every day. Everyone plays the same."),
            ["DAILY_PLAY"] = ("JUGAR RETO", "PLAY CHALLENGE"),
            ["DAILY_DONE"] = ("Ya lo has completado hoy. ¡Vuelve mañana!", "Already completed today. Come back tomorrow!"),
            ["MARATHON_TITLE"] = ("MARATÓN", "MARATHON"),
            ["MARATHON_DESC"] = ("Empiezas con 5 ranuras. Cada 10 niveles ganas una.", "Start with 5 slots. Every 10 levels you gain one."),
            ["MARATHON_PLAY"] = ("EMPEZAR", "START"),
            ["MARATHON_LEVEL"] = ("Nivel {0} de la maratón", "Marathon level {0}"),

            // ---------------------------------------------------------- ayuda
            ["HELP_TITLE"] = ("CÓMO JUGAR", "HOW TO PLAY"),
            ["HELP_STEP1_T"] = ("1. Desatornilla", "1. Unscrew"),
            ["HELP_STEP1_D"] = ("Toca un tornillo libre para quitarlo de la placa.", "Tap a free bolt to remove it from the plate."),
            ["HELP_STEP2_T"] = ("2. Junta tres", "2. Match three"),
            ["HELP_STEP2_D"] = ("Tres tornillos del mismo color en la bandeja desaparecen.", "Three bolts of the same color in the tray vanish."),
            ["HELP_STEP3_T"] = ("3. Vacía la estructura", "3. Clear the structure"),
            ["HELP_STEP3_D"] = ("Cuando una placa se queda sin tornillos, cae.", "When a plate has no bolts left, it falls."),
            ["HELP_STEP4_T"] = ("4. No llenes la bandeja", "4. Don't fill the tray"),
            ["HELP_STEP4_D"] = ("Si se llena sin trío, pierdes. Usa las herramientas.", "If it fills without a match, you lose. Use tools."),
            ["HELP_TOOLS"] = ("HERRAMIENTAS", "TOOLS"),
            ["HELP_OBSTACLES"] = ("OBSTÁCULOS", "OBSTACLES"),

            // ---------------------------------------------------------- mensajes
            ["MSG_BLOCKED"] = ("Ese tornillo está bloqueado", "That bolt is blocked"),
            ["MSG_ICE"] = ("Rompé el hielo primero", "Break the ice first"),
            ["MSG_LOCKED"] = ("Necesitás la llave", "You need the key"),
            ["MSG_WOOD"] = ("¡Casi! Otro toque", "Almost! One more tap"),
            ["MSG_TRAY_FULL"] = ("¡La bandeja se llenó!", "The tray is full!"),
            ["MSG_NO_TOOL"] = ("No te quedan usos", "No uses left"),
            ["MSG_DAILY_DONE"] = ("Ya jugaste el reto de hoy", "You already played today's challenge"),
            ["MSG_BOMB"] = ("¡La bomba va a explotar!", "The bomb is about to blow!"),
            ["MSG_HINT"] = ("Prueba con el tornillo marcado", "Try the highlighted bolt"),
            ["MSG_NEW_LEVEL"] = ("¡Nuevo nivel desbloqueado!", "New level unlocked!"),
            ["MSG_STARS"] = ("{0} de 3 estrellas", "{0} of 3 stars"),

            // ---------------------------------------------------------- tutorial
            ["TUT_WELCOME_T"] = ("¡BIENVENIDO!", "WELCOME!"),
            ["TUT_WELCOME"] = ("Toca un tornillo libre para desatornillarlo. Las flechas te indican dónde.",
                                "Tap a free bolt to unscrew it. The arrows show you where."),
            ["TUT_TRAY_T"] = ("LA BANDEJA", "THE TRAY"),
            ["TUT_TRAY"] = ("Los tornillos que quitas caen aquí. Junta 3 del mismo color para eliminarlos.",
                             "Bolts you remove drop here. Match 3 of the same color to clear them."),
            ["TUT_PLATE_T"] = ("LAS PLACAS", "THE PLATES"),
            ["TUT_PLATE"] = ("Cuando una placa se queda sin tornillos, cae y deja ver las de abajo.",
                              "When a plate runs out of bolts, it falls and reveals the ones below."),
            ["TUT_TOOLS_T"] = ("HERRAMIENTAS", "TOOLS"),
            ["TUT_TOOLS"] = ("Si te bloqueas, usa martillo, intercambio, ampliar, pista o congelar. Se ganan cada 3 niveles.",
                              "If you get stuck, use hammer, swap, expand, hint or freeze. You earn one every 3 levels."),
            ["TUT_DONE_T"] = ("¡YA SABES JUGAR!", "YOU'VE GOT IT!"),
            ["TUT_DONE"] = ("Vacía toda la estructura sin llenar la bandeja. ¡Buena suerte!",
                             "Clear the whole structure without filling the tray. Good luck!"),
            ["TUT_NEXT"] = ("SIGUIENTE", "NEXT"),
            ["TUT_OK"] = ("ENTENDIDO", "GOT IT"),
            ["TUT_PLAY"] = ("A JUGAR", "PLAY"),
            ["TUT_SKIP"] = ("SALTAR", "SKIP"),
        };

        public static void SetLanguage(string code)
        {
            Language = code == "en" ? "en" : "es";
        }

        public static void DetectFromSystem()
        {
            var c = CultureInfo.CurrentUICulture;
            SetLanguage(c.TwoLetterISOLanguageName == "es" ? "es" : "en");
        }

        public static string T(string key)
        {
            if (!Strings.TryGetValue(key, out var v)) return key;
            return Language == "en" ? v.en : v.es;
        }

        public static string T(string key, params object[] args)
        {
            var s = T(key);
            try { return string.Format(s, args); }
            catch { return s; }
        }

        public static string ColorName(int i)
        {
            var arr = Language == "en" ? Config.ColorNamesEn : Config.ColorNamesEs;
            if (i < 0 || i >= arr.Length) return "?";
            return arr[i];
        }
    }
}
