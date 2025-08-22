using System;
using UnityEngine;

namespace NekoLib.Utilities
{
    public static class Swatch
    {
        #region Gray Colors
        /// <summary>
        /// Dark Gray Color (Gray).
        /// </summary>
        public static Color DG
        {
            get
            {
                return new Color(0.478f, 0.478f, 0.478f, 1.0f);
            }
        }

        /// <summary>
        /// Medium Gray Color (Gray).
        /// </summary>
        public static Color MG
        {
            get
            {
                return new Color(0.722f, 0.722f, 0.722f, 1.0f);
            }
        }

        /// <summary>
        /// Pale Gray Color (Gray).
        /// </summary>
        public static Color PG
        {
            get
            {
                return new Color(0.878f, 0.878f, 0.878f, 1.0f);
            }
        }
        #endregion

        #region Yellow Colors
        /// <summary>
        /// Golden Amber Color (Yellow).
        /// </summary>
        public static Color GA
        {
            get
            {
                return new Color(1.0f, 0.757f, 0.027f, 1.0f);
            }
        }

        /// <summary>
        /// Sunny Gold Color (Yellow).
        /// </summary>
        public static Color SG
        {
            get
            {
                return new Color(1.0f, 0.878f, 0.400f, 1.0f);
            }
        }

        /// <summary>
        /// Vivid Yellow Color (Yellow).
        /// </summary>
        public static Color VY
        {
            get
            {
                return new Color(1.0f, 0.925f, 0.251f, 1.0f);
            }
        }

        /// <summary>
        /// Cream Gold Color (Yellow).
        /// </summary>
        public static Color CG
        {
            get
            {
                return new Color(1.0f, 0.980f, 0.804f, 1.0f);
            }
        }
        #endregion

        #region Red Colors
        /// <summary>
        /// Candy Red Color (Red).
        /// </summary>
        public static Color CR
        {
            get
            {
                return new Color(1.0f, 0.620f, 0.620f, 1.0f);
            }
        }

        /// <summary>
        /// Coral Color (Red).
        /// </summary>
        public static Color CO
        {
            get
            {
                return new Color(1.0f, 0.420f, 0.420f, 1.0f);
            }
        }

        /// <summary>
        /// Vibrant Red Color (Red).
        /// </summary>
        public static Color VR
        {
            get
            {
                return new Color(1.0f, 0.310f, 0.310f, 1.0f);
            }
        }
        #endregion

        #region Green Colors
        /// <summary>
        /// Deep Emerald Color (Green).
        /// </summary>
        public static Color DE
        {
            get
            {
                return new Color(0.290f, 0.686f, 0.314f, 1.0f);
            }
        }

        /// <summary>
        /// Light Emerald Color (Green).
        /// </summary>
        public static Color LE
        {
            get
            {
                return new Color(0.506f, 0.780f, 0.518f, 1.0f);
            }
        }

        /// <summary>
        /// Mint Emerald Color (Green).
        /// </summary>
        public static Color ME
        {
            get
            {
                return new Color(0.647f, 0.839f, 0.655f, 1.0f);
            }
        }
        #endregion

        #region Blue Colors
        /// <summary>
        /// Azure Teal Color (Blue).
        /// </summary>
        public static Color AT
        {
            get
            {
                return new Color(0.0f, 0.592f, 0.655f, 1.0f);
            }
        }

        /// <summary>
        /// Vibrant Cyan Color (Blue).
        /// </summary>
        public static Color VC
        {
            get
            {
                return new Color(0.0f, 0.737f, 0.831f, 1.0f);
            }
        }

        /// <summary>
        /// Aqua Cyan Color (Blue).
        /// </summary>
        public static Color AC
        {
            get
            {
                return new Color(0.302f, 0.816f, 0.882f, 1.0f);
            }
        }
        #endregion

        #region Purple Colors
        /// <summary>
        /// Royal Purple Color (Purple).
        /// </summary>
        public static Color RP
        {
            get
            {
                return new Color(0.557f, 0.141f, 0.667f, 1.0f);
            }
        }

        /// <summary>
        /// Amethyst Color (Purple).
        /// </summary>
        public static Color AM
        {
            get
            {
                return new Color(0.671f, 0.278f, 0.737f, 1.0f);
            }
        }

        /// <summary>
        /// Lavender Color (Purple).
        /// </summary>
        public static Color LA
        {
            get
            {
                return new Color(0.808f, 0.576f, 0.847f, 1.0f);
            }
        }
        #endregion

        #region Orange Colors
        /// <summary>
        /// Burnt Orange Color (Orange).
        /// </summary>
        public static Color BO
        {
            get
            {
                return new Color(0.902f, 0.318f, 0.0f, 1.0f);
            }
        }

        /// <summary>
        /// Pumpkin Orange Color (Orange).
        /// </summary>
        public static Color PO
        {
            get
            {
                return new Color(0.961f, 0.486f, 0.0f, 1.0f);
            }
        }

        /// <summary>
        /// Light Orange Color (Orange).
        /// </summary>
        public static Color LO
        {
            get
            {
                return new Color(1.0f, 0.800f, 0.502f, 1.0f);
            }
        }
        #endregion
    }

    [Obsolete("Use Swatch instead.")]
    public static class Palette
    {

    }
}
