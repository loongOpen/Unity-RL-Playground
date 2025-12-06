using System;
using System.Collections.Generic;
using UnityEngine;

namespace GifImporter
{
    [CreateAssetMenu(fileName = "Gif", menuName = "GifImporter/Gif", order = 0)]
    public class Gif : ScriptableObject
    {
        public List<GifFrame> Frames;
    }

    [Serializable]
    public class GifFrame
    {
        public int    DelayInMs;
        public Sprite Sprite;
    }
}