using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundSystem
{


    public class BGMData : ScriptableObject
    {
        [field: SerializeField]
        public string ID { get; private set; }
        [field: SerializeField]
        public string Title { get; private set; }
        [field: SerializeField]
        public string File { get; private set; }
        [field: SerializeField]
        public string Composer { get; private set; }
        [field: SerializeField]
        public string Comment { get; private set; }

        [field: SerializeField]
        public bool hasLoop { get; private set; }
        [field: SerializeField]
        public int LoopStart { get; private set; }
        [field: SerializeField]
        public int LoopLength { get; private set; }

        [field: SerializeField]
        public AudioClip Audio { get; private set; }

        public void Init(string id, string title, string file, string composer, string comment, bool hasloop, int loop_start, int loop_length, AudioClip clip)
        {
            ID = id;
            File = file;
            Title = title;
            Composer = composer;
            Comment = comment;
            hasLoop = hasloop;
            LoopStart = loop_start;
            LoopLength = loop_length;
            Audio = clip;
        }
    }



}