using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TeamPicker.Models
{
    [Serializable]
    public class TeamModel
    {
        public ulong ID { get; set; }
        public string Name { get; set; } = "";
        public string DefaultKit { get; set; } = "";
        public SpawnCoordinates Spawn { get; set; } = new SpawnCoordinates { X = 500.0f, Y = 500.0f, Z= 500.0f };
    }

    public class SpawnCoordinates
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}
