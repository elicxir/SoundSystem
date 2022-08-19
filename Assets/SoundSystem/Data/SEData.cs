using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundSystem
{
    public class SEData : ScriptableObject
    {
        [field: SerializeField]
        public string ID { get; private set; }

        [field: SerializeField]
        public string File { get; private set; }

        [field: SerializeField]
        public AudioClip Audio { get; private set; }

        public void Init(string id, string file, AudioClip clip)
        {
            ID = id;
            File = file;
            Audio = clip;
        }
    }

}